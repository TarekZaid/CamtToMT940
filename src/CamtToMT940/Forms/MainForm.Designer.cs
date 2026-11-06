namespace CamtToMT940.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabCamt = new TabPage();
            tabCsv = new TabPage();

            // CAMT-Tab
            groupBoxInput = new GroupBox();
            btnConvert = new Button();
            btnClear = new Button();
            btnSelectFolder = new Button();
            panelInputHeader = new Panel();
            lblInputHeader = new Label();
            lblHint = new Label();
            lstFiles = new ListBox();

            // CSV-Tab
            groupBoxCsvInput = new GroupBox();
            lblTemplate = new Label();
            cmbTemplates = new ComboBox();
            lblYear = new Label();
            cmbYear = new ComboBox();
            btnCsvConvert = new Button();
            btnCsvClear = new Button();
            btnCsvSelectFolder = new Button();
            panelCsvInputHeader = new Panel();
            lblCsvInputHeader = new Label();
            lblCsvHint = new Label();
            splitContainerCsv = new SplitContainer();
            lstCsvFiles = new ListBox();
            groupBoxManualInput = new GroupBox();
            lblIban = new Label();
            txtIban = new TextBox();
            lblBlz = new Label();
            txtBlz = new TextBox();
            lblKonto = new Label();
            txtKonto = new TextBox();
            lblOpening = new Label();
            txtOpening = new TextBox();
            lblClosing = new Label();
            txtClosing = new TextBox();

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();

            tabControl.SuspendLayout();
            tabCamt.SuspendLayout();
            tabCsv.SuspendLayout();
            groupBoxInput.SuspendLayout();
            groupBoxCsvInput.SuspendLayout();
            panelInputHeader.SuspendLayout();
            panelCsvInputHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerCsv).BeginInit();
            splitContainerCsv.Panel1.SuspendLayout();
            splitContainerCsv.Panel2.SuspendLayout();
            splitContainerCsv.SuspendLayout();
            groupBoxManualInput.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // ----------------------------------------------------------------
            // tabControl
            // ----------------------------------------------------------------
            tabControl.Controls.Add(tabCamt);
            tabControl.Controls.Add(tabCsv);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(884, 511);
            tabControl.TabIndex = 0;

            // ----------------------------------------------------------------
            // tabCamt
            // ----------------------------------------------------------------
            tabCamt.Controls.Add(lstFiles);
            tabCamt.Controls.Add(panelInputHeader);
            tabCamt.Controls.Add(groupBoxInput);
            tabCamt.Location = new Point(4, 29);
            tabCamt.Name = "tabCamt";
            tabCamt.Padding = new Padding(3);
            tabCamt.Size = new Size(876, 478);
            tabCamt.TabIndex = 0;
            tabCamt.Text = "CAMT → MT940";
            tabCamt.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // groupBoxInput (CAMT) — nur noch Buttons
            // ----------------------------------------------------------------
            groupBoxInput.Controls.Add(btnConvert);
            groupBoxInput.Controls.Add(btnClear);
            groupBoxInput.Controls.Add(btnSelectFolder);
            groupBoxInput.Dock = DockStyle.Top;
            groupBoxInput.Location = new Point(3, 3);
            groupBoxInput.Name = "groupBoxInput";
            groupBoxInput.Size = new Size(870, 70);
            groupBoxInput.TabIndex = 0;
            groupBoxInput.TabStop = false;
            groupBoxInput.Text = "CAMT.052 / CAMT.053 – Funktionen";

            btnConvert.Enabled = false;
            btnConvert.Location = new Point(315, 25);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new Size(150, 30);
            btnConvert.Text = "Konvertieren";
            btnConvert.UseVisualStyleBackColor = true;

            btnClear.Location = new Point(180, 25);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(120, 30);
            btnClear.Text = "Liste leeren";
            btnClear.UseVisualStyleBackColor = true;

            btnSelectFolder.Location = new Point(15, 25);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(150, 30);
            btnSelectFolder.Text = "Ordner auswählen...";
            btnSelectFolder.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // panelInputHeader (CAMT) — "Eingabe" + Hinweis nebeneinander
            // ----------------------------------------------------------------
            panelInputHeader.Controls.Add(lblHint);
            panelInputHeader.Controls.Add(lblInputHeader);
            panelInputHeader.Dock = DockStyle.Top;
            panelInputHeader.Location = new Point(3, 73);
            panelInputHeader.Name = "panelInputHeader";
            panelInputHeader.Size = new Size(870, 30);
            panelInputHeader.TabIndex = 1;

            lblInputHeader.AutoSize = true;
            lblInputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblInputHeader.Location = new Point(5, 5);
            lblInputHeader.Name = "lblInputHeader";
            lblInputHeader.Text = "Eingabe";

            lblHint.AutoSize = true;
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(80, 6);
            lblHint.Name = "lblHint";
            lblHint.Text = "Dateien oder Ordner hierher ziehen oder mit Strg+V einfügen";

            // ----------------------------------------------------------------
            // lstFiles (CAMT) — Full-Width, Dock=Fill
            // ----------------------------------------------------------------
            lstFiles.AllowDrop = true;
            lstFiles.Dock = DockStyle.Fill;
            lstFiles.FormattingEnabled = true;
            lstFiles.IntegralHeight = false;
            lstFiles.Location = new Point(3, 103);
            lstFiles.Name = "lstFiles";
            lstFiles.SelectionMode = SelectionMode.MultiExtended;
            lstFiles.Size = new Size(870, 372);
            lstFiles.TabIndex = 2;

            // ----------------------------------------------------------------
            // tabCsv
            // ----------------------------------------------------------------
            tabCsv.Controls.Add(splitContainerCsv);
            tabCsv.Controls.Add(panelCsvInputHeader);
            tabCsv.Controls.Add(groupBoxCsvInput);
            tabCsv.Location = new Point(4, 29);
            tabCsv.Name = "tabCsv";
            tabCsv.Padding = new Padding(3);
            tabCsv.Size = new Size(876, 478);
            tabCsv.TabIndex = 1;
            tabCsv.Text = "CSV → MT940";
            tabCsv.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // groupBoxCsvInput — nur noch Template + Buttons
            // ----------------------------------------------------------------
            groupBoxCsvInput.Controls.Add(cmbTemplates);
            groupBoxCsvInput.Controls.Add(lblTemplate);
            groupBoxCsvInput.Controls.Add(btnCsvConvert);
            groupBoxCsvInput.Controls.Add(btnCsvClear);
            groupBoxCsvInput.Controls.Add(btnCsvSelectFolder);
            groupBoxCsvInput.Dock = DockStyle.Top;
            groupBoxCsvInput.Location = new Point(3, 3);
            groupBoxCsvInput.Name = "groupBoxCsvInput";
            groupBoxCsvInput.Size = new Size(870, 70);
            groupBoxCsvInput.TabIndex = 0;
            groupBoxCsvInput.TabStop = false;
            groupBoxCsvInput.Text = "CSV – Funktionen";

            lblTemplate.AutoSize = true;
            lblTemplate.Location = new Point(15, 30);
            lblTemplate.Name = "lblTemplate";
            lblTemplate.Size = new Size(70, 20);
            lblTemplate.Text = "Bank:";

            cmbTemplates.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTemplates.FormattingEnabled = true;
            cmbTemplates.Location = new Point(90, 27);
            cmbTemplates.Name = "cmbTemplates";
            cmbTemplates.Size = new Size(220, 28);
            cmbTemplates.TabIndex = 0;

            btnCsvSelectFolder.Location = new Point(330, 25);
            btnCsvSelectFolder.Name = "btnCsvSelectFolder";
            btnCsvSelectFolder.Size = new Size(150, 30);
            btnCsvSelectFolder.Text = "Ordner auswählen...";
            btnCsvSelectFolder.UseVisualStyleBackColor = true;

            btnCsvClear.Location = new Point(495, 25);
            btnCsvClear.Name = "btnCsvClear";
            btnCsvClear.Size = new Size(120, 30);
            btnCsvClear.Text = "Liste leeren";
            btnCsvClear.UseVisualStyleBackColor = true;

            btnCsvConvert.Enabled = false;
            btnCsvConvert.Location = new Point(630, 25);
            btnCsvConvert.Name = "btnCsvConvert";
            btnCsvConvert.Size = new Size(150, 30);
            btnCsvConvert.Text = "Konvertieren";
            btnCsvConvert.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // panelCsvInputHeader (CSV) — "Eingabe" + Hinweis nebeneinander
            // ----------------------------------------------------------------
            panelCsvInputHeader.Controls.Add(lblCsvHint);
            panelCsvInputHeader.Controls.Add(lblCsvInputHeader);
            panelCsvInputHeader.Dock = DockStyle.Top;
            panelCsvInputHeader.Location = new Point(3, 73);
            panelCsvInputHeader.Name = "panelCsvInputHeader";
            panelCsvInputHeader.Size = new Size(870, 30);
            panelCsvInputHeader.TabIndex = 1;

            lblCsvInputHeader.AutoSize = true;
            lblCsvInputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCsvInputHeader.Location = new Point(5, 5);
            lblCsvInputHeader.Name = "lblCsvInputHeader";
            lblCsvInputHeader.Text = "Eingabe";

            lblCsvHint.AutoSize = true;
            lblCsvHint.ForeColor = SystemColors.GrayText;
            lblCsvHint.Location = new Point(80, 6);
            lblCsvHint.Name = "lblCsvHint";
            lblCsvHint.Text = "Dateien oder Ordner hierher ziehen oder mit Strg+V einfügen";

            // ----------------------------------------------------------------
            // splitContainerCsv
            // ----------------------------------------------------------------
            splitContainerCsv.Dock = DockStyle.Fill;
            splitContainerCsv.Location = new Point(3, 103);
            splitContainerCsv.Name = "splitContainerCsv";
            splitContainerCsv.Panel1.Controls.Add(lstCsvFiles);
            splitContainerCsv.Panel1MinSize = 200;
            splitContainerCsv.Panel2.Controls.Add(groupBoxManualInput);
            splitContainerCsv.Panel2MinSize = 300;
            splitContainerCsv.Size = new Size(870, 372);
            splitContainerCsv.SplitterDistance = 540;
            splitContainerCsv.SplitterWidth = 6;
            splitContainerCsv.IsSplitterFixed = true;
            splitContainerCsv.TabIndex = 2;

            lstCsvFiles.AllowDrop = true;
            lstCsvFiles.Dock = DockStyle.Fill;
            lstCsvFiles.FormattingEnabled = true;
            lstCsvFiles.IntegralHeight = false;
            lstCsvFiles.Location = new Point(0, 0);
            lstCsvFiles.Name = "lstCsvFiles";
            lstCsvFiles.SelectionMode = SelectionMode.MultiExtended;
            lstCsvFiles.Size = new Size(540, 372);
            lstCsvFiles.TabIndex = 0;

            // ----------------------------------------------------------------
            // groupBoxManualInput (rechts) — fünf Felder untereinander
            // ----------------------------------------------------------------
            groupBoxManualInput.Controls.Add(lblIban);
            groupBoxManualInput.Controls.Add(txtIban);
            groupBoxManualInput.Controls.Add(lblBlz);
            groupBoxManualInput.Controls.Add(txtBlz);
            groupBoxManualInput.Controls.Add(lblKonto);
            groupBoxManualInput.Controls.Add(txtKonto);
            groupBoxManualInput.Controls.Add(lblOpening);
            groupBoxManualInput.Controls.Add(txtOpening);
            groupBoxManualInput.Controls.Add(lblClosing);
            groupBoxManualInput.Controls.Add(txtClosing);
            groupBoxManualInput.Controls.Add(lblYear);
            groupBoxManualInput.Controls.Add(cmbYear);
            groupBoxManualInput.Dock = DockStyle.Fill;
            groupBoxManualInput.Location = new Point(0, 0);
            groupBoxManualInput.Name = "groupBoxManualInput";
            groupBoxManualInput.Size = new Size(324, 372);
            groupBoxManualInput.TabIndex = 0;
            groupBoxManualInput.TabStop = false;
            groupBoxManualInput.Text = "Manuelle Angaben (optional)";

            lblIban.AutoSize = true;
            lblIban.Location = new Point(15, 35);
            lblIban.Name = "lblIban";
            lblIban.Text = "IBAN:";

            txtIban.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIban.Location = new Point(15, 58);
            txtIban.Name = "txtIban";
            txtIban.Size = new Size(294, 27);
            txtIban.MaxLength = 34;
            txtIban.PlaceholderText = "DE... (überschreibt alles)";

            lblBlz.AutoSize = true;
            lblBlz.Location = new Point(15, 95);
            lblBlz.Name = "lblBlz";
            lblBlz.Text = "BLZ:";

            txtBlz.Location = new Point(15, 118);
            txtBlz.Name = "txtBlz";
            txtBlz.Size = new Size(140, 27);
            txtBlz.MaxLength = 8;
            txtBlz.PlaceholderText = "8 Ziffern";

            lblKonto.AutoSize = true;
            lblKonto.Location = new Point(170, 95);
            lblKonto.Name = "lblKonto";
            lblKonto.Text = "Kontonr.:";

            txtKonto.Location = new Point(170, 118);
            txtKonto.Name = "txtKonto";
            txtKonto.Size = new Size(139, 27);
            txtKonto.MaxLength = 10;
            txtKonto.PlaceholderText = "max. 10 Ziffern";

            lblOpening.AutoSize = true;
            lblOpening.Location = new Point(15, 155);
            lblOpening.Name = "lblOpening";
            lblOpening.Text = "Anfangssaldo:";

            txtOpening.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOpening.Location = new Point(15, 178);
            txtOpening.Name = "txtOpening";
            txtOpening.Size = new Size(294, 27);
            txtOpening.PlaceholderText = "z.B. 1234,56 (überschreibt Datei-Wert)";

            lblClosing.AutoSize = true;
            lblClosing.Location = new Point(15, 215);
            lblClosing.Name = "lblClosing";
            lblClosing.Text = "Endsaldo:";

            txtClosing.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtClosing.Location = new Point(15, 238);
            txtClosing.Name = "txtClosing";
            txtClosing.Size = new Size(294, 27);
            txtClosing.PlaceholderText = "z.B. 2345,67 (überschreibt Datei-Wert)";

            lblYear.AutoSize = true;
            lblYear.Location = new Point(15, 275);
            lblYear.Name = "lblYear";
            lblYear.Text = "Nur Jahr:";

            cmbYear.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbYear.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbYear.FormattingEnabled = true;
            cmbYear.Location = new Point(15, 298);
            cmbYear.Name = "cmbYear";
            cmbYear.Size = new Size(294, 28);

            // Feste Jahresliste für den optionalen Jahresfilter
            cmbYear.Items.Add("(alle Jahre)");
            for (int y = 2020; y <= 2030; y++)
                cmbYear.Items.Add(y);
            cmbYear.SelectedIndex = 0;

            // ----------------------------------------------------------------
            // statusStrip
            // ----------------------------------------------------------------
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 511);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(884, 26);
            statusStrip.TabIndex = 3;

            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(48, 20);
            statusLabel.Text = "Bereit";

            // ----------------------------------------------------------------
            // MainForm
            // ----------------------------------------------------------------
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 537);
            Controls.Add(tabControl);
            Controls.Add(statusStrip);
            MinimumSize = new Size(800, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CAMT/CSV zu MT940 Konverter";

            tabControl.ResumeLayout(false);
            tabCamt.ResumeLayout(false);
            tabCamt.PerformLayout();
            tabCsv.ResumeLayout(false);
            groupBoxInput.ResumeLayout(false);
            groupBoxCsvInput.ResumeLayout(false);
            groupBoxCsvInput.PerformLayout();
            panelInputHeader.ResumeLayout(false);
            panelInputHeader.PerformLayout();
            panelCsvInputHeader.ResumeLayout(false);
            panelCsvInputHeader.PerformLayout();
            splitContainerCsv.Panel1.ResumeLayout(false);
            splitContainerCsv.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerCsv).EndInit();
            splitContainerCsv.ResumeLayout(false);
            groupBoxManualInput.ResumeLayout(false);
            groupBoxManualInput.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // ====================================================================
        // Felder
        // ====================================================================
        private TabControl tabControl;
        private TabPage tabCamt;
        private TabPage tabCsv;

        // CAMT
        private GroupBox groupBoxInput;
        private Button btnSelectFolder;
        private Button btnConvert;
        private Button btnClear;
        private Panel panelInputHeader;
        private Label lblInputHeader;
        private Label lblHint;
        private ListBox lstFiles;

        // CSV
        private GroupBox groupBoxCsvInput;
        private Label lblTemplate;
        private ComboBox cmbTemplates;
        private Button btnCsvConvert;
        private Button btnCsvClear;
        private Button btnCsvSelectFolder;
        private Panel panelCsvInputHeader;
        private Label lblCsvInputHeader;
        private Label lblCsvHint;
        private SplitContainer splitContainerCsv;
        private ListBox lstCsvFiles;
        private GroupBox groupBoxManualInput;
        private Label lblIban;
        private TextBox txtIban;
        private Label lblBlz;
        private TextBox txtBlz;
        private Label lblKonto;
        private TextBox txtKonto;
        private Label lblOpening;
        private TextBox txtOpening;
        private Label lblClosing;
        private TextBox txtClosing;
        private Label lblYear;
        private ComboBox cmbYear;

        // Status
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}