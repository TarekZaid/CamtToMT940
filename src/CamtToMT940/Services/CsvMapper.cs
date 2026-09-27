using System;
using System.Globalization;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class CsvMapper
    {
        private readonly CultureInfo _german = CultureInfo.GetCultureInfo("de-DE");
        private readonly CultureInfo _invariant = CultureInfo.InvariantCulture;

        public BankTransaction? Map(string[] row, CsvTemplate template)
        {
            if (row == null || row.Length == 0) return null;

            // Status-Filter (DKB: "Gebucht", andere: leer)
            if (template.Columns.Status.HasValue)
            {
                var status = GetField(row, template.Columns.Status.Value);
                if (!string.IsNullOrWhiteSpace(status) &&
                    !status.Equals("Gebucht", StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            var tx = new BankTransaction();

            // Datum
            var bookingStr = GetField(row, template.Columns.BookingDate);
            var valueStr = GetField(row, template.Columns.ValueDate);
            var bookingDate = ParseDate(bookingStr, template.DateFormat);
            var valueDate = ParseDate(valueStr, template.DateFormat);

            if (bookingDate == null && valueDate == null)
                return null;

            tx.BookingDate = bookingDate ?? valueDate!.Value;
            tx.ValueDate = valueDate ?? bookingDate!.Value;

            // Betrag + Richtung
            if (!TryParseAmount(row, template, out var amount, out var isCredit))
                return null;

            tx.Amount = Math.Abs(amount);
            tx.IsCredit = isCredit;

            // Währung
            var currency = GetField(row, template.Columns.Currency);
            tx.Currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency.Trim();

            // Verwendungszweck
            tx.RemittanceInfo = FirstNonEmpty(
                GetField(row, template.Columns.RemittanceInfo),
                GetField(row, template.Columns.BookingText));

            tx.AdditionalInfo = GetField(row, template.Columns.AdditionalInfo);

            // Geschäftspartner
            // Zahler - Empfänger (Geld fließt von links nach rechts)
            var name = FirstNonEmpty(
                GetField(row, template.Columns.CounterpartyName),
                CombineNames(
                    GetField(row, template.Columns.CounterpartyName1),
                    GetField(row, template.Columns.CounterpartyName2)));

            if (isCredit)
                tx.DebtorName = name;
            else
                tx.CreditorName = name;

            var iban = GetField(row, template.Columns.CounterpartyIban);
            var bic = GetField(row, template.Columns.CounterpartyBic);
            if (isCredit)
            {
                tx.DebtorIban = iban;
                tx.DebtorBic = bic;
            }
            else
            {
                tx.CreditorIban = iban;
                tx.CreditorBic = bic;
            }

            // Referenzen
            tx.AcctSvcrRef = GetField(row, template.Columns.CustomerRef);
            tx.MandateId = GetField(row, template.Columns.MandateId);
            tx.EndToEndId = GetField(row, template.Columns.EndToEndId);

            // Defaults aus dem Template
            tx.GvcOverride = template.DefaultGvc;
            tx.BookingKeyOverride = template.DefaultBookingKey;

            return tx;
        }

        private static string? GetField(string[] row, int? index)
        {
            if (index == null || index.Value < 0 || index.Value >= row.Length)
                return null;
            var value = row[index.Value]?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private DateTime? ParseDate(string? input, string format)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            if (DateTime.TryParseExact(input, format, _invariant,
                DateTimeStyles.None, out var result))
                return result;

            if (DateTime.TryParse(input, _german, DateTimeStyles.None, out result))
                return result;

            return null;
        }

        private bool TryParseAmount(string[] row, CsvTemplate template,
            out decimal amount, out bool isCredit)
        {
            amount = 0;
            isCredit = true;

            if (template.AmountSignSource == "separateColumn")
            {
                var soll = GetField(row, template.Columns.Soll);
                var haben = GetField(row, template.Columns.Haben);

                if (!string.IsNullOrWhiteSpace(haben) && TryParseDecimal(haben, template.AmountFormat, out var h))
                {
                    amount = h;
                    isCredit = true;
                    return true;
                }
                if (!string.IsNullOrWhiteSpace(soll) && TryParseDecimal(soll, template.AmountFormat, out var s))
                {
                    amount = s;
                    isCredit = false;
                    return true;
                }
                return false;
            }

            var rawAmount = GetField(row, template.Columns.Amount);
            if (string.IsNullOrWhiteSpace(rawAmount))
                return false;

            if (!TryParseDecimal(rawAmount, template.AmountFormat, out var parsed))
                return false;

            amount = Math.Abs(parsed);
            isCredit = parsed >= 0;
            return true;
        }

        private bool TryParseDecimal(string input, string amountFormat, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var cleaned = input.Trim();
            var culture = amountFormat == "us" ? _invariant : _german;

            if (decimal.TryParse(cleaned, NumberStyles.Any, culture, out value))
                return true;

            var fallback = cleaned.Replace(".", "").Replace(",", ".");
            return decimal.TryParse(fallback, NumberStyles.Any, _invariant, out value);
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var v in values)
                if (!string.IsNullOrWhiteSpace(v)) return v;
            return null;
        }

        private static string? CombineNames(string? first, string? second)
        {
            if (string.IsNullOrWhiteSpace(first)) return second;
            if (string.IsNullOrWhiteSpace(second)) return first;
            if (first.Equals(second, StringComparison.OrdinalIgnoreCase)) return first;
            return $"{first} - {second}";
        }
    }
}