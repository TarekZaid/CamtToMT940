using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class CsvConversionService
    {
        private readonly CsvReader _reader = new();
        private readonly CsvMapper _mapper = new();
        private readonly Mt940Writer _writer = new();
        private readonly CultureInfo _german = CultureInfo.GetCultureInfo("de-DE");
        private readonly CultureInfo _invariant = CultureInfo.InvariantCulture;

        public ConversionResult Convert(string sourcePath, CsvTemplate template,
            Func<string, bool>? onConflict = null)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    return ConversionResult.Fail(sourcePath, "Datei nicht gefunden.");

                var directory = Path.GetDirectoryName(sourcePath) ?? ".";
                var baseName = Path.GetFileNameWithoutExtension(sourcePath);
                var outputPath = Path.Combine(directory, baseName + ".txt");

                if (File.Exists(outputPath) && onConflict != null)
                {
                    if (!onConflict(outputPath))
                        return ConversionResult.Fail(sourcePath, "Vom Benutzer abgebrochen.");
                }

                // 1. Alle Zeilen lesen (Header + Daten)
                var allLines = _reader.ReadAllLines(sourcePath, template.Encoding);

                // 2. IBAN und Salden aus dem Header lesen
                var ownIban = ReadHeaderField(allLines, template.OwnIbanRow, template.OwnIbanColumn);
                var closingBalance = ReadHeaderAmount(allLines, template.ClosingBalanceRow,
                    template.ClosingBalanceColumn, template.AmountFormat);
                var openingBalance = ReadHeaderAmount(allLines, template.OpeningBalanceRow,
                    template.OpeningBalanceColumn, template.AmountFormat);

                // 3. Datenzeilen einlesen
                var delimiter = template.Delimiter.FirstOrDefault(';');
                var quote = template.QuoteChar.FirstOrDefault('"');
                var rows = allLines
                    .Skip(template.HeaderRows)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => _reader.ParseLine(line, delimiter, quote))
                    .ToList();

                if (rows.Count == 0)
                    return ConversionResult.Fail(sourcePath, "Keine Datenzeilen gefunden.");

                // 4. Buchungen mappen
                var transactions = new List<BankTransaction>();
                foreach (var row in rows)
                {
                    var tx = _mapper.Map(row, template);
                    if (tx != null)
                        transactions.Add(tx);
                }

                if (transactions.Count == 0)
                    return ConversionResult.Fail(sourcePath,
                        "Keine gültigen Buchungen erkannt (Template prüfen).");

                // 5. Sortieren (aufsteigend nach Buchungsdatum)
                if (template.SortTransactionsAscending)
                    transactions = transactions.OrderBy(t => t.BookingDate).ToList();

                // 6. Anfangssaldo berechnen, falls nicht aus Header vorhanden
                if (openingBalance == null && closingBalance != null)
                {
                    var sum = transactions.Sum(t => t.IsCredit ? t.Amount : -t.Amount);
                    openingBalance = closingBalance.Value - sum;
                }

                // 7. BankStatement bauen
                var statement = new BankStatement
                {
                    Iban = ownIban ?? "",
                    OwnerName = "",
                    Currency = transactions[0].Currency,
                    OpeningDate = transactions.First().ValueDate,
                    OpeningBalance = openingBalance.HasValue ? Math.Abs(openingBalance.Value) : 0m,
                    OpeningIsCredit = !openingBalance.HasValue || openingBalance.Value >= 0,
                    ClosingDate = transactions.Last().ValueDate,
                    ClosingBalance = closingBalance.HasValue ? Math.Abs(closingBalance.Value) : 0m,
                    ClosingIsCredit = !closingBalance.HasValue || closingBalance.Value >= 0,
                    Transactions = transactions
                };

                // --- Debug: Salden und Summe prüfen ---
                var sumCheck = transactions.Sum(t => t.IsCredit ? t.Amount : -t.Amount);
                var debugPath = Path.Combine(directory, baseName + "_debug.txt");
                File.WriteAllLines(debugPath, new[]
                {
                    $"Template:         {template.Name}",
                    $"Header-Zeilen:    {template.HeaderRows}",
                    $"Eigene IBAN:      {ownIban ?? "(nicht gelesen)"}",
                    $"Anfangssaldo:     {(openingBalance.HasValue ? openingBalance.Value.ToString("0.00") : "(nicht gelesen)")}",
                    $"Endsaldo:         {(closingBalance.HasValue ? closingBalance.Value.ToString("0.00") : "(nicht gelesen)")}",
                    $"Summe Buchungen:  {sumCheck.ToString("0.00")}",
                    $"Anzahl Buchungen: {transactions.Count}",
                    "",
                    "Buchungen:",
                    "----------"
                });

                var debugLines = new List<string>(File.ReadAllLines(debugPath));
                foreach (var t in transactions)
                {
                    debugLines.Add($"{t.BookingDate:yyyy-MM-dd} | {(t.IsCredit ? "+" : "-")}{t.Amount,10:0.00} | {t.GetCounterpartyName()}");
                }
                File.WriteAllLines(debugPath, debugLines);
                // 8. MT940 schreiben
                var mt940 = _writer.Write(statement);
                File.WriteAllText(outputPath, mt940, _writer.Encoding);

                return ConversionResult.Ok(sourcePath, outputPath);
            }
            catch (Exception ex)
            {
                return ConversionResult.Fail(sourcePath, ex.Message);
            }
        }

        // --------------------------------------------------------------------
        // Header-Parsing
        // --------------------------------------------------------------------

        /// <summary>
        /// Liest ein einzelnes Feld aus dem Header.
        /// row = -1 bedeutet "letzte Zeile".
        /// </summary>
        private string? ReadHeaderField(string[] lines, int? row, int? col)
        {
            if (row == null || col == null) return null;

            // -1 = letzte Zeile
            var actualRow = row.Value < 0 ? lines.Length + row.Value : row.Value;
            if (actualRow < 0 || actualRow >= lines.Length) return null;

            var fields = _reader.ParseLine(lines[actualRow], ';', '"');
            if (col.Value < 0 || col.Value >= fields.Length) return null;

            var value = fields[col.Value].Trim().Trim('"').Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// Liest einen Betrag aus dem Header. Behandelt Formate wie
        /// "1.976,81 €" und "€ 3.864,03" und "599,21".
        /// </summary>
        private decimal? ReadHeaderAmount(string[] lines, int? row, int? col,
            string amountFormat)
        {
            var raw = ReadHeaderField(lines, row, col);
            if (raw == null) return null;

            // Euro-Zeichen, geschützte Leerzeichen und einfache Leerzeichen entfernen
            var cleaned = new string(raw
                .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-')
                .ToArray());

            var culture = amountFormat == "us" ? _invariant : _german;

            if (decimal.TryParse(cleaned, NumberStyles.Any, culture, out var value))
                return value;

            // Fallback: "1.234,56" hart parsen
            var fallback = cleaned.Replace(".", "").Replace(",", ".");
            return decimal.TryParse(fallback, NumberStyles.Any, _invariant, out value)
                ? value
                : null;
        }
    }
}