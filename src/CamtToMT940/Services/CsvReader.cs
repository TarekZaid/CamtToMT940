using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class CsvReader
    {
        /// <summary>Liest alle Zeilen einer Datei, mit Encoding-Erkennung und BOM-Handling.</summary>
        public string[] ReadAllLines(string filePath, string? forcedEncoding = null)
        {
            var (encoding, hasBom) = DetectEncoding(filePath, forcedEncoding);
            var lines = File.ReadAllLines(filePath, encoding);
            if (hasBom && lines.Length > 0)
                lines[0] = lines[0].TrimStart('\uFEFF');
            return lines;
        }

        /// <summary>Liest die Datenzeilen (ohne Kopfzeilen) und zerlegt sie in Spalten.</summary>
        public List<string[]> Read(string filePath, CsvTemplate template)
        {
            var allLines = ReadAllLines(filePath);
            var delimiter = template.Delimiter.FirstOrDefault(';');
            var quote = template.QuoteChar.FirstOrDefault('"');

            var dataLines = allLines
                .Skip(template.HeaderRows)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            var result = new List<string[]>();
            foreach (var line in dataLines)
                result.Add(ParseLine(line, delimiter, quote));
            return result;
        }

        /// <summary>Zerlegt eine einzelne Zeile in Spalten, berücksichtigt Anführungszeichen.</summary>
        public string[] ParseLine(string line, char delimiter, char quote)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == quote)
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == quote)
                    {
                        current.Append(quote);
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }

        private (Encoding encoding, bool hasBom) DetectEncoding(string filePath, string? forcedEncoding)
        {
            // Wenn Template eine Kodierung vorgibt, diese verwenden
            if (!string.IsNullOrWhiteSpace(forcedEncoding))
            {
                var enc = Encoding.GetEncoding(forcedEncoding);
                return (enc, false);
            }

            var bom = new byte[3];
            using (var fs = File.OpenRead(filePath))
                fs.Read(bom, 0, 3);

            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                return (new UTF8Encoding(true), true);
            if (bom[0] == 0xFF && bom[1] == 0xFE)
                return (Encoding.Unicode, true);
            if (bom[0] == 0xFE && bom[1] == 0xFF)
                return (Encoding.BigEndianUnicode, true);

            return (Encoding.GetEncoding("Windows-1252"), false);
        }
    }
}