/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using System;
using System.IO;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// Shared by both GedcomExporter.Cli and the WinForms GedcomExporter so the default output
    /// location (tied to the hood's own folder, not wherever a user last saved a file) is computed
    /// exactly one way regardless of which front end is driving the export.
    /// </summary>
    public static class ExportPaths
    {
        /// <summary>Text before the first '_' in the *_Neighborhood.package filename - the same
        /// rule HoodLoader.Load uses. Falls back to the folder's own name if that file can't be
        /// found, purely so a default export path can still be named sensibly; HoodLoader.Load
        /// raises the real, clearer error later if the hood folder turns out to be invalid.</summary>
        public static string ComputeHoodCode(string hoodDir)
        {
            try
            {
                FileInfo[] files = new DirectoryInfo(hoodDir).GetFiles("*_Neighborhood.package", SearchOption.TopDirectoryOnly);
                if (files.Length == 1)
                {
                    string name = files[0].Name;
                    return name.Substring(0, name.IndexOf('_'));
                }
            }
            catch (Exception)
            {
                // fall through to the folder-name fallback below
            }

            return new DirectoryInfo(hoodDir).Name;
        }

        /// <summary>Sibling of the hood folder, named after it - so an export always lands in the
        /// same place regardless of where a given run's .ged path happens to point.</summary>
        public static string ComputeExportDir(string hoodDir, string hoodCode)
        {
            DirectoryInfo hoodInfo = new DirectoryInfo(hoodDir);
            string parent = hoodInfo.Parent != null ? hoodInfo.Parent.FullName : hoodInfo.FullName;
            return Path.Combine(parent, hoodCode + "_GedcomExport");
        }

        /// <summary>The default .ged path when the user hasn't chosen one - hood-anchored. Note
        /// there is deliberately no equivalent DefaultPortraitDir: portraits always go next to
        /// wherever the .ged actually ends up (see each caller's own outputDir-derived portraitDir),
        /// so the two outputs of one export never scatter across different folders even if this
        /// default is overridden.</summary>
        public static string DefaultGedPath(string hoodDir)
        {
            string hoodCode = ComputeHoodCode(hoodDir);
            return Path.Combine(ComputeExportDir(hoodDir, hoodCode), hoodCode + ".ged");
        }

        /// <summary>True if candidateDir is the hood folder itself or nested inside it - callers
        /// must refuse to write there.</summary>
        public static bool IsInsideHood(string candidateDir, string hoodDir)
        {
            string candidateFull = new DirectoryInfo(candidateDir).FullName.TrimEnd(Path.DirectorySeparatorChar);
            string hoodFull = new DirectoryInfo(hoodDir).FullName.TrimEnd(Path.DirectorySeparatorChar);
            return candidateFull.Equals(hoodFull, StringComparison.OrdinalIgnoreCase) ||
                   candidateFull.StartsWith(hoodFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
    }
}
