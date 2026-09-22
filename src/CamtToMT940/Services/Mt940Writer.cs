using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    /// <summary>
    /// Erzeugt MT940-Dateien im DATEV-kompatiblen Format.
    /// </summary>
    public class Mt940Writer
    {
        // DATEV erwartet ISO-8859-1 (Latin-1)
        private static readonly Encoding OutputEncoding =
            Encoding.GetEncoding("ISO-8859-1");

        public Encoding Encoding => OutputEncoding;

        // ====================================================================
        // Öffentliche API
        // ====================================================================

        /// <summary>
        /// Erzeugt den vollständigen MT940-Inhalt für einen Kontoauszug.
        /// </summary>
        public string Write(BankStatement statement)
        {
            var sb = new StringBuilder();

            // :20: Transaktionsreferenz (frei wählbar)
            sb.AppendLine(":20:STARTUMS");

            // :25: Konto-Identifikation (BLZ/Kontonummer, aus IBAN abgeleitet)
            var (blz, kontonummer) = ExtractBlzAndKontoFromIban(statement.Iban);
            sb.AppendLine($":25:{blz}/{kontonummer}");

            // :28C: Statement-Nummer / Sequenz
            sb.AppendLine(":28C:1");

            // :60F: Anfangssaldo
            var openingDate = statement.OpeningDate ?? DateTime.Today;
            var openingSign = statement.OpeningIsCredit ? "C" : "D";
            sb.AppendLine($":60F:{openingSign}{openingDate:yyMMdd}{statement.Currency}" +
                          $"{FormatAmount(statement.OpeningBalance)}");

            // Buchungen
            foreach (var tx in statement.Transactions)
            {
                WriteTransaction(sb, tx);
            }

            // :62F: Endsaldo
            var closingDate = statement.ClosingDate ?? DateTime.Today;
            var closingSign = statement.ClosingIsCredit ? "C" : "D";
            sb.AppendLine($":62F:{closingSign}{closingDate:yyMMdd}{statement.Currency}" +
                          $"{FormatAmount(statement.ClosingBalance)}");

            // MT940-Abschluss
            sb.AppendLine("-");

            return sb.ToString();
        }

        // ====================================================================
        // Transaktions-Aufbau (:61: + :86:)
        // ====================================================================

        private void WriteTransaction(StringBuilder sb, BankTransaction tx)
        {
            // :61: Zeile
            // Aufbau nach MT940-Spezifikation:
            //   Subfeld 1: Valutadatum (YYMMDD)
            //   Subfeld 2: Buchungsdatum (MMDD)
            //   Subfeld 3: Soll/Haben-Kennung (CR = Haben, DR = Soll)
            //   Subfeld 5: Betrag (Komma als Dezimaltrenner)
            //   Subfeld 6: Konstante "N"
            //   Subfeld 7: Buchungsschlüssel (TRF, DDT, etc.)
            //   Subfeld 8: Referenz (KREF+... oder NONREF)
            var valueDate = tx.ValueDate.ToString("yyMMdd");
            var bookingDate = tx.BookingDate.ToString("MMdd");
            var sign = tx.IsCredit ? "CR" : "DR";
            var amount = FormatAmount(tx.Amount);
            var txCode = ExtractTransactionCode(tx.TransactionCode) ?? "TRF";
            var reference = string.IsNullOrWhiteSpace(tx.AcctSvcrRef)
                ? "NONREF"
                : $"KREF+{tx.AcctSvcrRef}";

            sb.AppendLine($":61:{valueDate}{bookingDate}{sign}{amount}N{txCode}{reference}");
            // :86: Verwendungszweck-Zeilen
            WriteRemittanceInfo(sb, tx);
        }

        /// <summary>
        /// Baut die :86:-Felder mit allen relevanten ?XX-Codes auf.
        /// Alle Codes werden in EINEM :86:-Feld zusammengefasst und in
        /// 27-Zeichen-Blöcke umgebrochen (DATEV-Konvention).
        /// </summary>
        private void WriteRemittanceInfo(StringBuilder sb, BankTransaction tx)
        {
            var parts = new StringBuilder();

            // ============================================================
            // ?00 Buchungstext (mit GVC davor)
            // ============================================================
            var gvc = ExtractGvc(tx.TransactionCode) ?? "116";
            var additionalInfo = string.IsNullOrWhiteSpace(tx.AdditionalInfo)
                ? "Ueberweisung"
                : Sanitize(tx.AdditionalInfo);
            parts.Append($"{gvc}?00{additionalInfo}");

            // ?10 Buchungsschlüssel
            var bookingKey = ExtractBookingKey(tx.TransactionCode);
            if (!string.IsNullOrWhiteSpace(bookingKey))
   parts.Append($"?10{bookingKey}");
             
            // ============================================================
            // ?20–?29 Verwendungszweck-Block (EREF + KREF + SVWZ)
            // ============================================================
            var verwendungszweck = new StringBuilder();

            // EREF
            if (!string.IsNullOrWhiteSpace(tx.EndToEndId))
                verwendungszweck.Append($"EREF+{Sanitize(tx.EndToEndId)} ");

            // KREF
            if (!string.IsNullOrWhiteSpace(tx.AcctSvcrRef))
                verwendungszweck.Append($"KREF+{Sanitize(tx.AcctSvcrRef)} ");

            // SVWZ
            if (!string.IsNullOrWhiteSpace(tx.RemittanceInfo))
                verwendungszweck.Append($"SVWZ+{Sanitize(tx.RemittanceInfo)}");

            // Auf ?20–?29 verteilen (max. 24 Zeichen pro Code, maximal 10 Codes)
            AppendDistributedBlock(parts, 20, 29, verwendungszweck.ToString().TrimEnd());

            // ============================================================
            // ?30 BIC
            // ============================================================
            var bic = tx.GetCounterpartyBic();
            if (!string.IsNullOrWhiteSpace(bic))
                parts.Append($"?30{Sanitize(bic)}");

            // ============================================================
            // ?31 IBAN
            // ============================================================
            var iban = tx.GetCounterpartyIban();
            if (!string.IsNullOrWhiteSpace(iban))
                parts.Append($"?31{Sanitize(iban)}");

            // ============================================================
            // ?32/?33 Name (jeweils max. 24 Zeichen pro Code)
            // ============================================================
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

            // ============================================================
            // Zeilenweise ausgeben
            // ============================================================
            var lines = Wrap86SingleField(parts.ToString());
            for (int i = 0; i < lines.Count; i++)
            {
                sb.AppendLine(i == 0 ? $":86:{lines[i]}" : lines[i]);
            }
        }

        // ====================================================================
        // :86:-Formatierung
        // ====================================================================

        /// <summary>
        /// Bricht den :86:-Inhalt in 27-Zeichen-Blöcke um.
        /// ?XX-Codes werden nicht zerschnitten; zu lange Codes werden
        /// ohne erneutes ?XX-Präfix in der Folgzeile fortgesetzt.
        /// </summary>
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

        /// <summary>
        /// Verteilt einen Text auf fortlaufende ?XX-Codes von startCode bis endCode.
        /// Jeder Code bekommt max. 24 Zeichen (27 minus "?XX"). Wenn der Text nicht
        /// in den Code-Bereich passt, wird der Rest abgeschnitten.
        /// </summary>
        private static void AppendDistributedBlock(StringBuilder parts, int startCode,
            int endCode, string content)
        {
            const int maxContentPerField = 24; // ?XX + 24 = 27 Zeichen pro Zeile
            if (string.IsNullOrEmpty(content)) return;

            var remaining = content;
            for (int code = startCode; code <= endCode && remaining.Length > 0; code++)
            {
                var take = Math.Min(maxContentPerField, remaining.Length);
                var chunk = remaining.Substring(0, take);
                remaining = remaining.Substring(take);

                parts.Append($"?{code:D2}{chunk}");
            }
            // Wenn remaining > 0 nach dem letzten Code: Rest wird verworfen
        }

        /// <summary>
        /// Teilt einen :86:-String in Segmente, die jeweils mit ?XX beginnen.
        /// </summary>
        private static List<string> SplitIntoSegments(string content)
        {
            var segments = new List<string>();
            var current = new StringBuilder();

            for (int i = 0; i < content.Length; i++)
            {
                // Neues Segment beginnt mit ?XX (Fragezeichen + zwei Ziffern)
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

        // ====================================================================
        // CAMT-Feld-Extraktion
        // ====================================================================

        /// <summary>
        /// Extrahiert aus "NTRF+166+00931" den Buchungscode "TRF".
        /// Der führende Buchstabe N (Konstante) wird entfernt.
        /// </summary>
        private static string? ExtractTransactionCode(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var idx = camtCode.IndexOf('+');
            var code = idx < 0 ? camtCode : camtCode[..idx];

            // Führendes "N" entfernen (z.B. "NTRF" → "TRF")
            if (code.StartsWith("N") && code.Length > 1)
                code = code.Substring(1);

            return code;
        }

        /// <summary>
        /// Extrahiert aus "NTRF+166+00931" den Geschäftsvorfall-Code "166".
        /// </summary>
        private static string? ExtractGvc(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var parts = camtCode.Split('+');
            if (parts.Length >= 2) return parts[1];
            return null;
        }

        /// <summary>
        /// Extrahiert aus "NTRF+166+00931" den numerischen Teil "166"
        /// für den ?10-Buchungsschlüssel.
        /// </summary>
        private static string? ExtractNumericPart(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var parts = camtCode.Split('+');
            if (parts.Length >= 2 && int.TryParse(parts[1], out _))
                return parts[1];
            return null;
        }

        /// <summary>
        /// Extrahiert aus "NTRF+166+00931" den Buchungsschlüssel "931".
        /// </summary>
        private static string? ExtractBookingKey(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var parts = camtCode.Split('+');
            if (parts.Length >= 3)
            {
                // Führende Nullen entfernen: "00931" → "931"
                return parts[2].TrimStart('0');
            }
            return null;
        }

        /// <summary>
        /// Extrahiert BLZ und Kontonummer aus einer deutschen IBAN.
        /// Struktur: DE + 2 Prüfziffern + 8-stellige BLZ + 10-stellige Kontonummer.
        /// </summary>
        private static (string Blz, string Konto) ExtractBlzAndKontoFromIban(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return ("", "");

            var cleaned = iban.Replace(" ", "").ToUpperInvariant();

            // Nur deutsche IBANs werden aufgeteilt; sonst IBAN unverändert zurück
            if (cleaned.Length != 22 || !cleaned.StartsWith("DE"))
                return (cleaned, "");

            var blz = cleaned.Substring(4, 8);       // Stellen 5-12
            var konto = cleaned.Substring(12, 10);   // Stellen 13-22

            return (blz, konto);
        }

        // ====================================================================
        // Formatierung
        // ====================================================================

        /// <summary>
        /// Formatiert einen Betrag MT940-konform: "1234,56" (Komma als
        /// Dezimaltrenner, kein Tausenderpunkt).
        /// </summary>
        private static string FormatAmount(decimal amount)
        {
            return amount.ToString("0.00", CultureInfo.GetCultureInfo("de-DE"))
                .Replace(".", ",");
        }

        /// <summary>
        /// Bereinigt Text für die Verwendung in :86:.
        /// Ersetzt Umlaute durch ASCII-Äquivalente (ae, oe, ue, ss), entfernt
        /// Zeilenumbrüche und reduziert mehrfache Leerzeichen. Damit ist die
        /// Ausgabe unabhängig von der Zeichenkodierung und maximal DATEV-kompatibel.
        /// </summary>
        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Umlaute und Sonderzeichen durch ASCII-Äquivalente ersetzen
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
                        // Alle Nicht-ASCII-Zeichen durch '?' ersetzen
                        if (c > 127) sb.Append('?');
                        else sb.Append(c);
                        break;
                }
            }

            // Zeilenumbrüche und Steuerzeichen durch Leerzeichen ersetzen
            var cleaned = sb.ToString()
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            // Mehrfache Leerzeichen zusammenfassen
            while (cleaned.Contains("  "))
                cleaned = cleaned.Replace("  ", " ");

            return cleaned.Trim();
        }
    }
}