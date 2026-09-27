using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    /// <summary>
    /// Verwaltet die verfügbaren CSV-Templates.
    /// Lädt beim Start alle .json-Dateien aus dem templates/-Ordner
    /// (neben der .exe bzw. im Projekt-Root während der Entwicklung).
    /// </summary>
    public class CsvTemplateStore
    {
        private readonly string _templatesFolder;
        private readonly List<CsvTemplate> _templates = new();

        public CsvTemplateStore(string? templatesFolder = null)
        {
            _templatesFolder = templatesFolder
                ?? Path.Combine(AppContext.BaseDirectory, "templates");
            Load();
        }

        public IReadOnlyList<CsvTemplate> Templates => _templates;

        public CsvTemplate? Get(string name)
            => _templates.FirstOrDefault(t =>
                string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Lädt alle Templates aus dem Ordner. Fehlerhafte Dateien
        /// werden übersprungen, das Programm läuft weiter.
        /// </summary>
        public void Load()
        {
            _templates.Clear();

            if (!Directory.Exists(_templatesFolder))
                Directory.CreateDirectory(_templatesFolder);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            foreach (var file in Directory.EnumerateFiles(_templatesFolder, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var template = JsonSerializer.Deserialize<CsvTemplate>(json, jsonOptions);
                    if (template != null && !string.IsNullOrWhiteSpace(template.Name))
                        _templates.Add(template);
                }
                catch
                {
                    // Ungültige Datei überspringen
                }
            }

            _templates.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Speichert ein Template als JSON. Nützlich für spätere
        /// "Template-Editor"-Funktion.
        /// </summary>
        public void Save(CsvTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new ArgumentException("Template braucht einen Namen.");

            if (!Directory.Exists(_templatesFolder))
                Directory.CreateDirectory(_templatesFolder);

            var safeName = MakeSafeFileName(template.Name);
            var path = Path.Combine(_templatesFolder, safeName + ".json");

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(template, jsonOptions);
            File.WriteAllText(path, json);
        }

        private static string MakeSafeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray());
            return cleaned.Trim().ToLowerInvariant().Replace(' ', '_');
        }
    }
}