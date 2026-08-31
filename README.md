# MazEdit

A Windows desktop viewer for Mazatrol Nexus 2 / Matrix sub-program (`.maz`) files. It reads the packed binary format and shows units in a grid with PAD-style field names and values.

Layout was mapped from `TEST.MAZ` against its PAD listing (MG3-252). Other programs may still show unknown markers as `CODE XX`.

Packed `.maz` files do not store the EIA **O** program number. The name on SETUP is the file name (for `TEST.MAZ`, **TEST**). Open a `.pad` / three-digit G-format listing to read `O99999999 (MG3-251 : name)` or `<NAME>(MG3-252)`. Names longer than 48 characters are truncated, as on the control.

> **Note:** The `.maz` format is reverse-engineered. Keep a backup of original files. **Save is not implemented** — the app does not write back to disk.

## Requirements

- Windows (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build
- Testers receiving a self-contained publish do **not** need the SDK

## Build and run

```bash
dotnet build
dotnet run
```

Or open `MazEdit.slnx` in Visual Studio.

### Share a test build

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish
```

Zip the `publish` folder and send `MazEdit.exe` with any files next to it.

## Usage

1. Click **OPEN .MAZ** (`.pad` listings are also accepted).
2. Review **UNo**, **SNo**, **TYPE**, and **SUMMARY**. Child rows (OFS, TOOL, figures) keep the parent unit number.
3. Parameter names are light blue; values are white.
4. **EXPORT DUMP** writes a text report (PAD-style listing + hex). Use that when you cannot upload a `.maz` file.

The **Save** button stays disabled until binary write-back exists.

## Working with sample files

Do not commit real `.maz` programs. Copy them into `TestData/` (gitignored) and open them from there.

## Project layout

| File | Purpose |
|------|---------|
| `MazatrolCatalog.cs` | Shared Mazatrol unit names, field lists, and option labels |
| `MazEiaHeader.cs` | EIA/PAD program number and name |
| `MazProgram.cs` / `MazUnit.cs` | Parsed data models |
| `MazDump.cs` | Text dump export |
| `MainWindow.xaml` | UI |
| `SummaryPartsConverter.cs` | Name vs value coloring in SUMMARY |

## Known unit markers

| Code | Label |
|------|-------|
| file header | Common unit (U0): NAME, MAT (`0x54` ASCII: CST IRN, DUCT IRN, CBN STL, ALY STL, STNLESS, ALUMINUM, L.C.STL, AL CAST), INITIAL-Z (`0x28`), ATC MODE (`0x08`), MULTI MODE (`0x09`: 1=OFF, 2=5*2, 3=OFFSET TYPE) |
| `0xA0` | OFS point (child of SETUP) |
| `0x0A` | PROCESS (main) — P (`+4`) |
| `0x0C` | INDEX |
| `0x05` | SUB PRO — NAME (`+36` ASCII), L (`+20`); PAD also has F/K (0 in 1_TSK) |
| `0x02` | WPC |
| `0x03` | OFFSET |
| `0x40` | LINE CTR (central linear) — DEPTH, SRV-Z, SRV-R, RGH, FIN-Z; START/END not decoded yet |
| `0xB1` | TOOL (child) — type (`+9`: 1 CTR-DR … 19 BAL EMIL), Φ, letter, No, ZFD, DEP-Z, C-SP, FR, M-codes; APRCH-X/Y only if set |
| `0xC2` | FIGURE LINE / CW (child) |
| `0x04` | END — CONTI (`+9`), NUMBER (`+10`), ATC (`+11`), RETURN (`+8`: 0=None, 1=Machine zero point, 2=Fixed point, 3=Arbitrary), WORK No. (`+16` if set), NAME (`+36` ASCII if set), EXECUTE (`+20`: 0=YES, 1=NO) |
| Other | `CODE XX` |

## License

[MIT](LICENSE)
