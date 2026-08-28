# Sims 2 Genealogy Exporter

Turns a **The Sims 2** neighborhood's save data into a **GEDCOM** (`.ged`) family tree file — the format read by genealogy sites and desktop programs. Point it at your `Neighborhoods\N00x` folder, click Export, get a family tree.

> **Status: v0.1 Beta.** The export logic has been run against 11 real neighborhoods (including a 970-Sim megahood) with zero data-safety issues, and one real user's Family Echo round-trip already found and fixed a bug (missing sibling/parent links). It has not yet been run on a machine other than the one it was built on — see [Reporting a problem](#reporting-a-problem) if something looks wrong.

## What it does

- Reads a neighborhood's `.package` files **read-only** — it never modifies your saves. The CLI even hashes every package before and after export and refuses to finish silently if anything changed.
- Rebuilds the family tree from the game's own parent / spouse / sibling tie data (the `FAMt` resource), the same source SimPE's Family Ties view reads.
- Exports a standard `.ged` file you can import into [Family Echo](https://www.familyecho.com/import.php) or [Gramps](https://gramps-project.org/) (a free desktop genealogy program).
- Optionally extracts every in-game Sim portrait it can find into a `Portraits\` folder alongside the export.
- Handles the messy real-world cases correctly:
  - NPC parents (a Pollination Technician birth shows up as a real parent, not a gap in the tree)
  - sibling-only ties with no recorded parents (emitted as a phantom family, the standard GEDCOM idiom)
  - dangling / one-sided tie data left over from deleted or corrupted Sims
  - pets, with their own tree and a note tag
  - megahoods with hundreds of Sims
  - sub-hood (University / Downtown / Vacation) contributions to the main family tree

There are no in-game calendar dates in The Sims 2, so this tool doesn't invent fictional birth/death dates — trees sort by relationship, not by date.

## Requirements

- Windows, .NET Framework 4.8 (already present on any reasonably current Windows 10/11 install; Windows Update will offer it otherwise).
- A Sims 2 neighborhood folder — `Documents\EA Games\The Sims 2\Neighborhoods\N00x`, or wherever your install keeps saves. It's the folder containing a file named like `N001_Neighborhood.package`.

## Quick start

1. Grab the latest release zip and unzip it anywhere.
2. Run **`GedcomExporter.exe`**.
3. Click **Browse...** next to "Hood folder" and select your neighborhood folder.
4. The output path fills in automatically, next to your neighborhood folder (see [Where output goes](#where-output-goes)). Leave it, or click its own **Browse...** to pick somewhere else.
5. Choose your filters (see below).
6. Click **Export**.
7. Read the report that appears when it's done — it lists anything unusual it found (broken ties, unnamed Sims, and so on) and confirms where everything was saved.
8. Import the `.ged` into [Family Echo](https://www.familyecho.com/import.php) or [Gramps](https://gramps-project.org/).

### Where output goes

By default, everything lands **next to your neighborhood folder**, never inside it:

```
Neighborhoods\
├── N002\                          <- your neighborhood (never modified)
└── N002_GedcomExport\
    ├── N002.ged                   <- the family tree file
    └── Portraits\                 <- in-game Sim photos, if you checked that box
```

This tool only *reads* `.package` files; everything it produces goes into a new, separate folder. Back up your saves before trying any new tool on them regardless — that's just good practice, not a comment on this one.

### The checkboxes

| Option | Default | What it does |
|---|---|---|
| **Include NPCs** | Off | Service Sims, the Pollination Technician, townies without a household, etc. |
| **Include pets** | Off | Cats and dogs get their own `INDI` entries, tagged as pets in their notes. |
| **Include deceased Sims** | On | Turn off to export only the living. |
| **Extract portraits** | On | Pulls every in-game portrait it can find per Sim into `Portraits\`. Noticeably slower than the export itself on a large hood — it's real image decoding, not instant. |

### About portraits

Family Echo's own support team has confirmed it does **not** auto-import photos from a GEDCOM file — you'd attach each Sim's photo manually inside Family Echo's editor after import, using the extracted `Portraits\` folder as a reference (files are named `Given_Family_GUID_LifeStage.png`, easy to match by eye). Gramps has no such limitation for its own photo workflow, though this tool doesn't currently wire up automatic GEDCOM photo links for it either — a possible future addition.

## Command-line usage

`GedcomExporter.Cli.exe` runs the same export headlessly — useful for scripting or batch-processing several hoods:

```
GedcomExporter.Cli.exe <hoodFolder> [out.ged] [--pets] [--npcs] [--no-deceased] [--portraits]
```

- `out.ged` is optional; if omitted, it defaults to `<hoodFolder>\..\<hoodCode>_GedcomExport\<hoodCode>.ged` — the same hood-anchored location the GUI uses.
- `--portraits` extracts in-game portraits to a `Portraits\` folder next to wherever `out.ged` ends up.
- The CLI refuses to write output under the neighborhood folder itself, and exits non-zero (with a `SAFETY VIOLATION` message) if any package under the hood folder changed during export.

## Building from source

The projects live under `whoward69/Source_Code/`, alongside a full checkout of the [Sims2Tools](https://github.com/whoward69/Sims2Tools) solution they depend on:

- `GedcomExporter.Core` — the DBPF/hood loading, tie-graph, and GEDCOM-writing logic (no UI).
- `GedcomExporter` — the WinForms GUI.
- `GedcomExporter.Cli` — the command-line front end shown above.

Open `whoward69/Source_Code/Sims2Tools.sln` in Visual Studio (2019+) with the .NET Framework 4.8 targeting pack installed, and build. All three `GedcomExporter*` projects reference `DbpfLibrary` and `UtilsLibrary` from the rest of the solution.

## Known limitations (why this is a beta)

- Tested against 11 real neighborhoods on the author's own machine, including a ~1000-Sim megahood, with no data-safety issues — but not yet tested on anyone else's machine or hood.
- No in-game calendar means no real birth/death dates.
- Adoption vs. biological parentage isn't distinguished, because the underlying game data doesn't reliably distinguish it either.

## Reporting a problem

If the exported `.ged` doesn't look right, or the program errors out, please open an issue with:

- which neighborhood folder you pointed it at (and roughly how big — number of Sims, sub-hoods present),
- which checkboxes were on,
- the exact text of the report panel (or error dialog) after the export finished or failed.

That's the exact information that already found and fixed two real bugs during testing.

## Credits

Built entirely on top of **William Howard's (whoward69) [Sims2Tools](https://github.com/whoward69/Sims2Tools)** — the DBPF file library that makes reading Sims 2 save data possible at all, plus the shared dialog/registry helpers this program's interface is built from. Reused under his published Code Reuse Policy (reuse permitted, credit appreciated though not required; not for resale). Thank you for building and sharing the tools that made this possible.

## License

Permission granted to use this program in any way, except to claim it as your own or sell it — the same terms carried in every source file this project wrote, in the same spirit as the Sims2Tools code it's built on.
