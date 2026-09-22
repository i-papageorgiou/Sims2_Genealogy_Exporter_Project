/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as a GEDCOM file
 *
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Microsoft.WindowsAPICodePack.Dialogs;
using Sims2Tools;
using Sims2Tools.GedcomExporter;
using Sims2Tools.Utils.Persistence;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GedcomExporter
{
    public partial class GedcomExporterForm : Form
    {
        private readonly CommonOpenFileDialog selectHoodDialog;
        private MruList myMruList;

        public GedcomExporterForm()
        {
            InitializeComponent();
            this.Text = GedcomExporterApp.AppTitle;

            selectHoodDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select the Sims 2 neighborhood folder (containing *_Neighborhood.package)",
            };
        }

        private void OnLoad(object sender, EventArgs e)
        {
            RegistryTools.LoadAppSettings(GedcomExporterApp.RegistryKey, GedcomExporterApp.AppVersionMajor, GedcomExporterApp.AppVersionMinor);
            RegistryTools.LoadFormSettings(GedcomExporterApp.RegistryKey, this);

            textHoodPath.Text = RegistryTools.GetSetting(GedcomExporterApp.RegistryKey, textHoodPath.Name, "") as string;
            textOutputPath.Text = RegistryTools.GetSetting(GedcomExporterApp.RegistryKey, textOutputPath.Name, "") as string;

            chkIncludeNpcs.Checked = ((int)RegistryTools.GetSetting(GedcomExporterApp.RegistryKey + @"\Options", chkIncludeNpcs.Name, 0) != 0);
            chkIncludePets.Checked = ((int)RegistryTools.GetSetting(GedcomExporterApp.RegistryKey + @"\Options", chkIncludePets.Name, 0) != 0);
            chkIncludeDeceased.Checked = ((int)RegistryTools.GetSetting(GedcomExporterApp.RegistryKey + @"\Options", chkIncludeDeceased.Name, 1) != 0);
            chkExtractPortraits.Checked = ((int)RegistryTools.GetSetting(GedcomExporterApp.RegistryKey + @"\Options", chkExtractPortraits.Name, 1) != 0);

            myMruList = new MruList(GedcomExporterApp.RegistryKey, menuRecentHoods, Properties.Settings.Default.MruSize, false, true);
            myMruList.FileSelected += OnRecentHoodSelected;

            UpdateForm();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (Form.ModifierKeys == (Keys.Control | Keys.Shift))
            {
                RegistryTools.RemoveAppSettings(GedcomExporterApp.RegistryKey);
                return;
            }

            RegistryTools.SaveAppSettings(GedcomExporterApp.RegistryKey, GedcomExporterApp.AppVersionMajor, GedcomExporterApp.AppVersionMinor);
            RegistryTools.SaveFormSettings(GedcomExporterApp.RegistryKey, this);

            RegistryTools.SaveSetting(GedcomExporterApp.RegistryKey, textHoodPath.Name, textHoodPath.Text);
            RegistryTools.SaveSetting(GedcomExporterApp.RegistryKey, textOutputPath.Name, textOutputPath.Text);

            RegistryTools.SaveSetting(GedcomExporterApp.RegistryKey + @"\Options", chkIncludeNpcs.Name, chkIncludeNpcs.Checked ? 1 : 0);
            RegistryTools.SaveSetting(GedcomExporterApp.RegistryKey + @"\Options", chkIncludePets.Name, chkIncludePets.Checked ? 1 : 0);
            RegistryTools.SaveSetting(GedcomExporterApp.RegistryKey + @"\Options", chkIncludeDeceased.Name, chkIncludeDeceased.Checked ? 1 : 0);
            RegistryTools.SaveSetting(GedcomExporterApp.RegistryKey + @"\Options", chkExtractPortraits.Name, chkExtractPortraits.Checked ? 1 : 0);
        }

        private void OnRecentHoodSelected(string folder)
        {
            textHoodPath.Text = folder;
            TryAutoFillOutputPath(folder);
        }

        private void OnBrowseHoodClicked(object sender, EventArgs e)
        {
            selectHoodDialog.InitialDirectory = textHoodPath.Text;

            if (selectHoodDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textHoodPath.Text = selectHoodDialog.FileName;
                TryAutoFillOutputPath(selectHoodDialog.FileName);
            }
        }

        /// <summary>Fills in the output path from the hood folder's own location (see
        /// ExportPaths.DefaultGedPath) the first time a hood is chosen, without ever overwriting
        /// a path the user already typed or browsed to themselves.</summary>
        private void TryAutoFillOutputPath(string hoodFolder)
        {
            if (textOutputPath.Text.Trim().Length > 0) return;

            try
            {
                textOutputPath.Text = ExportPaths.DefaultGedPath(hoodFolder);
            }
            catch (Exception)
            {
                // Leave the output field blank - not worth failing the whole picker over a
                // best-effort convenience default.
            }
        }

        private void OnBrowseOutputClicked(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "GEDCOM files (*.ged)|*.ged|All files (*.*)|*.*",
                DefaultExt = "ged",
                AddExtension = true,
                FileName = textOutputPath.Text,
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textOutputPath.Text = dialog.FileName;
                }
            }
        }

        private void OnPathsChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void UpdateForm()
        {
            btnExport.Enabled = (textHoodPath.Text.Trim().Length > 0 && textOutputPath.Text.Trim().Length > 0);
        }

        private void OnExportClicked(object sender, EventArgs e)
        {
            if (!Directory.Exists(textHoodPath.Text))
            {
                MessageBox.Show(this, $"Hood folder not found:\n{textHoodPath.Text}", GedcomExporterApp.AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string hoodPath = textHoodPath.Text;
            string outputPath = textOutputPath.Text;
            bool extractPortraits = chkExtractPortraits.Checked;
            string outputDir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            // Portraits always sit next to the .ged that was actually written, wherever that ended
            // up - not independently hood-anchored - so the two outputs of one export never scatter
            // across different folders even if the output path was overridden via Browse.
            string portraitDir = Path.Combine(outputDir, "Portraits");

            if (ExportPaths.IsInsideHood(outputDir, hoodPath))
            {
                MessageBox.Show(this, "Refusing to write output under the neighborhood folder.", GedcomExporterApp.AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ExportOptions options = new ExportOptions
            {
                IncludeNpcs = chkIncludeNpcs.Checked,
                IncludePets = chkIncludePets.Checked,
                IncludeDeceased = chkIncludeDeceased.Checked,
            };

            ProgressDialog progress = new ProgressDialog
            {
                DefaultStatusText = "Reading hood and writing GEDCOM...",
                VisualMode = Sims2Tools.Controls.ProgressBarDisplayMode.CustomText,
            };

            progress.DoWork += (dlg, args) =>
            {
                ExportReport report = new ExportReport();

                Directory.CreateDirectory(outputDir);

                HoodLoader loader = new HoodLoader(report);
                loader.Load(hoodPath);

                FamilyBuilder familyBuilder = new FamilyBuilder();
                var families = familyBuilder.Build(loader.Sims, loader.Ties, report);

                new GedcomWriter().Write(outputPath, loader.Sims, families, options, report);

                if (extractPortraits)
                {
                    new PortraitExtractor().ExtractAll(hoodPath, portraitDir, report);
                }

                args.Result = report;
            };

            progress.ShowDialog(this);

            if (progress.Result?.Error != null)
            {
                MessageBox.Show(this, $"Export failed:\n{progress.Result.Error.Message}", GedcomExporterApp.AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (progress.Result?.Cancelled == true)
            {
                return;
            }

            myMruList.AddFile(hoodPath);

            if (progress.Result?.Result is ExportReport finishedReport)
            {
                ShowReport(finishedReport, outputPath, extractPortraits ? portraitDir : null);
            }
        }

        private void ShowReport(ExportReport report, string outputPath, string portraitDir)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Output: {outputPath}");
            if (portraitDir != null) sb.AppendLine($"Portraits: {portraitDir}");
            sb.AppendLine(report.ToString());

            AppendSection(sb, "Warnings", report.Warnings);
            AppendSection(sb, "Dangling ties (skipped)", report.DanglingTies);
            AppendSection(sb, "Repaired ties", report.RepairedTies);
            AppendSection(sb, "Unnamed Sims", report.UnnamedSims);

            textReport.Text = sb.ToString();
        }

        private static void AppendSection(StringBuilder sb, string title, System.Collections.Generic.List<string> lines)
        {
            if (lines.Count == 0) return;

            sb.AppendLine();
            sb.AppendLine($"--- {title} ({lines.Count}) ---");
            foreach (string line in lines) sb.AppendLine(line);
        }

        private void OnAboutClicked(object sender, EventArgs e)
        {
            new AboutDialog(GedcomExporterApp.AppProduct).ShowDialog(this);
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
