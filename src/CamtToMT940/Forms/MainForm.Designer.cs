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
            lblHint = new Label();
            btnConvert = new Button();
            btnClear = new Button();
            btnSelectFolder = new Button();
            splitContainer = new SplitContainer();
            lstFiles = new ListBox();
            lblInputHeader = new Label();
            lstOutput = new ListBox();
            lblOutputHeader = new Label();

            // CSV-Tab
            groupBoxCsvInput = new GroupBox();
            lblCsvHint = new Label();
            lblTemplate = new Label();
            cmbTemplates = new ComboBox();
            btnCsvConvert = new Button();
            btnCsvClear = new Button();
            btnCsvSelectFolder = new Button();
            splitContainerCsv = new SplitContainer();
            lstCsvFiles = new ListBox();
            lblCsvInputHeader = new Label();
            lstCsvOutput = new ListBox();
            lblCsvOutputHeader = new Label();

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();

            tabControl.SuspendLayout();
            tabCamt.SuspendLayout();
            tabCsv.SuspendLayout();
            groupBoxInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            groupBoxCsvInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerCsv).BeginInit();
            splitContainerCsv.Panel1.SuspendLayout();
            splitContainerCsv.Panel2.SuspendLayout();
            splitContainerCsv.SuspendLayout();
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
            tabCamt.Controls.Add(splitContainer);
            tabCamt.Controls.Add(groupBoxInput);
            tabCamt.Location = new Point(4, 29);
            tabCamt.Name = "tabCamt";
            tabCamt.Padding = new Padding(3);
            tabCamt.Size = new Size(876, 478);
            tabCamt.TabIndex = 0;
            tabCamt.Text = "CAMT → MT940";
            tabCamt.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // groupBoxInput (CAMT)
            // ----------------------------------------------------------------
            groupBoxInput.Controls.Add(lblHint);
            groupBoxInput.Controls.Add(btnConvert);
            groupBoxInput.Controls.Add(btnClear);
            groupBoxInput.Controls.Add(btnSelectFolder);
            groupBoxInput.Dock = DockStyle.Top;
            groupBoxInput.Location = new Point(3, 3);
            groupBoxInput.Name = "groupBoxInput";
            groupBoxInput.Size = new Size(870, 100);
            groupBoxInput.TabIndex = 0;
            groupBoxInput.TabStop = false;
            groupBoxInput.Text = "Eingabe (CAMT.052 / CAMT.053)";

            lblHint.AutoSize = true;
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(15, 70);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(600, 20);
            lblHint.Text = "Dateien oder Ordner hierher ziehen (oder Pfade einfügen mit Strg+V)";

            btnConvert.Enabled = false;
            btnConvert.Location = new Point(315, 30);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new Size(150, 30);
            btnConvert.Text = "Konvertieren";
            btnConvert.UseVisualStyleBackColor = true;

            btnClear.Location = new Point(180, 30);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(120, 30);
            btnClear.Text = "Liste leeren";
            btnClear.UseVisualStyleBackColor = true;

            btnSelectFolder.Location = new Point(15, 30);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(150, 30);
            btnSelectFolder.Text = "Ordner auswählen...";
            btnSelectFolder.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // splitContainer (CAMT)
            // ----------------------------------------------------------------
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(3, 103);
            splitContainer.Name = "splitContainer";
            splitContainer.Panel1.Controls.Add(lstFiles);
            splitContainer.Panel1.Controls.Add(lblInputHeader);
            splitContainer.Panel1MinSize = 200;
            splitContainer.Panel2.Controls.Add(lstOutput);
            splitContainer.Panel2.Controls.Add(lblOutputHeader);
            splitContainer.Panel2MinSize = 200;
            splitContainer.Size = new Size(870, 372);
            splitContainer.SplitterDistance = 433;
            splitContainer.SplitterWidth = 6;
            splitContainer.TabIndex = 1;

            lstFiles.AllowDrop = true;
            lstFiles.Dock = DockStyle.Fill;
            lstFiles.FormattingEnabled = true;
            lstFiles.IntegralHeight = false;
            lstFiles.Location = new Point(0, 30);
            lstFiles.Name = "lstFiles";
            lstFiles.SelectionMode = SelectionMode.MultiExtended;
            lstFiles.Size = new Size(433, 342);
            lstFiles.TabIndex = 1;

            lblInputHeader.Dock = DockStyle.Top;
            lblInputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblInputHeader.Location = new Point(0, 0);
            lblInputHeader.Name = "lblInputHeader";
            lblInputHeader.Padding = new Padding(5, 5, 0, 0);
            lblInputHeader.Size = new Size(433, 30);
            lblInputHeader.Text = "Eingabe";

            lstOutput.AllowDrop = true;
            lstOutput.Dock = DockStyle.Fill;
            lstOutput.FormattingEnabled = true;
            lstOutput.IntegralHeight = false;
            lstOutput.Location = new Point(0, 30);
            lstOutput.Name = "lstOutput";
            lstOutput.SelectionMode = SelectionMode.MultiExtended;
            lstOutput.Size = new Size(431, 342);
            lstOutput.TabIndex = 1;

            lblOutputHeader.Dock = DockStyle.Top;
            lblOutputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblOutputHeader.Location = new Point(0, 0);
            lblOutputHeader.Name = "lblOutputHeader";
            lblOutputHeader.Padding = new Padding(5, 5, 0, 0);
            lblOutputHeader.Size = new Size(431, 30);
            lblOutputHeader.Text = "Ausgabe";

            // ----------------------------------------------------------------
            // tabCsv
            // ----------------------------------------------------------------
            tabCsv.Controls.Add(splitContainerCsv);
            tabCsv.Controls.Add(groupBoxCsvInput);
            tabCsv.Location = new Point(4, 29);
            tabCsv.Name = "tabCsv";
            tabCsv.Padding = new Padding(3);
            tabCsv.Size = new Size(876, 478);
            tabCsv.TabIndex = 1;
            tabCsv.Text = "CSV → MT940";
            tabCsv.UseVisualStyleBackColor = true;

            // ----------------------------------------------------------------
            // groupBoxCsvInput
            // ----------------------------------------------------------------
            groupBoxCsvInput.Controls.Add(cmbTemplates);
            groupBoxCsvInput.Controls.Add(lblTemplate);
            groupBoxCsvInput.Controls.Add(lblCsvHint);
            groupBoxCsvInput.Controls.Add(btnCsvConvert);
            groupBoxCsvInput.Controls.Add(btnCsvClear);
            groupBoxCsvInput.Controls.Add(btnCsvSelectFolder);
            groupBoxCsvInput.Dock = DockStyle.Top;
            groupBoxCsvInput.Location = new Point(3, 3);
            groupBoxCsvInput.Name = "groupBoxCsvInput";
            groupBoxCsvInput.Size = new Size(870, 120);
            groupBoxCsvInput.TabIndex = 0;
            groupBoxCsvInput.TabStop = false;
            groupBoxCsvInput.Text = "Eingabe (CSV)";

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

            lblCsvHint.AutoSize = true;
            lblCsvHint.ForeColor = SystemColors.GrayText;
            lblCsvHint.Location = new Point(15, 80);
            lblCsvHint.Name = "lblCsvHint";
            lblCsvHint.Size = new Size(600, 20);
            lblCsvHint.Text = "Dateien oder Ordner hierher ziehen (oder Pfade einfügen mit Strg+V)";

            // ----------------------------------------------------------------
            // splitContainerCsv
            // ----------------------------------------------------------------
            splitContainerCsv.Dock = DockStyle.Fill;
            splitContainerCsv.Location = new Point(3, 123);
            splitContainerCsv.Name = "splitContainerCsv";
            splitContainerCsv.Panel1.Controls.Add(lstCsvFiles);
            splitContainerCsv.Panel1.Controls.Add(lblCsvInputHeader);
            splitContainerCsv.Panel1MinSize = 200;
            splitContainerCsv.Panel2.Controls.Add(lstCsvOutput);
            splitContainerCsv.Panel2.Controls.Add(lblCsvOutputHeader);
            splitContainerCsv.Panel2MinSize = 200;
            splitContainerCsv.Size = new Size(870, 352);
            splitContainerCsv.SplitterDistance = 433;
            splitContainerCsv.SplitterWidth = 6;
            splitContainerCsv.TabIndex = 1;

            lstCsvFiles.AllowDrop = true;
            lstCsvFiles.Dock = DockStyle.Fill;
            lstCsvFiles.FormattingEnabled = true;
            lstCsvFiles.IntegralHeight = false;
            lstCsvFiles.Location = new Point(0, 30);
            lstCsvFiles.Name = "lstCsvFiles";
            lstCsvFiles.SelectionMode = SelectionMode.MultiExtended;
            lstCsvFiles.Size = new Size(433, 322);
            lstCsvFiles.TabIndex = 1;

            lblCsvInputHeader.Dock = DockStyle.Top;
            lblCsvInputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCsvInputHeader.Location = new Point(0, 0);
            lblCsvInputHeader.Name = "lblCsvInputHeader";
            lblCsvInputHeader.Padding = new Padding(5, 5, 0, 0);
            lblCsvInputHeader.Size = new Size(433, 30);
            lblCsvInputHeader.Text = "Eingabe";

            lstCsvOutput.AllowDrop = true;
            lstCsvOutput.Dock = DockStyle.Fill;
            lstCsvOutput.FormattingEnabled = true;
            lstCsvOutput.IntegralHeight = false;
            lstCsvOutput.Location = new Point(0, 30);
            lstCsvOutput.Name = "lstCsvOutput";
            lstCsvOutput.SelectionMode = SelectionMode.MultiExtended;
            lstCsvOutput.Size = new Size(431, 322);
            lstCsvOutput.TabIndex = 1;

            lblCsvOutputHeader.Dock = DockStyle.Top;
            lblCsvOutputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCsvOutputHeader.Location = new Point(0, 0);
            lblCsvOutputHeader.Name = "lblCsvOutputHeader";
            lblCsvOutputHeader.Padding = new Padding(5, 5, 0, 0);
            lblCsvOutputHeader.Size = new Size(431, 30);
            lblCsvOutputHeader.Text = "Ausgabe";

            // ----------------------------------------------------------------
            // statusStrip
            // ----------------------------------------------------------------
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 511);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(884, 26);
            statusStrip.TabIndex = 1;

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
            MinimumSize = new Size(700, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CAMT/CSV zu MT940 Konverter";

            tabControl.ResumeLayout(false);
            tabCamt.ResumeLayout(false);
            tabCsv.ResumeLayout(false);
            groupBoxInput.ResumeLayout(false);
            groupBoxInput.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            groupBoxCsvInput.ResumeLayout(false);
            groupBoxCsvInput.PerformLayout();
            splitContainerCsv.Panel1.ResumeLayout(false);
            splitContainerCsv.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerCsv).EndInit();
            splitContainerCsv.ResumeLayout(false);
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
        private Label lblHint;
        private Button btnConvert;
        private Button btnClear;
        private SplitContainer splitContainer;
        private ListBox lstFiles;
        private Label lblInputHeader;
        private ListBox lstOutput;
        private Label lblOutputHeader;

        // CSV
        private GroupBox groupBoxCsvInput;
        private Label lblTemplate;
        private ComboBox cmbTemplates;
        private Label lblCsvHint;
        private Button btnCsvConvert;
        private Button btnCsvClear;
        private Button btnCsvSelectFolder;
        private SplitContainer splitContainerCsv;
        private ListBox lstCsvFiles;
        private Label lblCsvInputHeader;
        private ListBox lstCsvOutput;
        private Label lblCsvOutputHeader;

        // Status
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}