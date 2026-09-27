using System;
using System.Collections.Generic;
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

            // Templates laden (aus Ordner neben der .exe bzw. aus Projektordner)
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

            // Strg+V
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
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

            lstOutput.Items.Clear();

            foreach (var input in inputs)
            {
                var result = _camtConverter.Convert(input, HandleConflict);

                if (result.Success)
                {
                    lstOutput.Items.Add(Path.GetFileName(result.OutputPath));
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

            var inputs = lstCsvFiles.Items.Cast<FileEntry>()
                .Select(x => x.FullPath).ToList();

            var succeeded = 0;
            var failed = 0;
            var skipped = 0;
            var errors = new List<string>();

            lstCsvOutput.Items.Clear();

            foreach (var input in inputs)
            {
                var result = _csvConverter.Convert(input, template, HandleConflict);

                if (result.Success)
                {
                    lstCsvOutput.Items.Add(Path.GetFileName(result.OutputPath));
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

            // Bestimmen, in welchen Tab die Dateien kommen
            // ? wir fügen sie in beide möglichen Listen ein, wenn die Extension passt
            AddPathsTo(lstFiles, paths, CamtExtensions);
            AddPathsTo(lstCsvFiles, paths, CsvExtensions);
        }

        // --------------------------------------------------------------------
        // Strg+V
        // --------------------------------------------------------------------

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsText())
                {
                    var paths = Clipboard.GetText()
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim().Trim('"'))
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToArray();

                    if (paths.Length > 0)
                    {
                        AddPathsTo(lstFiles, paths, CamtExtensions);
                        AddPathsTo(lstCsvFiles, paths, CsvExtensions);
                    }
                }
                e.Handled = true;
            }
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
    }
}