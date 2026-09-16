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
using System.Collections.Generic;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// A readable log of everything the export skipped, repaired, or was unsure about.
    /// Never used to abort a run - one bad record must never take down the whole export.
    /// </summary>
    public class ExportReport
    {
        public List<string> Warnings { get; } = new List<string>();
        public List<string> DanglingTies { get; } = new List<string>();
        public List<string> RepairedTies { get; } = new List<string>();
        public List<string> UnnamedSims { get; } = new List<string>();

        public int SimsRead { get; set; }
        public int SimsWritten { get; set; }
        public int TiesRead { get; set; }
        public int FamiliesWritten { get; set; }
        public int PortraitsExtracted { get; set; }

        public void Warn(string message) => Warnings.Add(message);

        public void DanglingTie(string packageName, ushort fromLocalId, FamilyTieTypes type, ushort toLocalId) =>
            DanglingTies.Add($"[{packageName}] Sim 0x{fromLocalId:X4} --{type}--> 0x{toLocalId:X4}: target has no SDSC record, skipped");

        public void TieRepaired(TypeGUID from, FamilyTieTypes type, TypeGUID to, string what) =>
            RepairedTies.Add($"{from} --{type}--> {to}: {what}");

        public void UnnamedSim(TypeGUID guid) =>
            UnnamedSims.Add($"{guid}: no character file and no NPC table entry - named \"Unknown Sim {guid}\"");

        public override string ToString()
        {
            string s = $"Sims read: {SimsRead}, written: {SimsWritten}\n" +
                       $"Ties read: {TiesRead}, repaired: {RepairedTies.Count}, dangling/skipped: {DanglingTies.Count}\n" +
                       $"Families written: {FamiliesWritten}\n" +
                       $"Unnamed Sims: {UnnamedSims.Count}\n" +
                       $"Warnings: {Warnings.Count}";

            if (PortraitsExtracted > 0) s += $"\nPortraits extracted: {PortraitsExtracted}";

            return s;
        }
    }
}
