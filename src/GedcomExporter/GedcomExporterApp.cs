/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as a GEDCOM file
 *
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using System;
using System.Windows.Forms;

namespace GedcomExporter
{
    internal static class GedcomExporterApp
    {
        public static readonly string AppName = "Gedcom Exporter";

        public static readonly int AppVersionMajor = 0;
        public static readonly int AppVersionMinor = 1;

        private static readonly string AppVersionType = "a"; // a - alpha, b - beta, r - release

        public static readonly string AppTitle = $"{AppName} V{AppVersionMajor}.{AppVersionMinor}{AppVersionType}";

        public static readonly string AppProduct = $"{AppName} Version {AppVersionMajor}.{AppVersionMinor}{AppVersionType}";

        public static readonly string RegistryKey = Sims2Tools.Sims2ToolsLib.RegistryKey + @"\GedcomExporter";

        [STAThread]
        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GedcomExporterForm());
        }
    }
}
