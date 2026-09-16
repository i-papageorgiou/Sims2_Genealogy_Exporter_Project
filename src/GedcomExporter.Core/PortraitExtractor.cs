/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF.CTSS;
using Sims2Tools.DBPF.Data;
using Sims2Tools.DBPF.Images.IMG;
using Sims2Tools.DBPF.Images.JPG;
using Sims2Tools.DBPF.OBJD;
using Sims2Tools.DBPF.Package;
using Sims2Tools.DBPF.STR;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// Extracts in-game Sim portraits from Characters\*.package into a folder, standalone from
    /// the GEDCOM pipeline - no GEDCOM OBJE/FILE linking (Family Echo does not read those; see
    /// phase3.md). Every package is opened read-only via DBPFFile, exactly like HoodLoader.
    /// </summary>
    public class PortraitExtractor
    {
        // The portrait resource's InstanceID encodes life stage using the same bitmask code as
        // Sims2Tools.Helpers.AgeHelper.CpfAgeCode in UtilsLibrary - NOT the LifeSections enum used
        // elsewhere in this project. Duplicated here (rather than referencing UtilsLibrary) to keep
        // Core's dependency surface at just DbpfLibrary. See HoodExporterForm.cs:736-746 for the
        // original mapping this mirrors.
        private static readonly Dictionary<uint, string> LifeStageSuffixes = new Dictionary<uint, string>
        {
            { 0x20, "Baby" },
            { 0x01, "Toddler" },
            { 0x02, "Child" },
            { 0x04, "Teen" },
            { 0x40, "YoungAdult" },
            { 0x08, "Adult" },
            { 0x10, "Elder" },
        };

        /// <summary>
        /// Extracts every available life-stage portrait for every Sim with a character file.
        /// Returns the number of image files written. outputDir is created if it doesn't exist.
        /// </summary>
        public int ExtractAll(string hoodDir, string outputDir, ExportReport report)
        {
            DirectoryInfo charactersDir = new DirectoryInfo(Path.Combine(hoodDir, "Characters"));
            if (!charactersDir.Exists) return 0;

            Directory.CreateDirectory(outputDir);

            int totalExtracted = 0;

            foreach (FileInfo characterFile in charactersDir.GetFiles("*.package"))
            {
                try
                {
                    totalExtracted += ExtractFromCharacterFile(characterFile, outputDir, report);
                }
                catch (Exception ex)
                {
                    report.Warn($"Character file {characterFile.Name} could not be read for portraits: {ex.Message}");
                }
            }

            report.PortraitsExtracted = totalExtracted;
            return totalExtracted;
        }

        private int ExtractFromCharacterFile(FileInfo characterFile, string outputDir, ExportReport report)
        {
            using (DBPFFile package = new DBPFFile(characterFile.FullName))
            {
                List<DBPFEntry> objds = package.GetEntriesByType(Objd.TYPE);
                List<DBPFEntry> ctsss = package.GetEntriesByType(Ctss.TYPE);
                if (objds.Count != 1 || ctsss.Count != 1) return 0;

                Objd objd = (Objd)package.GetResourceByEntry(objds[0]);
                Ctss ctss = (Ctss)package.GetResourceByEntry(ctsss[0]);

                List<StrItem> strs = ctss.LanguageItems(MetaData.Languages.Default);
                string given = strs.Count > 0 ? strs[0].Title : "Unknown";
                string family = strs.Count > 2 ? strs[2].Title : "";

                List<DBPFEntry> imgs = new List<DBPFEntry>(package.GetEntriesByType(Img.TYPE));
                imgs.AddRange(package.GetEntriesByType(Jpg.TYPES[(int)Jpg.JpgTypeIndex.Normal]));

                int extracted = 0;

                foreach (DBPFEntry entry in imgs)
                {
                    uint lifeStageCode = entry.InstanceID.AsUInt();
                    if (!LifeStageSuffixes.TryGetValue(lifeStageCode, out string suffix)) continue;

                    System.Drawing.Image image;
                    try
                    {
                        Img img = (Img)package.GetResourceByEntry(entry);
                        image = img.Image;
                    }
                    catch (Exception ex)
                    {
                        report.Warn($"{characterFile.Name}: portrait resource could not be read: {ex.Message}");
                        continue;
                    }

                    if (image == null) continue;

                    string fileName = MakeSafeFileName($"{given}_{family}_{objd.Guid}_{suffix}") + ".png";
                    string path = Path.Combine(outputDir, fileName);

                    using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        image.Save(stream, ImageFormat.Png);
                    }

                    extracted++;
                }

                if (extracted == 0)
                {
                    report.Warn($"{characterFile.Name} has no usable portrait images");
                }

                return extracted;
            }
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
