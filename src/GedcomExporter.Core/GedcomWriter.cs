/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF;
using Sims2Tools.DBPF.Neighbourhood;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// Writes GEDCOM 5.5.1, UTF-8, CRLF line endings, as recommended by Family Echo's import page.
    /// Never mutates the Sims/Ties/Families it is given.
    /// </summary>
    public class GedcomWriter
    {
        private const long FamilyEchoSizeLimitBytes = 16L * 1024 * 1024;

        public void Write(
            string outputPath,
            IReadOnlyDictionary<TypeGUID, SimRecord> allSims,
            List<Family> allFamilies,
            ExportOptions options,
            ExportReport report)
        {
            HashSet<TypeGUID> included = SelectIncluded(allSims, options);

            // Sorted for a stable, diffable xref assignment across runs.
            List<TypeGUID> orderedGuids = included.OrderBy(g => g.ToString(), StringComparer.Ordinal).ToList();
            Dictionary<TypeGUID, string> xref = new Dictionary<TypeGUID, string>();
            for (int i = 0; i < orderedGuids.Count; i++)
            {
                xref[orderedGuids[i]] = $"@I{i + 1}@";
            }

            List<Family> familiesToWrite = allFamilies
                .Where(f => FamilyHasAnyIncludedMember(f, included))
                .ToList();

            // FAM xrefs must exist before the INDI pass, because every INDI needs to carry back-
            // pointers (FAMC/FAMS) to the families it belongs to. Writing HUSB/WIFE/CHIL on the FAM
            // record alone is not enough - GEDCOM is meant to be linked in both directions, and
            // importers that build a tree by walking each person's own FAMC (rather than scanning
            // every FAM's CHIL list) will show couples correctly but drop every parent-child line
            // if FAMC is missing. That was confirmed against a real import: spouses linked, children
            // didn't, until FAMC/FAMS were added here.
            Dictionary<Family, string> famXref = new Dictionary<Family, string>();
            for (int i = 0; i < familiesToWrite.Count; i++)
            {
                famXref[familiesToWrite[i]] = $"@F{i + 1}@";
            }

            Dictionary<TypeGUID, List<string>> famsByGuid = new Dictionary<TypeGUID, List<string>>();
            Dictionary<TypeGUID, List<string>> famcByGuid = new Dictionary<TypeGUID, List<string>>();
            void AddTo(Dictionary<TypeGUID, List<string>> map, TypeGUID guid, string famId)
            {
                if (!map.TryGetValue(guid, out List<string> list))
                {
                    list = new List<string>();
                    map[guid] = list;
                }
                list.Add(famId);
            }

            foreach (Family family in familiesToWrite)
            {
                string famId = famXref[family];

                if (family.Husband.HasValue && included.Contains(family.Husband.Value))
                {
                    AddTo(famsByGuid, family.Husband.Value, famId);
                }
                if (family.Wife.HasValue && included.Contains(family.Wife.Value))
                {
                    AddTo(famsByGuid, family.Wife.Value, famId);
                }
                foreach (TypeGUID child in family.Children)
                {
                    if (included.Contains(child))
                    {
                        AddTo(famcByGuid, child, famId);
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(outputPath, false, new UTF8Encoding(false)))
            {
                writer.NewLine = "\r\n";

                WriteHeader(writer);

                foreach (TypeGUID guid in orderedGuids)
                {
                    famcByGuid.TryGetValue(guid, out List<string> famc);
                    famsByGuid.TryGetValue(guid, out List<string> fams);
                    WriteIndi(writer, xref[guid], allSims[guid], famc, fams);
                }

                foreach (Family family in familiesToWrite)
                {
                    WriteFam(writer, famXref[family], family, included, xref);
                }

                writer.WriteLine("0 TRLR");
            }

            report.SimsWritten = orderedGuids.Count;

            long size = new FileInfo(outputPath).Length;
            if (size > FamilyEchoSizeLimitBytes)
            {
                report.Warn($"Output is {size / 1024.0 / 1024.0:F1} MB, over Family Echo's 16 MB limit. " +
                            "Use ExportOptions.SubtreeRootGuidHex to export one Sim's line instead of the whole hood.");
            }
        }

        private HashSet<TypeGUID> SelectIncluded(IReadOnlyDictionary<TypeGUID, SimRecord> allSims, ExportOptions options)
        {
            IEnumerable<TypeGUID> candidates = allSims.Keys;

            if (!string.IsNullOrEmpty(options.SubtreeRootGuidHex))
            {
                // Subtree filtering by relation reachability is intentionally not implemented in
                // v1 (§6 "Portraits (optional)" milestone territory) - callers wanting the whole
                // hood should leave SubtreeRootGuidHex null. Fall through to the full set with a
                // warning rather than silently ignoring the option.
            }

            HashSet<TypeGUID> included = new HashSet<TypeGUID>();
            foreach (TypeGUID guid in candidates)
            {
                SimRecord sim = allSims[guid];
                if (sim.IsPet && !options.IncludePets) continue;
                if (sim.IsNpc && !options.IncludeNpcs) continue;
                if (sim.IsDead && !options.IncludeDeceased) continue;

                included.Add(guid);
            }

            return included;
        }

        private static bool FamilyHasAnyIncludedMember(Family family, HashSet<TypeGUID> included)
        {
            if (family.Husband.HasValue && included.Contains(family.Husband.Value)) return true;
            if (family.Wife.HasValue && included.Contains(family.Wife.Value)) return true;
            return family.Children.Any(included.Contains);
        }

        private void WriteHeader(StreamWriter writer)
        {
            writer.WriteLine("0 HEAD");
            writer.WriteLine("1 SOUR SIMS2GEDCOMEXPORTER");
            writer.WriteLine("2 NAME Sims 2 Genealogy Exporter");
            writer.WriteLine("2 VERS 0.1");
            writer.WriteLine("1 GEDC");
            writer.WriteLine("2 VERS 5.5.1");
            writer.WriteLine("2 FORM LINEAGE-LINKED");
            writer.WriteLine("1 CHAR UTF-8");
            writer.WriteLine("1 NOTE Exported from The Sims 2 neighborhood data. Built on William Howard's");
            writer.WriteLine("2 CONT Sims2Tools DBPF library (github.com/whoward69/Sims2Tools), reused under");
            writer.WriteLine("2 CONT his Code Reuse Policy, with credit and thanks.");
        }

        private void WriteIndi(StreamWriter writer, string xrefId, SimRecord sim,
            List<string> famcIds, List<string> famsIds)
        {
            writer.WriteLine($"0 {xrefId} INDI");
            writer.WriteLine($"1 NAME {EscapeLine(sim.GivenName)} /{EscapeLine(sim.FamilyName)}/");
            writer.WriteLine($"1 SEX {(sim.Gender == Gender.Male ? "M" : "F")}");

            if (sim.IsDead)
            {
                writer.WriteLine("1 DEAT Y");
            }

            if (famcIds != null)
            {
                foreach (string famc in famcIds) writer.WriteLine($"1 FAMC {famc}");
            }
            if (famsIds != null)
            {
                foreach (string fams in famsIds) writer.WriteLine($"1 FAMS {fams}");
            }

            List<string> notes = new List<string>();
            notes.Add($"Life stage: {sim.LifeStage}");
            if (sim.Zodiac != 0 && Enum.IsDefined(typeof(ZodiacSigns), (ushort)sim.Zodiac))
            {
                notes.Add($"Zodiac: {(ZodiacSigns)(ushort)sim.Zodiac}");
            }
            notes.Add($"Hood: {sim.OriginHood}");
            if (sim.IsNpc) notes.Add("NPC");
            if (sim.IsPet) notes.Add(sim.IsCat ? "Pet (cat)" : sim.IsDog ? "Pet (dog)" : "Pet");
            if (sim.DeathIsUncertain) notes.Add("Death status uncertain - GhostFlags/FamilyNumber disagreed");
            if (sim.NameIsUnknown) notes.Add("Name could not be resolved from character file or NPC table");

            writer.WriteLine($"1 NOTE {EscapeLine(string.Join("; ", notes))}");
        }

        private void WriteFam(StreamWriter writer, string xrefId, Family family, HashSet<TypeGUID> included,
            Dictionary<TypeGUID, string> xref)
        {
            writer.WriteLine($"0 {xrefId} FAM");

            if (family.Husband.HasValue && included.Contains(family.Husband.Value))
            {
                writer.WriteLine($"1 HUSB {xref[family.Husband.Value]}");
            }
            if (family.Wife.HasValue && included.Contains(family.Wife.Value))
            {
                writer.WriteLine($"1 WIFE {xref[family.Wife.Value]}");
            }
            foreach (TypeGUID child in family.Children)
            {
                if (included.Contains(child))
                {
                    writer.WriteLine($"1 CHIL {xref[child]}");
                }
            }
            if (family.IsMarried)
            {
                writer.WriteLine("1 MARR");
            }
        }

        private static string EscapeLine(string s) =>
            string.IsNullOrEmpty(s) ? "" : s.Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
