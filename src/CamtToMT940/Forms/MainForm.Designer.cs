using System.Windows.Forms;

namespace CamtToMT940
{
    partial class MainForm
    {
        ///  Required designer variable.
        private System.ComponentModel.IContainer components = null;

        ///  Clean up any resources being used.

        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
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
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            groupBoxInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxInput
            // 
            groupBoxInput.Controls.Add(lblHint);
            groupBoxInput.Controls.Add(btnConvert);
            groupBoxInput.Controls.Add(btnClear);
            groupBoxInput.Controls.Add(btnSelectFolder);
            groupBoxInput.Dock = DockStyle.Top;
            groupBoxInput.Location = new Point(0, 0);
            groupBoxInput.Name = "groupBoxInput";
            groupBoxInput.Size = new Size(884, 100);
            groupBoxInput.TabIndex = 0;
            groupBoxInput.TabStop = false;
            groupBoxInput.Text = "Eingabe";
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(15, 70);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(468, 20);
            lblHint.TabIndex = 3;
            lblHint.Text = "Dateien oder Ordner hierher ziehen (oder Pfade einfügen mit Strg+V)";
            // 
            // btnConvert
            // 
            btnConvert.Enabled = false;
            btnConvert.Location = new Point(315, 30);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new Size(150, 30);
            btnConvert.TabIndex = 2;
            btnConvert.Text = "Konvertieren";
            btnConvert.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(180, 30);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(120, 30);
            btnClear.TabIndex = 1;
            btnClear.Text = "Liste leeren";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(15, 30);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(150, 30);
            btnSelectFolder.TabIndex = 0;
            btnSelectFolder.Text = "Ordner auswählen...";
            btnSelectFolder.UseVisualStyleBackColor = true;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 100);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(lstFiles);
            splitContainer.Panel1.Controls.Add(lblInputHeader);
            splitContainer.Panel1MinSize = 200;
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(lstOutput);
            splitContainer.Panel2.Controls.Add(lblOutputHeader);
            splitContainer.Panel2MinSize = 200;
            splitContainer.Size = new Size(884, 411);
            splitContainer.SplitterDistance = 439;
            splitContainer.SplitterWidth = 6;
            splitContainer.TabIndex = 1;
            // 
            // lstFiles
            // 
            lstFiles.AllowDrop = true;
            lstFiles.Dock = DockStyle.Fill;
            lstFiles.FormattingEnabled = true;
            lstFiles.IntegralHeight = false;
            lstFiles.Location = new Point(0, 30);
            lstFiles.Name = "lstFiles";
            lstFiles.SelectionMode = SelectionMode.MultiExtended;
            lstFiles.Size = new Size(439, 381);
            lstFiles.TabIndex = 1;
            // 
            // lblInputHeader
            // 
            lblInputHeader.Dock = DockStyle.Top;
            lblInputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblInputHeader.Location = new Point(0, 0);
            lblInputHeader.Name = "lblInputHeader";
            lblInputHeader.Padding = new Padding(5, 5, 0, 0);
            lblInputHeader.Size = new Size(439, 30);
            lblInputHeader.TabIndex = 0;
            lblInputHeader.Text = "Eingabe (CAMT)";
            // 
            // lstOutput
            // 
            lstOutput.AllowDrop = true;
            lstOutput.Dock = DockStyle.Fill;
            lstOutput.FormattingEnabled = true;
            lstOutput.IntegralHeight = false;
            lstOutput.Location = new Point(0, 30);
            lstOutput.Name = "lstOutput";
            lstOutput.SelectionMode = SelectionMode.MultiExtended;
            lstOutput.Size = new Size(439, 381);
            lstOutput.TabIndex = 1;
            // 
            // lblOutputHeader
            // 
            lblOutputHeader.Dock = DockStyle.Top;
            lblOutputHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblOutputHeader.Location = new Point(0, 0);
            lblOutputHeader.Name = "lblOutputHeader";
            lblOutputHeader.Padding = new Padding(5, 5, 0, 0);
            lblOutputHeader.Size = new Size(439, 30);
            lblOutputHeader.TabIndex = 0;
            lblOutputHeader.Text = "Ausgabe (MT940)";
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 511);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(884, 26);
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(48, 20);
            statusLabel.Text = "Bereit";
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 537);
            Controls.Add(splitContainer);
            Controls.Add(groupBoxInput);
            Controls.Add(statusStrip);
            MinimumSize = new Size(700, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CAMT/CSV zu MT940 Konverter";
            groupBoxInput.ResumeLayout(false);
            groupBoxInput.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

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
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}
