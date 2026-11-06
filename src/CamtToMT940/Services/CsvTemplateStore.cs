using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CamtToMT940.Models;

namespace CamtToMT940.Services
{
    /// <summary>
    /// Verwaltet die verfügbaren CSV-Templates.
    /// Lädt alle .json-Dateien aus den eingebetteten Ressourcen der Assembly.
    /// </summary>
    public class CsvTemplateStore
    {
        private readonly List<CsvTemplate> _templates = new();

        public CsvTemplateStore()
        {
            Load();
        }

        public IReadOnlyList<CsvTemplate> Templates => _templates;

        public CsvTemplate? Get(string name)
            => _templates.FirstOrDefault(t =>
                string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Lädt alle Templates aus den eingebetteten Ressourcen.
        /// Fehlerhafte Dateien werden übersprungen, das Programm läuft weiter.
        /// </summary>
        public void Load()
        {
            _templates.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                .Where(n => n.Contains(".templates.", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            foreach (var resourceName in resourceNames)
            {
                try
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null) continue;

                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    var json = reader.ReadToEnd();

                    var template = JsonSerializer.Deserialize<CsvTemplate>(json, jsonOptions);
                    if (template != null && !string.IsNullOrWhiteSpace(template.Name))
                        _templates.Add(template);
                }
                catch
                {
                    // Ungültige Ressource überspringen
                }
            }

            _templates.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}