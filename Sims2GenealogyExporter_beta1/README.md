# Sims 2 Genealogy Exporter (beta)

Turns a **The Sims 2** neighborhood's save data into a **GEDCOM** (`.ged`) family tree file — the format used by genealogy sites and desktop programs. Hood folder in, `.ged` out.

This is a **beta**: the export logic has been tested against 11 real neighborhoods (including a 970-Sim megahood) with zero safety issues, and a real user's Family Echo import round-trip already found and fixed one bug (missing sibling/parent links). It has not yet been run on a machine other than the one it was built on, so if something goes wrong, that's useful information — see "Reporting a problem" below.

## Quick start

1. Run **`GedcomExporter.exe`**.
2. Click **Browse...** next to "Hood folder" and select the folder containing your neighborhood — the one with a file named like `N001_Neighborhood.package` in it.
3. The output path fills in automatically, next to your neighborhood folder (see "Where output goes" below). Leave it, or click its own **Browse...** to pick somewhere else.
4. Choose your filters (see below).
5. Click **Export**.
6. A report appears when it's done — read it. It will tell you about anything unusual it found (broken ties, unnamed Sims, and so on) and where everything was saved.

## Where output goes

By default, everything lands **next to your neighborhood folder**, not wherever you last saved something:

```
Neighborhoods\
├── N002\                          <- your neighborhood (never modified)
└── N002_GedcomExport\
    ├── N002.ged                   <- the family tree file
    └── Portraits\                 <- in-game Sim photos, if you checked that box
```

This tool **never writes to your neighborhood folder**. It only reads `.package` files; everything it produces goes into a new, separate folder next to it. Back up your saves before trying any new tool on them anyway — that's just good practice, not a comment on this one.

## The checkboxes

- **Include NPCs** — service Sims, the Pollination Technician, townies without a household, etc. Off by default; turn it on if you want them in the tree too.
- **Include pets** — cats and dogs get their own `INDI` entries, tagged as pets in their notes.
- **Include deceased Sims** — leave this on unless you specifically want only the living.
- **Extract portraits** — pulls every in-game portrait image it can find for each Sim into a `Portraits\` folder next to the `.ged` (see below). On by default; note it's noticeably slower on a large hood than the export itself (real image decoding, not instant).

## Using the output

Import the `.ged` into [Family Echo](https://www.familyecho.com/import.php) or [Gramps](https://gramps-project.org/) (a free desktop genealogy program — the more forgiving option if a website rejects the file, since desktop tools give real parse errors instead of a silent failure).

**About portraits**: Family Echo's own support team has confirmed they do not auto-import photos from a GEDCOM file, full stop — you'd need to attach each Sim's photo to them manually inside Family Echo's own editor after import, using the extracted `Portraits\` folder as your reference (files are named `Given_Family_GUID_LifeStage.png`, so they're easy to match by eye). This tool does not attempt to link photos automatically in the `.ged` file itself, because Family Echo wouldn't read that link anyway.

## The CLI tool

`GedcomExporter.Cli.exe` does the same export from a command line — useful for scripting or batch-processing several hoods:

```
GedcomExporter.Cli.exe <hoodFolder> [out.ged] [--pets] [--npcs] [--no-deceased] [--portraits]
```

`out.ged` is optional; if you leave it out, it defaults to the same hood-anchored location the GUI uses.

## Credits

Built entirely on top of **William Howard's (whoward69) [Sims2Tools](https://github.com/whoward69/Sims2Tools)** — the DBPF file library that makes reading Sims 2 save data possible at all, along with the shared dialog/registry helpers this program's interface is built from. Reused under his published Code Reuse Policy on the [Hood Exporter notes page](https://www.picknmixmods.com/Sims2/Notes/HoodExporter/HoodExporter.html) (reuse permitted, credit appreciated though not required; not for sale). Thank you.

## License / terms for this program

Permission granted to use this program in any way, except to claim it as your own or sell it — the same terms carried in every source file this project wrote, in the same spirit as the Sims2Tools code it's built on.

## Reporting a problem

If the exported `.ged` doesn't look right, or the program errors out, note:
- which neighborhood folder you pointed it at (and roughly how big — number of Sims, sub-hoods present),
- which checkboxes were on,
- the exact text of the report panel (or error dialog) after the export finished or failed.

That's the same information that found and fixed the last real bug in this tool.
