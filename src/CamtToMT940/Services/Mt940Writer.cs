using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class Mt940Writer
    {
        // DATEV erwartet ISO-8859-1 (Latin-1)
        private static readonly Encoding OutputEncoding =
            Encoding.GetEncoding("ISO-8859-1");

        public Encoding Encoding => OutputEncoding;

        /// <summary>
        /// Erzeugt den vollständigen MT940-Inhalt für einen Kontoauszug.
        /// </summary>
        public string Write(BankStatement statement)
        {
            var sb = new StringBuilder();

            // :20: Transaktionsreferenz (frei wählbar)
            sb.AppendLine(":20:STARTUMS");

            // :25: Konto-Identifikation
            // Format bei VR-Banken: BLZ/Kontonummer
            // Wir verwenden die IBAN, da wir die BLZ nicht direkt haben.
            sb.AppendLine($":25:{statement.Iban}");

            // :28C: Statement-Nummer / Sequenz
            sb.AppendLine(":28C:0");

            // :60F: Anfangssaldo
            var openingDate = statement.OpeningDate ?? DateTime.Today;
            var openingSign = statement.OpeningIsCredit ? "C" : "D";
            sb.AppendLine($":60F:{openingSign}{openingDate:yyMMdd}{statement.Currency}" +
                          $"{FormatAmount(statement.OpeningBalance)}");

            // :61: + :86: pro Buchung
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

        private void WriteTransaction(StringBuilder sb, BankTransaction tx)
        {
            // :61: Zeile
            // Aufbau: YYMMDD (Buchung) YYMMDD (Valuta) [C|D|RD|RC] Betrag Nxx... 
            // Wir verwenden: Buchungsdatum + Valutadatum + CRDT/DBIT + Betrag + Buchungscode
            var booking = tx.BookingDate.ToString("yyMMdd");
            var value = tx.ValueDate.ToString("yyMMdd");
            var sign = tx.IsCredit ? "C" : "D";
            var amount = FormatAmount(tx.Amount);

            // Buchungscode (NTRF, NDDT, etc.) aus TransactionCode ableiten
            // Format in CAMT: "NTRF+166+00931" → wir nehmen den Teil vor dem ersten "+"
            var txCode = ExtractTransactionCode(tx.TransactionCode) ?? "NTRF";

            // Referenz (KREF+) – AcctSvcrRef als Referenz der Bank
            var reference = string.IsNullOrWhiteSpace(tx.AcctSvcrRef)
                ? "NONREF"
                : $"KREF+{tx.AcctSvcrRef}";

            sb.AppendLine($":61:{booking}{value}{sign}{amount}{txCode}{reference}");
            // Achtung: In echten MT940-Dateien steht nach dem Code oft noch //...
            // Für DATEV reicht diese Basisform.

            // :86: Zeilen
            WriteRemittanceInfo(sb, tx);
        }

        /// <summary>
        /// Baut die :86: Zeilen mit den wichtigsten ?XX-Codes auf.
        /// </summary>
        private void WriteRemittanceInfo(StringBuilder sb, BankTransaction tx)
        {
            // Wir bauen einen einzelnen String mit allen Codes und brechen
            // anschließend auf 27-Zeichen-Blöcke um (nach dem ?XX-Code).

            var parts = new StringBuilder();

            // ?00 Buchungstext (z.B. "Uberweisungsgutschr.")
            if (!string.IsNullOrWhiteSpace(tx.AdditionalInfo))
            {
                var text = Sanitize(tx.AdditionalInfo);
                parts.Append($"?00{text}");
            }

            // ?10 Buchungsschlüssel – optional
            if (!string.IsNullOrWhiteSpace(tx.TransactionCode))
            {
                var code = ExtractNumericPart(tx.TransactionCode);
                if (code != null) parts.Append($"?10{code}");
            }

            // ?20 EREF
            if (!string.IsNullOrWhiteSpace(tx.EndToEndId))
                parts.Append($"?20EREF+{Sanitize(tx.EndToEndId)}");

            // ?21 KREF
            if (!string.IsNullOrWhiteSpace(tx.AcctSvcrRef))
                parts.Append($"?21KREF+{Sanitize(tx.AcctSvcrRef)}");

            // ?23 SVWZ (Verwendungszweck)
            if (!string.IsNullOrWhiteSpace(tx.RemittanceInfo))
                parts.Append($"?23SVWZ+{Sanitize(tx.RemittanceInfo)}");

            // ?30 BIC
            var bic = tx.GetCounterpartyBic();
            if (!string.IsNullOrWhiteSpace(bic))
                parts.Append($"?30{Sanitize(bic)}");

            // ?31 IBAN
            var iban = tx.GetCounterpartyIban();
            if (!string.IsNullOrWhiteSpace(iban))
                parts.Append($"?31{Sanitize(iban)}");

            // ?32 Name
            var name = tx.GetCounterpartyName();
            if (!string.IsNullOrWhiteSpace(name))
                parts.Append($"?32{Sanitize(name)}");

            // Auf :86: Zeilen umbrechen
            var lines = Wrap86(parts.ToString());
            foreach (var line in lines)
            {
                sb.AppendLine($":86:{line}");
            }
        }

        // --------------------------------------------------------------------
        // Hilfsmethoden
        // --------------------------------------------------------------------

        /// <summary>
        /// Formatiert einen Betrag MT940-konform: 1234,56 (kein Tausenderpunkt).
        /// </summary>
        private static string FormatAmount(decimal amount)
        {
            return amount.ToString("0.00", CultureInfo.GetCultureInfo("de-DE"))
                .Replace(".", ",");
        }

        /// <summary>
        /// Entfernt Zeichen, die in :86: nicht erlaubt sind, und ersetzt Umlaute
        /// je nach Bedarf. Für DATEV sind Umlaute in ISO-8859-1 erlaubt.
        /// </summary>
        private static string Sanitize(string input)
        {
            // Zeilenumbrüche und Steuerzeichen entfernen
            var cleaned = input
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");
            // Mehrfache Leerzeichen zusammenfassen
            while (cleaned.Contains("  "))
                cleaned = cleaned.Replace("  ", " ");
            return cleaned.Trim();
        }

        /// <summary>
        /// Extrahiert aus "NTRF+166+00931" den Buchungscode "NTRF".
        /// </summary>
        private static string? ExtractTransactionCode(string? camtCode)
        {
            if (string.IsNullOrWhiteSpace(camtCode)) return null;
            var idx = camtCode.IndexOf('+');
            return idx < 0 ? camtCode : camtCode[..idx];
        }

        /// <summary>
        /// Extrahiert aus "NTRF+166+00931" den numerischen Teil "166" als ?10-Code.
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
        /// Bricht den :86:-Inhalt in Zeilen von maximal 27 Zeichen um.
        /// Ein ?XX-Code darf nicht getrennt werden.
        /// </summary>
        private static List<string> Wrap86(string content)
        {
            var result = new List<string>();
            const int maxLength = 27;

            // In ?XX-Segmente aufteilen (jedes beginnt mit ? und zwei Ziffern)
            var segments = SplitIntoSegments(content);

            var current = new StringBuilder();
            foreach (var segment in segments)
            {
                // Passt das Segment noch in die aktuelle Zeile?
                if (current.Length + segment.Length <= maxLength)
                {
                    current.Append(segment);
                }
                else
                {
                    // Zeile abschließen und neue beginnen
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    // Segment aufteilen, falls es allein zu lang ist
                    var seg = segment;
                    while (seg.Length > maxLength)
                    {
                        result.Add(seg[..maxLength]);
                        seg = seg[maxLength..];
                    }
                    current.Append(seg);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
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
                // Neues Segment beginnt mit ?XX
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
    }
}