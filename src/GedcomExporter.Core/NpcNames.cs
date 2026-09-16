/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>
    /// GUID -> name lookup for Sims that have no Characters\*.package - Maxis NPCs
    /// (Pollination Technician, Grim Reaper, ...) and vacation-hood visitors, whose SDSC
    /// records exist but who were never given a character file. Table copied from
    /// Hood Exporter's Resources\XML\npcs.xml.
    /// </summary>
    public class NpcNames
    {
        private readonly Dictionary<TypeGUID, string> namesByGuid = new Dictionary<TypeGUID, string>();

        public static NpcNames LoadDefault()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "npcs.xml");
            return File.Exists(path) ? Load(path) : new NpcNames();
        }

        public static NpcNames Load(string xmlPath)
        {
            NpcNames result = new NpcNames();

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            foreach (XmlNode npc in doc.SelectNodes("/npcs/npc"))
            {
                string valueText = npc.SelectSingleNode("value")?.InnerText?.Trim();
                string name = npc.SelectSingleNode("name")?.InnerText?.Trim();

                if (string.IsNullOrEmpty(valueText) || string.IsNullOrEmpty(name)) continue;

                uint value = uint.Parse(valueText.Replace("0x", "").Replace("0X", ""), NumberStyles.HexNumber);
                result.namesByGuid[(TypeGUID)value] = name;
            }

            return result;
        }

        public bool TryGetName(TypeGUID guid, out string name) => namesByGuid.TryGetValue(guid, out name);
    }
}
