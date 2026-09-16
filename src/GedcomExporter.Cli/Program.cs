/*
 * Gedcom Exporter CLI - headless test harness for GedcomExporter.Core
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.GedcomExporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GedcomExporter.Cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            List<string> positional = args.Where(a => !a.StartsWith("--")).ToList();
            HashSet<string> flags = new HashSet<string>(args.Where(a => a.StartsWith("--")), StringComparer.OrdinalIgnoreCase);

            if (positional.Count < 1)
            {
                Console.WriteLine("Usage: GedcomExporter.Cli <hoodFolder> [out.ged] [--pets] [--npcs] [--no-deceased] [--portraits]");
                Console.WriteLine("  If out.ged is omitted, it defaults to <hoodFolder>\\..\\<hoodCode>_GedcomExport\\<hoodCode>.ged");
                Console.WriteLine("  --portraits extracts in-game portraits to a Portraits\\ folder next to wherever out.ged ends up");
                return 1;
            }

            string hoodDir = positional[0];
            string hoodCode = ExportPaths.ComputeHoodCode(hoodDir);
            string exportDir = ExportPaths.ComputeExportDir(hoodDir, hoodCode);
            string outPath = positional.Count >= 2 ? positional[1] : Path.Combine(exportDir, hoodCode + ".ged");
            string outDir = Path.GetDirectoryName(Path.GetFullPath(outPath));
            // Portraits always sit next to the .ged that was actually written, wherever that ended
            // up - not independently hood-anchored - so the two outputs of one export never scatter
            // across different folders even when out.ged is overridden.
            string portraitDir = Path.Combine(outDir, "Portraits");
            bool extractPortraits = flags.Contains("--portraits");

            if (ExportPaths.IsInsideHood(outDir, hoodDir))
            {
                Console.Error.WriteLine("Refusing to write output under the neighborhood folder.");
                return 4;
            }

            ExportOptions options = new ExportOptions
            {
                IncludePets = flags.Contains("--pets"),
                IncludeNpcs = flags.Contains("--npcs"),
                IncludeDeceased = !flags.Contains("--no-deceased"),
            };

            ExportReport report = new ExportReport();

            // Safety check: hash every package before and after, and refuse to finish silently
            // if anything under the hood folder changed. The loader must never write back.
            Dictionary<string, string> before = HashPackages(hoodDir);

            try
            {
                Directory.CreateDirectory(outDir);

                HoodLoader loader = new HoodLoader(report);
                loader.Load(hoodDir);

                FamilyBuilder familyBuilder = new FamilyBuilder();
                List<Family> families = familyBuilder.Build(loader.Sims, loader.Ties, report);

                new GedcomWriter().Write(outPath, loader.Sims, families, options, report);

                if (extractPortraits)
                {
                    new PortraitExtractor().ExtractAll(hoodDir, portraitDir, report);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Export failed: {ex}");
                return 2;
            }

            Dictionary<string, string> after = HashPackages(hoodDir);
            if (!SameHashes(before, after))
            {
                Console.Error.WriteLine("SAFETY VIOLATION: package contents under the hood folder changed during export!");
                return 3;
            }

            Console.WriteLine($"Output: {outPath}");
            if (extractPortraits) Console.WriteLine($"Portraits: {portraitDir}");
            Console.WriteLine(report.ToString());
            foreach (string warning in report.Warnings) Console.WriteLine($"  WARN: {warning}");
            foreach (string dangling in report.DanglingTies) Console.WriteLine($"  DANGLING: {dangling}");
            foreach (string repaired in report.RepairedTies) Console.WriteLine($"  REPAIRED: {repaired}");
            foreach (string unnamed in report.UnnamedSims) Console.WriteLine($"  UNNAMED: {unnamed}");

            return 0;
        }

        private static Dictionary<string, string> HashPackages(string hoodDir)
        {
            var hashes = new Dictionary<string, string>();
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                foreach (string file in Directory.EnumerateFiles(hoodDir, "*.package", SearchOption.AllDirectories))
                {
                    using (FileStream stream = File.OpenRead(file))
                    {
                        hashes[file] = BitConverter.ToString(sha.ComputeHash(stream));
                    }
                }
            }
            return hashes;
        }

        private static bool SameHashes(Dictionary<string, string> a, Dictionary<string, string> b)
        {
            if (a.Count != b.Count) return false;
            foreach (KeyValuePair<string, string> kv in a)
            {
                if (!b.TryGetValue(kv.Key, out string otherHash) || otherHash != kv.Value) return false;
            }
            return true;
        }
    }
}
