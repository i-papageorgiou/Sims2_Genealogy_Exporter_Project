# Dev tools

`GuiHarness.cs` is a standalone reflection-based test harness used to drive
`GedcomExporterForm`'s private handlers (`OnLoad`, `OnFormClosing`,
`OnExportClicked`) directly during manual validation of a release build,
since UI input injection isn't reliable in this environment. It is compiled
on demand against whichever release copy is under test and is **not**
referenced by any project in `Sims2GenealogyExporter.sln`, not part of the
shipped build, and not included in source submissions of the product.

Usage: `GuiHarness <defaults|roundtrip|export> [hoodPath] [outPath] [portraits]`
