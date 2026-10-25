using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class Mt940Writer
    {
        private static readonly Encoding OutputEncoding =
            Encoding.GetEncoding("ISO-8859-1");

        public Encoding Encoding => OutputEncoding;

        public string Write(BankStatement statement)
        {
            var sb = new StringBuilder();

            sb.AppendLine(":20:STARTUMS");

            // :25: BLZ/Kontonummer aus IBAN ableiten.
            // Wenn keine IBAN vorhanden → leer (DATEV akzeptiert das).
            if (!string.IsNullOrWhiteSpace(statement.Iban))
            {
                var (blz, kontonummer) = ExtractBlzAndKontoFromIban(statement.Iban);
                sb.AppendLine($":25:{blz}/{kontonummer}");
            }
            else
            {
                sb.AppendLine(":25:");
            }

            sb.AppendLine(":28C:1");

            var openingDate = statement.OpeningDate ?? DateTime.Today;
            var openingSign = statement.OpeningIsCredit ? "C" : "D";
            sb.AppendLine($":60F:{openingSign}{openingDate:yyMMdd}{statement.Currency}" +
                          $"{FormatAmount(statement.OpeningBalance)}");

            foreach (var tx in statement.Transactions)
                WriteTransaction(sb, tx);

            var closingDate = statement.ClosingDate ?? DateTime.Today;
            var closingSign = statement.ClosingIsCredit ? "C" : "D";
            sb.AppendLine($":62F:{closingSign}{closingDate:yyMMdd}{statement.Currency}" +
                          $"{FormatAmount(statement.ClosingBalance)}");

            sb.AppendLine("-");
            return sb.ToString();
        }

        private void WriteTransaction(StringBuilder sb, BankTransaction tx)
        {
            var valueDate = tx.ValueDate.ToString("yyMMdd");
            var bookingDate = tx.BookingDate.ToString("MMdd");
            var sign = tx.IsCredit ? "CR" : "DR";
            var amount = FormatAmount(tx.Amount);
            var txCode = ExtractTransactionCode(tx.TransactionCode) ?? "TRF";
            var reference = string.IsNullOrWhiteSpace(tx.AcctSvcrRef)
                ? "NONREF"
                : $"KREF+{tx.AcctSvcrRef}";

            sb.AppendLine($":61:{valueDate}{bookingDate}{sign}{amount}N{txCode}{reference}");
            WriteRemittanceInfo(sb, tx);
        }

        private void WriteRemittanceInfo(StringBuilder sb, BankTransaction tx)
        {
            var parts = new StringBuilder();

            // GVC + ?00
            var gvc = tx.GvcOverride
                ?? ExtractGvc(tx.TransactionCode)
                ?? "166";
            var additionalInfo = string.IsNullOrWhiteSpace(tx.AdditionalInfo)
                ? "Ueberweisung"
                : Sanitize(tx.AdditionalInfo);
            parts.Append($"{gvc}?00{additionalInfo}");

            // ?10 Buchungsschlüssel
            var bookingKey = tx.BookingKeyOverride
                ?? ExtractBookingKey(tx.TransactionCode);
            if (!string.IsNullOrWhiteSpace(bookingKey))
                parts.Append($"?10{bookingKey}");

            // ?20 EREF
            if (!string.IsNullOrWhiteSpace(tx.EndToEndId))
                parts.Append($"?20EREF+{Sanitize(tx.EndToEndId)}");

            // ?21 KREF (mit NOTPROVIDED/NONREF-Filter)
            if (!string.IsNullOrWhiteSpace(tx.AcctSvcrRef)
                && !IsPlaceholderReference(tx.AcctSvcrRef))
                parts.Append($"?21KREF+{Sanitize(tx.AcctSvcrRef)}");

            // ?23 SVWZ (erster Teil), ?24 ?25 … (Folgeteile)
            if (!string.IsNullOrWhiteSpace(tx.RemittanceInfo))
            {
                var swvz = Sanitize(tx.RemittanceInfo);
                var chunks = SplitIntoChunks(swvz, 27);  // 27 Zeichen pro Feld
                for (int i = 0; i < chunks.Count && i < 10; i++)  // ?23 bis ?32 maximal
                {
                    var tag = i == 0 ? "23" : (23 + i).ToString("D2");  // 23, 24, 25, …
                    parts.Append($"?{tag}{chunks[i]}");
                }
            }

            // ?30 BIC
            var bic = tx.GetCounterpartyBic();
            if (!string.IsNullOrWhiteSpace(bic))
                parts.Append($"?30{Sanitize(bic)}");

            // ?31 IBAN
            var iban = tx.GetCounterpartyIban();
            if (!string.IsNullOrWhiteSpace(iban))
                parts.Append($"?31{Sanitize(iban)}");

            // ?32/?33 Name
            var name = tx.GetCounterpartyName();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var sanitized = Sanitize(name);
                if (sanitized.Length <= 24)
                {
                    parts.Append($"?32{sanitized}");
                }
                else
                {
                    var cutAt = 24;
                    var lastSpace = sanitized.Substring(0, 24).LastIndexOf(' ');
                    if (lastSpace > 15) cutAt = lastSpace;

                    var part1 = sanitized.Substring(0, cutAt).TrimEnd();
                    var part2 = sanitized.Substring(cutAt).TrimStart();

                    if (part1.Length > 24) part1 = part1.Substring(0, 24);
                    parts.Append($"?32{part1}");

                    if (part2.Length > 24) part2 = part2.Substring(0, 24);
                    if (part2.Length > 0)
                        parts.Append($"?33{part2}");
                }
            }

            var lines = Wrap86SingleField(parts.ToString());
            for (int i = 0; i < lines.Count; i++)
                sb.AppendLine(i == 0 ? $":86:{lines[i]}" : lines[i]);
        }

        private static List<string> Wrap86SingleField(string content)
        {
            const int maxLength = 27;
            var result = new List<string>();
            var segments = SplitIntoSegments(content);

            var current = new StringBuilder();
            foreach (var segment in segments)
            {
                if (current.Length + segment.Length <= maxLength)
                {
                    current.Append(segment);
                }
                else
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    current.Append(segment);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
        }

        private static List<string> SplitIntoSegments(string content)
        {
            var segments = new List<string>();
            var current = new StringBuilder();

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '?' && i + 2 < content.Length &&
                    char.IsDigit(content[i + 1]) && char.IsDigit(content[i + 2]))
                {
                    if (current.Length > 0)
                    {
                        segments.Add(current.ToString());
                        current.Clear();
                    }
                }
                current.Append(content[i]);
            }

            if (current.Length > 0)
                segments.Add(current.ToString());

            return segments;
        }

        private static string? ExtractTransactionCode(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var idx = camtCode.IndexOf('+');
            var code = idx < 0 ? camtCode : camtCode[..idx];

            if (code.StartsWith("N") && code.Length > 1)
                code = code.Substring(1);

            return code;
        }

        private static string? ExtractGvc(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var parts = camtCode.Split('+');
            if (parts.Length >= 2) return parts[1];
            return null;
        }

        private static string? ExtractBookingKey(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var parts = camtCode.Split('+');
            if (parts.Length >= 3)
                return parts[2].TrimStart('0');
            return null;
        }

        private static (string Blz, string Konto) ExtractBlzAndKontoFromIban(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return ("", "");

            var cleaned = iban.Replace(" ", "").ToUpperInvariant();

            if (cleaned.Length != 22 || !cleaned.StartsWith("DE"))
                return (cleaned, "");

            var blz = cleaned.Substring(4, 8);
            var konto = cleaned.Substring(12, 10);
            return (blz, konto);
        }

        private static string FormatAmount(decimal amount)
        {
            return amount.ToString("0.00", CultureInfo.GetCultureInfo("de-DE"))
                .Replace(".", ",");
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case 'ä': sb.Append("ae"); break;
                    case 'ö': sb.Append("oe"); break;
                    case 'ü': sb.Append("ue"); break;
                    case 'Ä': sb.Append("Ae"); break;
                    case 'Ö': sb.Append("Oe"); break;
                    case 'Ü': sb.Append("Ue"); break;
                    case 'ß': sb.Append("ss"); break;
                    default:
                        if (c > 127) sb.Append('?');
                        else sb.Append(c);
                        break;
                }
            }

            var cleaned = sb.ToString()
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            while (cleaned.Contains("  "))
                cleaned = cleaned.Replace("  ", " ");

            return cleaned.Trim();
        }

        private static bool IsPlaceholderReference(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            var v = value.Trim();
            return v.Equals("NOTPROVIDED", StringComparison.OrdinalIgnoreCase)
                || v.Equals("NONREF", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> SplitIntoChunks(string text, int maxLength)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text)) return result;

            // An Wortgrenzen trennen, wenn möglich
            while (text.Length > maxLength)
            {
                var cut = text.LastIndexOf(' ', maxLength);
                if (cut <= 0) cut = maxLength;

                var chunk = text.Substring(0, cut).TrimEnd();
                if (chunk.Length > 0) result.Add(chunk);
                text = text.Substring(cut).TrimStart();
            }
            if (text.Length > 0) result.Add(text);
            return result;
        }
    }
}