using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
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
            Func<string, bool>? onConflict = null,
            string? ibanOverride = null,
            string? blzOverride = null,
            string? kontoOverride = null,
            decimal? openingBalanceOverride = null,
            decimal? closingBalanceOverride = null)
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

                // 3b. Eigene IBAN aus Datenzeile lesen, falls nicht im Header gefunden
                if (string.IsNullOrWhiteSpace(ownIban) && template.OwnIbanDataColumn.HasValue)
                {
                    var col = template.OwnIbanDataColumn.Value;
                    if (col >= 0 && col < rows[0].Length)
                    {
                        var candidate = rows[0][col]?.Trim().Trim('"').Trim();
                        if (!string.IsNullOrWhiteSpace(candidate))
                            ownIban = candidate;
                    }
                }

                // 3c. Eigene IBAN aus Dateinamen lesen, falls immer noch nicht gefunden
                if (string.IsNullOrWhiteSpace(ownIban) && template.OwnIbanFromFileName)
                {
                    ownIban = ExtractIbanFromFileName(baseName);
                }

                // 3d. IBAN aus UI-Override (höchste Priorität, überschreibt alles)
                string ibanSource = "(nicht gelesen)";
                if (!string.IsNullOrWhiteSpace(ibanOverride))
                {
                    ownIban = ibanOverride.Trim();
                    ibanSource = "[aus UI]";
                }

                // 3e. IBAN aus BLZ + Kontonummer (UI bzw. Datenzeile) berechnen
                if (string.IsNullOrWhiteSpace(ownIban) && !string.IsNullOrWhiteSpace(blzOverride))
                {
                    string? konto = null;

                    if (template.OwnAccountNumberDataColumn.HasValue)
                    {
                        var col = template.OwnAccountNumberDataColumn.Value;
                        if (col >= 0 && col < rows[0].Length)
                            konto = rows[0][col]?.Trim().Trim('"').Trim();
                    }

                    if (string.IsNullOrWhiteSpace(konto))
                        konto = kontoOverride?.Trim();

                    if (!string.IsNullOrWhiteSpace(konto))
                    {
                        try
                        {
                            ownIban = BuildIbanFromBlzKonto(blzOverride, konto);
                            ibanSource = "[aus BLZ+Konto berechnet]";
                        }
                        catch (ArgumentException ex)
                        {
                            return ConversionResult.Fail(sourcePath,
                                $"IBAN konnte nicht berechnet werden: {ex.Message}");
                        }
                    }
                }

                // 3f. Wenn immer noch keine IBAN: harter Fehler (DATEV braucht :25:)
                if (string.IsNullOrWhiteSpace(ownIban))
                {
                    return ConversionResult.Fail(sourcePath,
                        "IBAN fehlt. Bitte im CSV-Tab unter 'IBAN' oder 'BLZ' + 'Kontonr.' eintragen " +
                        "— ohne diese Angaben kann DATEV die Datei nicht zuordnen.");
                }

                // 4. Buchungen mappen
                var transactions = new List<BankTransaction>();

                // Für closingBalanceDataColumn: Rohzeile mit dem zeitlich größten BookingDate merken.
                string[]? latestRow = null;
                DateTime latestBookingDate = DateTime.MinValue;

                foreach (var row in rows)
                {
                    var tx = _mapper.Map(row, template);
                    if (tx == null)
                        continue;

                    transactions.Add(tx);

                    if (tx.BookingDate > latestBookingDate)
                    {
                        latestBookingDate = tx.BookingDate;
                        latestRow = row;
                    }
                }

                if (transactions.Count == 0)
                    return ConversionResult.Fail(sourcePath,
                        "Keine gültigen Buchungen erkannt (Template prüfen).");

                // 5. Sortieren (aufsteigend nach Buchungsdatum)
                if (template.SortTransactionsAscending)
                    transactions = transactions.OrderBy(t => t.BookingDate).ToList();

                // 5b. Endsaldo aus Datenzeile lesen, falls nicht im Header gefunden.
                if (closingBalance == null
                    && template.ClosingBalanceDataColumn.HasValue
                    && latestRow != null)
                {
                    var col = template.ClosingBalanceDataColumn.Value;
                    if (col >= 0 && col < latestRow.Length)
                    {
                        var parsed = ParseAmount(latestRow[col], template.AmountFormat);
                        if (parsed.HasValue)
                            closingBalance = parsed;
                    }
                }

                // 5c. UI-Overrides für Salden (überschreiben Datei-Werte)
                if (openingBalanceOverride.HasValue)
                    openingBalance = openingBalanceOverride.Value;
                if (closingBalanceOverride.HasValue)
                    closingBalance = closingBalanceOverride.Value;

                // 6. Salden bestimmen
                var sum = transactions.Sum(t => t.IsCredit ? t.Amount : -t.Amount);

                if (openingBalance == null && closingBalance != null)
                {
                    openingBalance = closingBalance.Value - sum;
                }
                else if (openingBalance == null && closingBalance == null)
                {
                    openingBalance = 0m;
                    closingBalance = sum;
                }
                else if (openingBalance != null && closingBalance == null)
                {
                    closingBalance = openingBalance.Value + sum;
                }

                // 7. BankStatement bauen
                var statement = new BankStatement
                {
                    Iban = ownIban,
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

#if DEBUG
                // --- Debug ---
                var debugPath = Path.Combine(directory, baseName + "_debug.txt");
                File.WriteAllLines(debugPath, new[]
                {
                    $"Template:         {template.Name}",
                    $"Header-Zeilen:    {template.HeaderRows}",
                    $"Eigene IBAN:      {ownIban} {ibanSource}",
                    $"Anfangssaldo:     {(openingBalance.HasValue ? openingBalance.Value.ToString("0.00") : "(nicht gelesen)")}" +
                        (openingBalanceOverride.HasValue ? " [aus UI]" : ""),
                    $"Endsaldo:         {(closingBalance.HasValue ? closingBalance.Value.ToString("0.00") : "(nicht gelesen)")}" +
                        (closingBalanceOverride.HasValue ? " [aus UI]" : "")
                        + (template.ClosingBalanceDataColumn.HasValue && latestRow != null && !closingBalanceOverride.HasValue
                            ? " [aus Datenzeile]" : ""),
                    $"Summe Buchungen:  {sum.ToString("0.00")}",
                    $"Anzahl Buchungen: {transactions.Count}",
                    "",
                    "Buchungen:",
                    "----------"
                });

                var debugLines = new List<string>(File.ReadAllLines(debugPath));
                foreach (var t in transactions)
                {
                    var name = t.GetCounterpartyName() ?? "";
                    var swvz = t.RemittanceInfo ?? "";
                    if (swvz.Length > 60) swvz = swvz.Substring(0, 57) + "...";
                    debugLines.Add($"{t.BookingDate:yyyy-MM-dd} | {(t.IsCredit ? "+" : "-")}{t.Amount,10:0.00} | {name} | {swvz}");
                }
                File.WriteAllLines(debugPath, debugLines);
#endif

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

        private string? ReadHeaderField(string[] lines, int? row, int? col)
        {
            if (row == null || col == null) return null;

            var actualRow = row.Value < 0 ? lines.Length + row.Value : row.Value;
            if (actualRow < 0 || actualRow >= lines.Length) return null;

            var fields = _reader.ParseLine(lines[actualRow], ';', '"');
            if (col.Value < 0 || col.Value >= fields.Length) return null;

            var value = fields[col.Value].Trim().Trim('"').Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private decimal? ReadHeaderAmount(string[] lines, int? row, int? col,
            string amountFormat)
        {
            var raw = ReadHeaderField(lines, row, col);
            return ParseAmount(raw, amountFormat);
        }

        private decimal? ParseAmount(string? raw, string amountFormat)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var cleaned = new string(raw
                .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-')
                .ToArray());

            var culture = amountFormat == "us" ? _invariant : _german;

            if (decimal.TryParse(cleaned, NumberStyles.Any, culture, out var value))
                return value;

            var fallback = cleaned.Replace(".", "").Replace(",", ".");
            return decimal.TryParse(fallback, NumberStyles.Any, _invariant, out value)
                ? value
                : null;
        }

        private static string? ExtractIbanFromFileName(string fileNameWithoutExtension)
        {
            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
                return null;

            var match = Regex.Match(fileNameWithoutExtension, @"DE\d{20}\b");
            return match.Success ? match.Value : null;
        }

        /// <summary>
        /// Baut aus BLZ (8 Ziffern) und Kontonummer (max. 10 Ziffern) eine
        /// deutsche IBAN. Verwendet Mod-97 mit BigInteger (24-stellige Zahl).
        /// </summary>
        private static string BuildIbanFromBlzKonto(string blz, string konto)
        {
            blz = new string(blz.Where(char.IsDigit).ToArray());
            konto = new string(konto.Where(char.IsDigit).ToArray());

            if (blz.Length != 8)
                throw new ArgumentException(
                    $"BLZ muss genau 8 Ziffern haben (eingegeben: '{blz}', {blz.Length} Ziffern).");

            if (konto.Length == 0)
                throw new ArgumentException("Kontonummer ist leer.");

            if (konto.Length > 10)
                throw new ArgumentException(
                    $"Kontonummer darf max. 10 Ziffern haben (eingegeben: '{konto}', {konto.Length} Ziffern).");

            var bban = blz + konto.PadLeft(10, '0');
            var numeric = bban + "131400";   // DE = 13, 00 = 00

            var rest = BigInteger.Parse(numeric) % 97;
            var pruefziffer = 98 - (int)rest;

            return $"DE{pruefziffer:D2}{bban}";
        }
    }
}