/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

namespace Sims2Tools.GedcomExporter
{
    public class ExportOptions
    {
        public bool IncludeNpcs { get; set; } = false;
        public bool IncludePets { get; set; } = false;
        public bool IncludeDeceased { get; set; } = true;

        /// <summary>Restrict output to one Sim's ancestors and descendants (subtree filter for
        /// megahoods that exceed Family Echo's 16 MB ceiling). Null = whole hood.</summary>
        public string SubtreeRootGuidHex { get; set; } = null;
    }
}
