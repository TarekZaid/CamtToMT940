using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CamtToMT940.Models;
using CamtToMT940.Services;

namespace CamtToMT940.Forms
{
    public partial class MainForm : Form
    {
        private static readonly string[] CamtExtensions = { ".xml", ".camt" };
        private static readonly string[] CsvExtensions = { ".csv", ".txt" };

        private readonly ConversionService _camtConverter = new();
        private readonly CsvConversionService _csvConverter = new();
        private readonly CsvTemplateStore _templateStore;

        public MainForm()
        {
            InitializeComponent();

            _templateStore = new CsvTemplateStore();
            LoadTemplates();

            WireUpEvents();
        }

        // --------------------------------------------------------------------
        // Initialisierung
        // --------------------------------------------------------------------

        private void LoadTemplates()
        {
            cmbTemplates.Items.Clear();
            foreach (var t in _templateStore.Templates)
                cmbTemplates.Items.Add(t.Name);

            if (cmbTemplates.Items.Count > 0)
                cmbTemplates.SelectedIndex = 0;
            else
                cmbTemplates.Items.Add("(keine Templates gefunden)");
        }

        private void WireUpEvents()
        {
            // CAMT
            btnSelectFolder.Click += (s, e) => SelectFolderFor(lstFiles, CamtExtensions);
            btnClear.Click += (s, e) => { lstFiles.Items.Clear(); UpdateCamtButtonState(); };
            btnConvert.Click += BtnConvert_Click;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
            lstFiles.DragEnter += MainForm_DragEnter;
            lstFiles.DragDrop += MainForm_DragDrop;
            lstFiles.SelectedIndexChanged += (s, e) => UpdateCamtButtonState();

            // CSV
            btnCsvSelectFolder.Click += (s, e) => SelectFolderFor(lstCsvFiles, CsvExtensions);
            btnCsvClear.Click += (s, e) => { lstCsvFiles.Items.Clear(); UpdateCsvButtonState(); };
            btnCsvConvert.Click += BtnCsvConvert_Click;
            lstCsvFiles.DragEnter += MainForm_DragEnter;
            lstCsvFiles.DragDrop += MainForm_DragDrop;
            lstCsvFiles.SelectedIndexChanged += (s, e) => UpdateCsvButtonState();

            // Rechtsklick-Einfügen für die Eingabe-ListBoxen
            var pasteMenu = new ContextMenuStrip();
            pasteMenu.Items.Add("Einfügen (Strg+V)", null, (s, e) => PasteFromClipboard());
            lstFiles.ContextMenuStrip = pasteMenu;
            lstCsvFiles.ContextMenuStrip = pasteMenu;
        }

        // --------------------------------------------------------------------
        // CAMT-Tab: Konvertierung
        // --------------------------------------------------------------------

        private void BtnConvert_Click(object? sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                UpdateStatus("Keine Dateien zum Konvertieren.");
                return;
            }

            var inputs = lstFiles.Items.Cast<FileEntry>()
                .Select(x => x.FullPath).ToList();

            var succeeded = 0;
            var failed = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var input in inputs)
            {
                var result = _camtConverter.Convert(input, HandleConflict);

                if (result.Success)
                {
                    succeeded++;
                }
                else if (result.ErrorMessage == "Vom Benutzer abgebrochen.")
                {
                    skipped++;
                }
                else
                {
                    errors.Add($"{Path.GetFileName(input)}: {result.ErrorMessage}");
                    failed++;
                }
            }

            UpdateStatus($"CAMT: {succeeded} konvertiert, {skipped} übersprungen, {failed} fehlgeschlagen.");
            ShowErrors(errors);
        }

        // --------------------------------------------------------------------
        // CSV-Tab: Konvertierung
        // --------------------------------------------------------------------

        private void BtnCsvConvert_Click(object? sender, EventArgs e)
        {
            if (lstCsvFiles.Items.Count == 0)
            {
                UpdateStatus("Keine Dateien zum Konvertieren.");
                return;
            }

            var templateName = cmbTemplates.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(templateName) || templateName.StartsWith("("))
            {
                MessageBox.Show("Bitte ein Bank-Template auswählen.",
                    "Kein Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var template = _templateStore.Get(templateName);
            if (template == null)
            {
                MessageBox.Show($"Template '{templateName}' nicht gefunden.",
                    "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Optionale manuelle Angaben (leer = nicht verwendet)
            string? iban = string.IsNullOrWhiteSpace(txtIban.Text) ? null : txtIban.Text.Trim();
            string? blz = string.IsNullOrWhiteSpace(txtBlz.Text) ? null : txtBlz.Text.Trim();
            string? konto = string.IsNullOrWhiteSpace(txtKonto.Text) ? null : txtKonto.Text.Trim();
            decimal? openingOverride = ParseDecimalOrNull(txtOpening.Text);
            decimal? closingOverride = ParseDecimalOrNull(txtClosing.Text);

            var inputs = lstCsvFiles.Items.Cast<FileEntry>()
                .Select(x => x.FullPath).ToList();

            var succeeded = 0;
            var failed = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var input in inputs)
            {
                var result = _csvConverter.Convert(input, template, HandleConflict,
                    iban, blz, konto, openingOverride, closingOverride);

                if (result.Success)
                {
                    succeeded++;
                }
                else if (result.ErrorMessage == "Vom Benutzer abgebrochen.")
                {
                    skipped++;
                }
                else
                {
                    errors.Add($"{Path.GetFileName(input)}: {result.ErrorMessage}");
                    failed++;
                }
            }

            UpdateStatus($"CSV ({templateName}): {succeeded} konvertiert, {skipped} übersprungen, {failed} fehlgeschlagen.");
            ShowErrors(errors);
        }

        // --------------------------------------------------------------------
        // Dateiauswahl (gemeinsam für beide Tabs)
        // --------------------------------------------------------------------

        private void SelectFolderFor(ListBox target, string[] allowedExtensions)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Ordner mit zu konvertierenden Dateien auswählen",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            AddPathsTo(target, new[] { dialog.SelectedPath }, allowedExtensions);
        }

        private void AddPathsTo(ListBox target, IEnumerable<string> paths,
            string[] allowedExtensions)
        {
            var added = 0;
            var skipped = 0;

            foreach (var path in paths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.EnumerateFiles(path)
                            .Where(f => allowedExtensions.Contains(
                                Path.GetExtension(f).ToLowerInvariant()));
                        foreach (var f in files)
                        {
                            if (AddFileToList(target, f)) added++;
                            else skipped++;
                        }
                    }
                    else if (File.Exists(path))
                    {
                        if (allowedExtensions.Contains(
                                Path.GetExtension(path).ToLowerInvariant()))
                        {
                            if (AddFileToList(target, path)) added++;
                            else skipped++;
                        }
                        else skipped++;
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Fehler: {ex.Message}");
                }
            }

            UpdateStatus($"{added} Datei(en) hinzugefügt, {skipped} übersprungen.");
            UpdateCamtButtonState();
            UpdateCsvButtonState();
        }

        private static bool AddFileToList(ListBox target, string filePath)
        {
            var full = Path.GetFullPath(filePath);
            foreach (var item in target.Items)
            {
                if (item is FileEntry fe && fe.FullPath == full) return false;
            }
            target.Items.Add(new FileEntry(full));
            return true;
        }

        // --------------------------------------------------------------------
        // Drag & Drop
        // --------------------------------------------------------------------

        private void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            var paths = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (paths == null) return;

            AddPathsToBothLists(paths);
        }

        // --------------------------------------------------------------------
        // Einfügen (Strg+V und Rechtsklick)
        // --------------------------------------------------------------------

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.V))
            {
                PasteFromClipboard();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PasteFromClipboard()
        {
            var paths = new List<string>();

            if (Clipboard.ContainsFileDropList())
            {
                foreach (var file in Clipboard.GetFileDropList())
                {
                    if (!string.IsNullOrWhiteSpace(file))
                        paths.Add(file);
                }
            }

            if (Clipboard.ContainsText())
            {
                var textPaths = Clipboard.GetText()
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().Trim('"'))
                    .Where(p => !string.IsNullOrWhiteSpace(p));

                paths.AddRange(textPaths);
            }

            if (paths.Count == 0)
            {
                UpdateStatus("Zwischenablage enthält keine gültigen Pfade.");
                return;
            }

            var distinct = paths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            AddPathsToBothLists(distinct);
        }

        private void AddPathsToBothLists(IEnumerable<string> paths)
        {
            var list = paths.ToList();
            AddPathsTo(lstFiles, list, CamtExtensions);
            AddPathsTo(lstCsvFiles, list, CsvExtensions);
        }

        // --------------------------------------------------------------------
        // Überschreiben-Dialog
        // --------------------------------------------------------------------

        private bool HandleConflict(string outputPath)
        {
            var result = MessageBox.Show(
                $"Die Datei existiert bereits:\n{outputPath}\n\nÜberschreiben?",
                "Datei überschreiben?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        // --------------------------------------------------------------------
        // UI-Hilfsmethoden
        // --------------------------------------------------------------------

        private void UpdateStatus(string message)
        {
            if (InvokeRequired) { Invoke(() => UpdateStatus(message)); return; }
            statusLabel.Text = message;
        }

        private void UpdateCamtButtonState()
            => btnConvert.Enabled = lstFiles.Items.Count > 0;

        private void UpdateCsvButtonState()
            => btnCsvConvert.Enabled = lstCsvFiles.Items.Count > 0
                && cmbTemplates.SelectedIndex >= 0;

        private void ShowErrors(List<string> errors)
        {
            if (errors.Count == 0) return;

            MessageBox.Show(
                "Folgende Dateien konnten nicht konvertiert werden:\n\n" +
                string.Join("\n", errors),
                "Fehler bei der Konvertierung",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Parst einen optionalen Betrag aus einem TextBox-Inhalt.
        /// Leer → null. Deutsche Schreibweise (Komma) wird unterstützt.
        /// </summary>
        private static decimal? ParseDecimalOrNull(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var cleaned = new string(text
                .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-')
                .ToArray());

            var german = CultureInfo.GetCultureInfo("de-DE");
            if (decimal.TryParse(cleaned, NumberStyles.Any, german, out var value))
                return value;

            var fallback = cleaned.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(fallback, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return value;

            return null;
        }
    }
}