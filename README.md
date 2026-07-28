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
2. Review units in the grid (sequence, type, coordinates, name/command).
3. Edit coordinate cells as needed.

The **Save** button is disabled — write-back is not implemented yet, so changes stay in memory only.

## Project layout

| File | Purpose |
|------|---------|
| `MazParser.cs` | Binary parser for `.maz` sub-programs |
| `MazProgram.cs` / `MazUnit.cs` | Parsed data models |
| `MainWindow.xaml` | UI (open file, unit grid, status bar) |

## Known unit markers

| Code | Label |
|------|-------|
| `0xA0` | Unit header |
| `0x04` | Sub call |
| `0x0C` | WPC / coord shift |
| `0x02` | Shape / line |
| `0x66` | Tool path |
| `0xB2` | Tool data |
| Other | Shown as `CODE XX` |

## License

[MIT](LICENSE)
