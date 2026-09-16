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

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// One Sim, identified by their hood-wide-unique GUID (never the package-local Sim ID,
    /// which collides between the main hood and every sub-hood).
    /// </summary>
    public class SimRecord
    {
        public TypeGUID Guid { get; }

        public string GivenName { get; set; } = "";
        public string FamilyName { get; set; } = "";

        public Gender Gender { get; set; }
        public LifeSections LifeStage { get; set; }

        public bool IsPet { get; set; }
        public bool IsCat { get; set; }
        public bool IsDog { get; set; }

        public bool IsNpc { get; set; }
        public bool IsDead { get; set; }
        public bool DeathIsUncertain { get; set; }

        public uint FamilyNumber { get; set; }
        public uint Zodiac { get; set; }
        public uint Aspiration { get; set; }

        /// <summary>Hood code (e.g. "N002") the Sim's SDSC record was read from - the main hood
        /// unless the Sim exists only in a sub-hood (townies, dormies, vacationers, etc).</summary>
        public string OriginHood { get; set; } = "";

        /// <summary>True if no character package or NPC table entry supplied a name.</summary>
        public bool NameIsUnknown { get; set; }

        public SimRecord(TypeGUID guid)
        {
            Guid = guid;
        }

        public string DisplayName =>
            string.IsNullOrEmpty(FamilyName) ? GivenName : $"{GivenName} {FamilyName}";

        public override string ToString() => $"{DisplayName} ({Guid})";
    }
}
