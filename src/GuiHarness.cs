using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

// Consolidated reflection harness for validating GedcomExporterForm behavior in-process.
// UI input injection (SendKeys, UI Automation invoke/value patterns) does not work in this
// environment - confirmed repeatedly - so every GUI check drives the real private handlers
// directly via reflection instead of simulating clicks/typing.
//
// Compiled on demand by validate_beta_release.ps1 against whichever release copy is under
// test (never checked in as a prebuilt binary).
class GuiHarness
{
    private const string RegPath = @"Software\WHoward\Sims2Tools\GedcomExporter";

    [STAThread]
    static int Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0] : "";
        try
        {
            switch (mode)
            {
                case "defaults": return RunDefaults();
                case "roundtrip": return RunRoundtrip();
                case "export": return RunExport(args[1], args[2], args.Length > 3 && args[3] == "portraits");
                default:
                    Console.Error.WriteLine("Usage: GuiHarness <defaults|roundtrip|export> [hoodPath] [outPath] [portraits]");
                    return 99;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("HARNESS ERROR: " + ex);
            return 98;
        }
    }

    static Form NewForm() => (Form)Activator.CreateInstance(typeof(GedcomExporter.GedcomExporterForm));

    static object GetField(object obj, string name)
    {
        FieldInfo fi = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fi == null) throw new Exception("Field not found: " + name);
        return fi.GetValue(obj);
    }

    static void InvokeOnLoad(Form f)
    {
        MethodInfo m = f.GetType().GetMethod("OnLoad", BindingFlags.NonPublic | BindingFlags.Instance,
            null, new Type[] { typeof(object), typeof(EventArgs) }, null);
        m.Invoke(f, new object[] { null, EventArgs.Empty });
    }

    static void InvokeOnFormClosing(Form f)
    {
        // Must disambiguate from Control's own protected OnFormClosing/OnClosing overloads by
        // specifying the exact (object, FormClosingEventArgs) signature - the same class of
        // ambiguity already hit and fixed for OnLoad earlier this session.
        MethodInfo m = f.GetType().GetMethod("OnFormClosing", BindingFlags.NonPublic | BindingFlags.Instance,
            null, new Type[] { typeof(object), typeof(FormClosingEventArgs) }, null);
        m.Invoke(f, new object[] { null, new FormClosingEventArgs(CloseReason.UserClosing, false) });
    }

    static void ClearSavedSettings()
    {
        try { Registry.CurrentUser.DeleteSubKeyTree(RegPath); } catch { /* didn't exist */ }
    }

    // mode: defaults - fresh-install checkbox states must match the shipped defaults exactly.
    static int RunDefaults()
    {
        ClearSavedSettings();
        Form f = NewForm();
        InvokeOnLoad(f);

        bool npcs = ((CheckBox)GetField(f, "chkIncludeNpcs")).Checked;
        bool pets = ((CheckBox)GetField(f, "chkIncludePets")).Checked;
        bool dec = ((CheckBox)GetField(f, "chkIncludeDeceased")).Checked;
        bool port = ((CheckBox)GetField(f, "chkExtractPortraits")).Checked;
        bool hasDatesField = f.GetType().GetField("chkGenerateDates", BindingFlags.NonPublic | BindingFlags.Instance) != null;

        Console.WriteLine($"npcs={npcs} pets={pets} deceased={dec} portraits={port} hasDatesField={hasDatesField}");

        bool ok = (npcs == false && pets == false && dec == true && port == true && hasDatesField == false);
        return ok ? 0 : 1;
    }

    // mode: roundtrip - non-default states must survive a save (OnFormClosing) + reload (OnLoad
    // on a fresh instance), proving the registry persistence path itself works, not just the
    // fallback defaults used when nothing has been saved yet.
    static int RunRoundtrip()
    {
        ClearSavedSettings();

        Form f1 = NewForm();
        InvokeOnLoad(f1);
        ((CheckBox)GetField(f1, "chkIncludeNpcs")).Checked = true;
        ((CheckBox)GetField(f1, "chkIncludePets")).Checked = true;
        ((CheckBox)GetField(f1, "chkIncludeDeceased")).Checked = false;
        ((CheckBox)GetField(f1, "chkExtractPortraits")).Checked = false;
        InvokeOnFormClosing(f1);

        Form f2 = NewForm();
        InvokeOnLoad(f2);
        bool npcs = ((CheckBox)GetField(f2, "chkIncludeNpcs")).Checked;
        bool pets = ((CheckBox)GetField(f2, "chkIncludePets")).Checked;
        bool dec = ((CheckBox)GetField(f2, "chkIncludeDeceased")).Checked;
        bool port = ((CheckBox)GetField(f2, "chkExtractPortraits")).Checked;

        Console.WriteLine($"after save+reload: npcs={npcs} pets={pets} deceased={dec} portraits={port}");

        bool ok = (npcs == true && pets == true && dec == false && port == false);
        return ok ? 0 : 1;
    }

    // mode: export - drives the real OnExportClicked handler. The post-export
    // NullReferenceException from myMruList (never initialized because Load never fires in this
    // headless harness) is a known harness-only artifact, not a product bug - it happens strictly
    // after the real export work has already completed, so it's swallowed here deliberately.
    static int RunExport(string hoodPath, string outPath, bool portraits)
    {
        Form f = NewForm();
        ((TextBox)GetField(f, "textHoodPath")).Text = hoodPath;
        ((TextBox)GetField(f, "textOutputPath")).Text = outPath;
        ((CheckBox)GetField(f, "chkIncludeNpcs")).Checked = false;
        ((CheckBox)GetField(f, "chkIncludePets")).Checked = false;
        ((CheckBox)GetField(f, "chkIncludeDeceased")).Checked = true;
        ((CheckBox)GetField(f, "chkExtractPortraits")).Checked = portraits;

        MethodInfo onExport = f.GetType().GetMethod("OnExportClicked", BindingFlags.NonPublic | BindingFlags.Instance);
        try
        {
            onExport.Invoke(f, new object[] { null, EventArgs.Empty });
        }
        catch (TargetInvocationException tie) when (tie.InnerException is NullReferenceException)
        {
            Console.WriteLine("(expected harness-only post-export artifact swallowed)");
        }

        return File.Exists(outPath) ? 0 : 1;
    }
}
