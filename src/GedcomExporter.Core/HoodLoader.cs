/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF;
using Sims2Tools.DBPF.CTSS;
using Sims2Tools.DBPF.Data;
using Sims2Tools.DBPF.Neighbourhood;
using Sims2Tools.DBPF.Neighbourhood.FAMT;
using Sims2Tools.DBPF.Neighbourhood.SDSC;
using Sims2Tools.DBPF.OBJD;
using Sims2Tools.DBPF.Package;
using Sims2Tools.DBPF.STR;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// Reads a Sims 2 neighborhood folder (main hood + any Suburb/University/Downtown/Vacation
    /// sub-hoods) into a GUID-keyed set of Sims and a merged tie graph.
    ///
    /// Every package is opened read-only via DBPFFile and never committed/updated/saved -
    /// this tool must never write back to a hood.
    ///
    /// The single fact that drives this loader: Sim IDs are package-local. The same numeric ID
    /// means a different Sim in the main hood than in a sub-hood (measured: 100% collision rate,
    /// 0% same identity, across every sub-hood checked). GUID is the only identity that means the
    /// same thing everywhere, so every tie is resolved through its *own* package's ID->GUID map
    /// before it is added to the global graph.
    /// </summary>
    public class HoodLoader
    {
        private readonly ExportReport report;
        private readonly NpcNames npcNames;

        public HoodLoader(ExportReport report, NpcNames npcNames = null)
        {
            this.report = report;
            this.npcNames = npcNames ?? NpcNames.LoadDefault();
        }

        public Dictionary<TypeGUID, SimRecord> Sims { get; } = new Dictionary<TypeGUID, SimRecord>();
        public TieGraph Ties { get; } = new TieGraph();

        public string HoodCode { get; private set; }
        public string HoodName { get; private set; }

        /// <summary>
        /// Loads everything under hoodDir. Throws only for conditions that make export
        /// meaningless (no main hood package found); anything else is logged and skipped.
        /// </summary>
        public void Load(string hoodDir)
        {
            DirectoryInfo hoodDirInfo = new DirectoryInfo(hoodDir);

            FileInfo[] mainFiles = hoodDirInfo.GetFiles("*_Neighborhood.package", SearchOption.TopDirectoryOnly);
            if (mainFiles.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one *_Neighborhood.package in {hoodDir}, found {mainFiles.Length}");
            }

            string mainPackagePath = mainFiles[0].FullName;
            HoodCode = mainFiles[0].Name.Substring(0, mainFiles[0].Name.IndexOf('_'));
            HoodName = HoodCode;

            // Character files carry given/family name and are keyed by Sim GUID - load first so
            // every package's SDSC pass can resolve names immediately.
            Dictionary<TypeGUID, (string given, string family)> namesByGuid = LoadCharacterNames(hoodDir);

            LoadPackage(mainPackagePath, HoodCode, namesByGuid);

            foreach (string pattern in new[] { "Suburb", "University", "Downtown", "Vacation" })
            {
                foreach (FileInfo subFile in hoodDirInfo.GetFiles($"{HoodCode}_{pattern}*.package", SearchOption.TopDirectoryOnly))
                {
                    LoadPackage(subFile.FullName, HoodCode, namesByGuid);
                }
            }

            Ties.RepairAsymmetry(report);
        }

        private Dictionary<TypeGUID, (string given, string family)> LoadCharacterNames(string hoodDir)
        {
            var result = new Dictionary<TypeGUID, (string, string)>();

            DirectoryInfo charactersDir = new DirectoryInfo(Path.Combine(hoodDir, "Characters"));
            if (!charactersDir.Exists) return result;

            foreach (FileInfo characterFile in charactersDir.GetFiles("*.package"))
            {
                try
                {
                    using (DBPFFile package = new DBPFFile(characterFile.FullName))
                    {
                        List<DBPFEntry> objds = package.GetEntriesByType(Objd.TYPE);
                        List<DBPFEntry> ctsss = package.GetEntriesByType(Ctss.TYPE);
                        if (objds.Count != 1 || ctsss.Count != 1) continue;

                        Objd objd = (Objd)package.GetResourceByEntry(objds[0]);
                        Ctss ctss = (Ctss)package.GetResourceByEntry(ctsss[0]);

                        List<StrItem> strs = ctss.LanguageItems(MetaData.Languages.Default);
                        // strs[0] = given name, strs[1] = biography, strs[2] = family name
                        if (strs.Count >= 3)
                        {
                            result[objd.Guid] = (strs[0].Title, strs[2].Title);
                        }
                    }
                }
                catch (Exception ex)
                {
                    report.Warn($"Character file {characterFile.Name} could not be read: {ex.Message}");
                }
            }

            return result;
        }

        private void LoadPackage(string packagePath, string hoodCode, Dictionary<TypeGUID, (string given, string family)> namesByGuid)
        {
            string packageName = Path.GetFileName(packagePath);

            try
            {
                using (DBPFFile package = new DBPFFile(packagePath))
                {
                    // Local Sim ID -> GUID, valid only within this package.
                    Dictionary<ushort, TypeGUID> localIdToGuid = new Dictionary<ushort, TypeGUID>();

                    foreach (DBPFEntry entry in package.GetEntriesByType(Sdsc.TYPE))
                    {
                        Sdsc sdsc;
                        try
                        {
                            sdsc = (Sdsc)package.GetResourceByEntry(entry);
                        }
                        catch (Exception ex)
                        {
                            report.Warn($"[{packageName}] SDSC 0x{entry.InstanceID.AsUInt():X4} could not be read: {ex.Message}");
                            continue;
                        }

                        ushort localId = (ushort)entry.InstanceID.AsUInt();
                        localIdToGuid[localId] = sdsc.SimGuid;

                        report.SimsRead++;

                        if (Sims.ContainsKey(sdsc.SimGuid))
                        {
                            // Same Sim seen again via another sub-hood copy of their SDSC - keep the
                            // first (main-hood) record, since sub-hood copies are often stale snapshots.
                            continue;
                        }

                        SimRecord sim = BuildSimRecord(sdsc, hoodCode, namesByGuid);
                        Sims[sdsc.SimGuid] = sim;
                    }

                    foreach (DBPFEntry entry in package.GetEntriesByType(Famt.TYPE))
                    {
                        Famt famt;
                        try
                        {
                            famt = (Famt)package.GetResourceByEntry(entry);
                        }
                        catch (Exception ex)
                        {
                            report.Warn($"[{packageName}] FAMT could not be read: {ex.Message}");
                            continue;
                        }

                        foreach (FamilyTieSim simTie in famt.Sims)
                        {
                            if (!localIdToGuid.TryGetValue(simTie.Instance, out TypeGUID fromGuid))
                            {
                                // The tie-owning Sim itself has no SDSC in this package - dangling at the source.
                                continue;
                            }

                            foreach (FamilyTieItem tie in simTie.Ties)
                            {
                                report.TiesRead++;

                                if (!localIdToGuid.TryGetValue(tie.Instance, out TypeGUID toGuid))
                                {
                                    report.DanglingTie(packageName, simTie.Instance, tie.Type, tie.Instance);
                                    continue;
                                }

                                Ties.Add(new TieEdge(fromGuid, tie.Type, toGuid));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                report.Warn($"Package {packageName} could not be opened: {ex.Message}");
            }
        }

        private SimRecord BuildSimRecord(Sdsc sdsc, string hoodCode, Dictionary<TypeGUID, (string given, string family)> namesByGuid)
        {
            SimRecord sim = new SimRecord(sdsc.SimGuid)
            {
                Gender = sdsc.Gender,
                LifeStage = sdsc.LifeSection,
                IsPet = sdsc.IsPet,
                IsCat = sdsc.IsCat,
                IsDog = sdsc.IsDog,
                OriginHood = hoodCode,
                FamilyNumber = sdsc.GetRawData(SdscIndex.FamilyNumber),
                Zodiac = sdsc.GetRawData(SdscIndex.Zodiac),
                Aspiration = sdsc.GetRawData(SdscIndex.Aspiration),
                IsNpc = sdsc.GetRawData(SdscIndex.NPCType) != 0,
            };

            // Death detection: GhostFlags bit 0 (IsGhost) is the closest available signal - see
            // the plan's open question. FamilyNumber==0 is used only as a cross-check for the report.
            ushort ghostFlags = sdsc.GetRawData(SdscIndex.GhostFlags);
            bool isGhost = (ghostFlags & 0x0001) != 0;
            bool hasNoFamily = sim.FamilyNumber == 0;
            sim.IsDead = isGhost;
            sim.DeathIsUncertain = isGhost != hasNoFamily;

            if (namesByGuid.TryGetValue(sdsc.SimGuid, out (string given, string family) name))
            {
                sim.GivenName = name.given;
                sim.FamilyName = name.family;
            }
            else if (npcNames.TryGetName(sdsc.SimGuid, out string npcName))
            {
                sim.GivenName = npcName;
                sim.FamilyName = "";
                sim.IsNpc = true;
            }
            else
            {
                sim.GivenName = $"Unknown Sim {sdsc.SimGuid}";
                sim.FamilyName = "";
                sim.NameIsUnknown = true;
                report.UnnamedSim(sdsc.SimGuid);
            }

            return sim;
        }
    }
}
