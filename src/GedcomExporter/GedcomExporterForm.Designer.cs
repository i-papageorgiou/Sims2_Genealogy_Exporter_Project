namespace GedcomExporter
{
    partial class GedcomExporterForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRecentHoods = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAbout = new System.Windows.Forms.ToolStripMenuItem();

            this.panelTop = new System.Windows.Forms.Panel();
            this.tableInputs = new System.Windows.Forms.TableLayoutPanel();
            this.lblHoodPath = new System.Windows.Forms.Label();
            this.textHoodPath = new System.Windows.Forms.TextBox();
            this.btnBrowseHood = new System.Windows.Forms.Button();
            this.lblOutputPath = new System.Windows.Forms.Label();
            this.textOutputPath = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.panelOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.chkIncludeNpcs = new System.Windows.Forms.CheckBox();
            this.chkIncludePets = new System.Windows.Forms.CheckBox();
            this.chkIncludeDeceased = new System.Windows.Forms.CheckBox();
            this.chkExtractPortraits = new System.Windows.Forms.CheckBox();
            this.btnExport = new System.Windows.Forms.Button();

            this.textReport = new System.Windows.Forms.TextBox();

            this.menuStrip1.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.tableInputs.SuspendLayout();
            this.SuspendLayout();

            //
            // menuStrip1
            //
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuFile,
                this.menuHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(700, 24);
            this.menuStrip1.TabIndex = 0;

            //
            // menuFile
            //
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuRecentHoods,
                this.menuFileSep1,
                this.menuExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "&File";

            this.menuRecentHoods.Name = "menuRecentHoods";
            this.menuRecentHoods.Size = new System.Drawing.Size(180, 22);
            this.menuRecentHoods.Text = "Recent &Hoods";

            this.menuFileSep1.Name = "menuFileSep1";
            this.menuFileSep1.Size = new System.Drawing.Size(177, 6);

            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(180, 22);
            this.menuExit.Text = "E&xit";
            this.menuExit.Click += new System.EventHandler(this.OnExitClicked);

            //
            // menuHelp
            //
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuAbout});
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(44, 20);
            this.menuHelp.Text = "&Help";

            this.menuAbout.Name = "menuAbout";
            this.menuAbout.Size = new System.Drawing.Size(180, 22);
            this.menuAbout.Text = "&About...";
            this.menuAbout.Click += new System.EventHandler(this.OnAboutClicked);

            //
            // panelTop
            //
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 24);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10);
            this.panelTop.Size = new System.Drawing.Size(700, 140);
            this.panelTop.TabIndex = 1;
            this.panelTop.Controls.Add(this.tableInputs);

            //
            // tableInputs
            //
            this.tableInputs.ColumnCount = 3;
            this.tableInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableInputs.RowCount = 4;
            this.tableInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableInputs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableInputs.AutoSize = true;
            this.tableInputs.Location = new System.Drawing.Point(10, 10);
            this.tableInputs.Name = "tableInputs";
            this.tableInputs.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);

            //
            // lblHoodPath
            //
            this.lblHoodPath.AutoSize = true;
            this.lblHoodPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblHoodPath.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblHoodPath.Name = "lblHoodPath";
            this.lblHoodPath.Text = "Hood folder:";

            //
            // textHoodPath
            //
            this.textHoodPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textHoodPath.Name = "textHoodPath";
            this.textHoodPath.TextChanged += new System.EventHandler(this.OnPathsChanged);

            //
            // btnBrowseHood
            //
            this.btnBrowseHood.AutoSize = true;
            this.btnBrowseHood.Name = "btnBrowseHood";
            this.btnBrowseHood.Text = "Browse...";
            this.btnBrowseHood.Click += new System.EventHandler(this.OnBrowseHoodClicked);

            //
            // lblOutputPath
            //
            this.lblOutputPath.AutoSize = true;
            this.lblOutputPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOutputPath.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblOutputPath.Name = "lblOutputPath";
            this.lblOutputPath.Text = "Output .ged file:";

            //
            // textOutputPath
            //
            this.textOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textOutputPath.Name = "textOutputPath";
            this.textOutputPath.TextChanged += new System.EventHandler(this.OnPathsChanged);

            //
            // btnBrowseOutput
            //
            this.btnBrowseOutput.AutoSize = true;
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Text = "Browse...";
            this.btnBrowseOutput.Click += new System.EventHandler(this.OnBrowseOutputClicked);

            //
            // panelOptions
            //
            this.panelOptions.AutoSize = true;
            this.panelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOptions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelOptions.WrapContents = true;
            this.panelOptions.Name = "panelOptions";
            this.tableInputs.SetColumnSpan(this.panelOptions, 3);
            this.panelOptions.Controls.Add(this.chkIncludeNpcs);
            this.panelOptions.Controls.Add(this.chkIncludePets);
            this.panelOptions.Controls.Add(this.chkIncludeDeceased);
            this.panelOptions.Controls.Add(this.chkExtractPortraits);

            //
            // chkIncludeNpcs
            //
            this.chkIncludeNpcs.AutoSize = true;
            this.chkIncludeNpcs.Margin = new System.Windows.Forms.Padding(3, 6, 12, 3);
            this.chkIncludeNpcs.Name = "chkIncludeNpcs";
            this.chkIncludeNpcs.Text = "Include NPCs";

            //
            // chkIncludePets
            //
            this.chkIncludePets.AutoSize = true;
            this.chkIncludePets.Margin = new System.Windows.Forms.Padding(3, 6, 12, 3);
            this.chkIncludePets.Name = "chkIncludePets";
            this.chkIncludePets.Text = "Include pets";

            //
            // chkIncludeDeceased
            //
            this.chkIncludeDeceased.AutoSize = true;
            this.chkIncludeDeceased.Checked = true;
            this.chkIncludeDeceased.Margin = new System.Windows.Forms.Padding(3, 6, 12, 3);
            this.chkIncludeDeceased.Name = "chkIncludeDeceased";
            this.chkIncludeDeceased.Text = "Include deceased Sims";

            //
            // chkExtractPortraits
            //
            this.chkExtractPortraits.AutoSize = true;
            this.chkExtractPortraits.Checked = true;
            this.chkExtractPortraits.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.chkExtractPortraits.Name = "chkExtractPortraits";
            this.chkExtractPortraits.Text = "Extract portraits (saved next to the hood folder; Family Echo needs each assigned manually)";

            //
            // btnExport
            //
            this.btnExport.AutoSize = true;
            this.btnExport.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnExport.Enabled = false;
            this.btnExport.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.btnExport.Name = "btnExport";
            this.btnExport.Text = "Export";
            this.tableInputs.SetColumnSpan(this.btnExport, 3);
            this.btnExport.Click += new System.EventHandler(this.OnExportClicked);

            this.tableInputs.Controls.Add(this.lblHoodPath, 0, 0);
            this.tableInputs.Controls.Add(this.textHoodPath, 1, 0);
            this.tableInputs.Controls.Add(this.btnBrowseHood, 2, 0);
            this.tableInputs.Controls.Add(this.lblOutputPath, 0, 1);
            this.tableInputs.Controls.Add(this.textOutputPath, 1, 1);
            this.tableInputs.Controls.Add(this.btnBrowseOutput, 2, 1);
            this.tableInputs.Controls.Add(this.panelOptions, 0, 2);
            this.tableInputs.Controls.Add(this.btnExport, 0, 3);

            //
            // textReport
            //
            this.textReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textReport.Multiline = true;
            this.textReport.ReadOnly = true;
            this.textReport.WordWrap = false;
            this.textReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textReport.Font = new System.Drawing.Font("Consolas", 9F);
            this.textReport.Name = "textReport";
            this.textReport.TabStop = false;

            //
            // GedcomExporterForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Controls.Add(this.textReport);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "GedcomExporterForm";
            this.Text = "Gedcom Exporter";
            this.Load += new System.EventHandler(this.OnLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);

            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableInputs.ResumeLayout(false);
            this.tableInputs.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuRecentHoods;
        private System.Windows.Forms.ToolStripSeparator menuFileSep1;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuAbout;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TableLayoutPanel tableInputs;
        private System.Windows.Forms.Label lblHoodPath;
        private System.Windows.Forms.TextBox textHoodPath;
        private System.Windows.Forms.Button btnBrowseHood;
        private System.Windows.Forms.Label lblOutputPath;
        private System.Windows.Forms.TextBox textOutputPath;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.FlowLayoutPanel panelOptions;
        private System.Windows.Forms.CheckBox chkIncludeNpcs;
        private System.Windows.Forms.CheckBox chkIncludePets;
        private System.Windows.Forms.CheckBox chkIncludeDeceased;
        private System.Windows.Forms.CheckBox chkExtractPortraits;
        private System.Windows.Forms.Button btnExport;

        private System.Windows.Forms.TextBox textReport;
    }
}
