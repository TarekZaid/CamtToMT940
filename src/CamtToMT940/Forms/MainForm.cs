using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CamtToMT940
{
    public partial class MainForm : Form
    {
        // Unterstützte Eingabeformate
        private static readonly string[] SupportedExtensions = { ".xml", ".camt", ".csv" };

        private readonly Services.ConversionService _converter = new();
        private bool _suppressOverwriteWarning = false;

        public MainForm()
        {
            InitializeComponent();
            WireUpEvents();
            TestParser();
        }

        /// <summary>
        /// Verknüpft die Events der Steuerelemente mit den Handler-Methoden.
        /// </summary>
        private void WireUpEvents()
        {
            btnSelectFolder.Click += BtnSelectFolder_Click;
            btnClear.Click += BtnClear_Click;
            btnConvert.Click += BtnConvert_Click;

            // Drag & Drop auf dem Formular
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;

            // Drag & Drop auch auf der ListBox
            lstFiles.DragEnter += MainForm_DragEnter;
            lstFiles.DragDrop += MainForm_DragDrop;

            // Strg+V zum Einfügen von Pfaden
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // ListBox-Änderungen beobachten
            lstFiles.SelectedIndexChanged += (s, e) => UpdateButtonState();

            lstFiles.DoubleClick += (s, e) =>
            {
                if (lstFiles.SelectedItem is Models.FileEntry entry)
                { System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{entry.FullPath}\""); }
            };
        }

        // --------------------------------------------------------------------
        // Buttons
        // --------------------------------------------------------------------

        private void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Ordner mit CAMT- oder CSV-Dateien auswählen",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                AddPaths(new[] { dialog.SelectedPath });
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            lstFiles.Items.Clear();
            UpdateStatus("Liste geleert.");
            UpdateButtonState();
        }

        private void BtnConvert_Click(object? sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                UpdateStatus("Keine Dateien zum Konvertieren vorhanden.");
                return;
            }

            var inputs = lstFiles.Items
                .Cast<Models.FileEntry>()
                .Select(e => e.FullPath)
                .ToList(); var succeeded = 0;
            var failed = 0;
            var skipped = 0;
            var errorMessages = new List<string>();

            lstOutput.Items.Clear();

            foreach (var input in inputs)
            {
                var result = _converter.Convert(input, HandleConflict);

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
                    errorMessages.Add($"{Path.GetFileName(input)}: {result.ErrorMessage}");
                    failed++;
                }
            }

            UpdateStatus($"Fertig: {succeeded} konvertiert, {skipped} übersprungen, {failed} fehlgeschlagen.");

            // Fehlerdetails in eigenem Fenster anzeigen, falls welche vorhanden
            if (errorMessages.Count > 0)
            {
                MessageBox.Show(
                    "Folgende Dateien konnten nicht konvertiert werden:\n\n" +
                    string.Join("\n", errorMessages),
                    "Fehler bei der Konvertierung",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private bool HandleConflict(string outputPath)
        {
            if (_suppressOverwriteWarning) return true;

            var result = MessageBox.Show(
                $"Die Datei existiert bereits:\n{outputPath}\n\nÜberschreiben?",
                "Datei überschreiben?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
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
            if (paths != null)
                AddPaths(paths);
        }

        // --------------------------------------------------------------------
        // Strg+V: Pfade aus der Zwischenablage einfügen
        // --------------------------------------------------------------------

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    var paths = text
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim().Trim('"'))
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToArray();

                    if (paths.Length > 0)
                        AddPaths(paths);
                }
                e.Handled = true;
            }
        }

        // --------------------------------------------------------------------
        // Pfade verarbeiten (Dateien + Ordner)
        // --------------------------------------------------------------------

        private void AddPaths(IEnumerable<string> paths)
        {
            var added = 0;
            var skipped = 0;

            foreach (var path in paths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        // Alle unterstützten Dateien im Ordner (nicht rekursiv)
                        var files = Directory.EnumerateFiles(path)
                            .Where(f => SupportedExtensions.Contains(
                                Path.GetExtension(f).ToLowerInvariant()));

                        foreach (var file in files)
                        {
                            if (AddFile(file)) added++;
                            else skipped++;
                        }
                    }
                    else if (File.Exists(path))
                    {
                        if (IsSupported(path))
                        {
                            if (AddFile(path)) added++;
                            else skipped++;
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Fehler beim Verarbeiten von '{path}': {ex.Message}");
                }
            }

            UpdateStatus($"{added} Datei(en) hinzugefügt, {skipped} übersprungen.");
            UpdateButtonState();
        }

        /// <summary>
        /// Fügt eine Datei zur ListBox hinzu, wenn sie nicht schon enthalten ist.
        /// </summary>
        private bool AddFile(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);

            // Prüfen, ob schon vorhanden
            foreach (var item in lstFiles.Items)
            {
                if (item is Models.FileEntry entry && entry.FullPath == fullPath)
                    return false;
            }

            lstFiles.Items.Add(new Models.FileEntry(fullPath));
            return true;
        }

        private static bool IsSupported(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return SupportedExtensions.Contains(ext);
        }

        // --------------------------------------------------------------------
        // UI-Hilfsmethoden
        // --------------------------------------------------------------------

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateStatus(message));
                return;
            }
            statusLabel.Text = message;
        }

        private void UpdateButtonState()
        {
            btnConvert.Enabled = lstFiles.Items.Count > 0;
        }

        private void TestParser()
        {
            var parser = new Services.CamtParser();
            var statements = parser.Parse(@"D:\GitHub\CamtToMT940\testdata\2026_C52_VR.xml");

            foreach (var stmt in statements)
            {
                System.Diagnostics.Debug.WriteLine($"IBAN: {stmt.Iban}");
                System.Diagnostics.Debug.WriteLine($"Anfangssaldo: {stmt.OpeningBalance} {stmt.Currency} ({stmt.OpeningDate})");
                System.Diagnostics.Debug.WriteLine($"Endsaldo: {stmt.ClosingBalance} {stmt.Currency}");
                System.Diagnostics.Debug.WriteLine($"Anzahl Buchungen: {stmt.Transactions.Count}");

                foreach (var tx in stmt.Transactions)
                {
                    var partner = tx.IsCredit ? tx.DebtorName : tx.CreditorName;
                    System.Diagnostics.Debug.WriteLine($"  {tx.BookingDate:yyyy-MM-dd} | {tx.Amount} | {(tx.IsCredit ? "CRDT" : "DBIT")} | {partner}");
                }
            }
        }
    }
}