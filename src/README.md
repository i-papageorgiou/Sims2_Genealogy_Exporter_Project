# Sims 2 Genealogy Exporter — Source Code

This is the full C# source for the three projects that make up the Sims 2
Genealogy Exporter:

- **GedcomExporter.Core** — DBPF/hood loading, tie-graph construction, and
  GEDCOM writing (no UI).
- **GedcomExporter** — the WinForms GUI (`GedcomExporter.exe`).
- **GedcomExporter.Cli** — the headless command-line front end
  (`GedcomExporter.Cli.exe`).

A standalone reflection-based test harness used to drive the GUI's private
handlers during validation lives outside this folder, in `../tools/`; it's
not part of the shipped build and isn't one of the projects above.

## Dependency: Sims2Tools

`DbpfLibrary/`, `UtilsLibrary/`, and `UtilsGraphicsLibrary/` in this folder
are William Howard's (whoward69) **[Sims2Tools](https://github.com/whoward69/Sims2Tools)**
libraries, included here **unmodified** under his published Code Reuse Policy
(reuse permitted, credit appreciated though not required; not for resale).
`DbpfLibrary` is what actually reads The Sims 2's `.package` (DBPF) file
format; `UtilsLibrary` (which itself depends on `UtilsGraphicsLibrary`)
provides shared helpers used by the GUI. Everything else in this folder
(`GedcomExporter`, `GedcomExporter.Core`, `GedcomExporter.Cli`)
is this project's own code.

To build:

1. Open `Sims2GenealogyExporter.sln` in Visual Studio 2019+ with the .NET
   Framework 4.8 targeting pack installed.
2. Restore NuGet packages (`log4net`, `Microsoft-WindowsAPICodePack-Core`,
   `Microsoft-WindowsAPICodePack-Shell`).
3. Build. Set `GedcomExporter` as the startup project to run the GUI.

The compiled release is distributed separately as the ready-to-run download.

## License

Permission granted to use this program in any way, except to claim it as
your own or sell it — the same terms carried in every source file here,
including the vendored `DbpfLibrary/`, `UtilsLibrary/`, and
`UtilsGraphicsLibrary/` folders from William Howard's Sims2Tools.
