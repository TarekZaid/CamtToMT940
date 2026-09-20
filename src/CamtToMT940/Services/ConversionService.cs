using System;
using System.IO;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    public class ConversionService
    {
        private readonly CamtParser _parser = new();
        private readonly Mt940Writer _writer = new();

        public ConversionResult Convert(string sourcePath, Func<string, bool>? onConflict = null)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    return ConversionResult.Fail(sourcePath, "Datei nicht gefunden.");

                // Ausgabepfad bestimmen
                var directory = Path.GetDirectoryName(sourcePath) ?? ".";
                var baseName = Path.GetFileNameWithoutExtension(sourcePath);
                var outputPath = Path.Combine(directory, baseName + ".txt");

                // Kollisionsprüfung
                if (File.Exists(outputPath) && onConflict != null)
                {
                    if (!onConflict(outputPath))
                        return ConversionResult.Fail(sourcePath, "Vom Benutzer abgebrochen.");
                }

                // 1. CAMT parsen
                var statements = _parser.Parse(sourcePath);
                if (statements.Count == 0)
                    return ConversionResult.Fail(sourcePath, "Keine Kontoauszüge in der Datei gefunden.");

                // 2. MT940 erzeugen
                var sb = new System.Text.StringBuilder();
                foreach (var stmt in statements)
                {
                    sb.Append(_writer.Write(stmt));
                }

                // 3. Schreiben (ISO-8859-1)
                File.WriteAllText(outputPath, sb.ToString(), _writer.Encoding);

                return ConversionResult.Ok(sourcePath, outputPath);
            }
            catch (Exception ex)
            {
                return ConversionResult.Fail(sourcePath, ex.Message);
            }
        }
    }
}