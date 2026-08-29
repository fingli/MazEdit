# MazEdit

A Windows desktop editor for Mazatrol Nexus 2 / Matrix sub-program (`.maz`) files. It reads the binary format, shows program units in a grid, and lets you inspect and edit coordinates in memory.

> **Note:** The `.maz` layout is reverse-engineered. Always keep a backup of original files before testing edits on a machine.

## Requirements

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build and run

```bash
dotnet build
dotnet run
```

Or open `MazEdit.slnx` in Visual Studio and run the project.

## Usage

1. Click **OPEN .MAZ** and select a sub-program file.
2. Review units in the grid (unit number, type, PAD-style summary, coordinates).
3. Edit coordinate cells as needed.

The **Save** button is disabled — write-back is not implemented yet, so changes stay in memory only.

## Working with sample files

`.maz` files usually cannot be uploaded to chat or GitHub. Use files locally instead:

1. Copy your program into `TestData/` (this folder is gitignored).
2. Run MazEdit and use **OPEN .MAZ** to load it from there.
3. Click **EXPORT DUMP** to save a `.txt` report — hex + parsed units. That text file **is** safe to paste or upload here for reverse-engineering.

Example local path:

```text
C:\Users\yulyo\source\repos\MazEdit\TestData\S120_B0.maz
```

## Project layout

| File | Purpose |
|------|---------|
| `MazParser.cs` | Binary parser for `.maz` sub-programs |
| `MazProgram.cs` / `MazUnit.cs` | Parsed data models |
| `MainWindow.xaml` | UI (open file, unit grid, status bar) |

## Known unit markers

Mapped from `TEST.MAZ` against its PAD listing. Other programs may still show `CODE XX`.

| Code | Label |
|------|-------|
| file header | SETUP (material, INITIAL-Z at `0x28`) |
| `0xA0` | OFS point (Unit 0) |
| `0x0C` | INDEX |
| `0x02` | WPC |
| `0x03` | OFFSET |
| `0x40` | LINE CTR |
| `0xB1` | TOOL (child of machining unit) |
| `0xC2` | FIGURE LINE / CW (child) |
| `0x04` | END |
| Other | Shown as `CODE XX` |

## License

[MIT](LICENSE)
