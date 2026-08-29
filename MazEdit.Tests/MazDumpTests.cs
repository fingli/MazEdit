using System.IO;
using Xunit;

namespace MazEdit.Tests;

public class MazDumpTests
{
    [Fact]
    public void BuildReport_IncludesPadListingAndHex()
    {
        byte[] data = MazFileBuilder.TestMazLayout();
        var (program, path) = MazFileBuilder.ParseKeepingFile(data);
        try
        {
            string report = MazDump.BuildReport(path, program);

            Assert.Contains("Material: CST IRN", report);
            Assert.Contains("Initial-Z: 200", report);
            Assert.Contains("Multi mode: 3", report);
            Assert.Contains("=== Program (PAD-style) ===", report);
            Assert.Contains("U0  SETUP", report);
            Assert.Contains("  S1  OFS", report);
            Assert.Contains("U4  LINE CTR", report);
            Assert.Contains("  S1  TOOL", report);
            Assert.Contains("=== Header (first 0x100 bytes) ===", report);
            Assert.Contains("=== Raw blocks ===", report);
            Assert.Contains("0x0064", report);
            Assert.DoesNotContain("Name:", report);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void WriteReport_WritesUtf8File()
    {
        byte[] data = MazFileBuilder.Header();
        var (program, mazPath) = MazFileBuilder.ParseKeepingFile(data);
        string outPath = Path.Combine(Path.GetTempPath(), $"maz-dump-{Guid.NewGuid():N}.txt");
        try
        {
            MazDump.WriteReport(mazPath, program, outPath);

            Assert.True(File.Exists(outPath));
            string text = File.ReadAllText(outPath);
            Assert.Contains("SETUP", text);
        }
        finally
        {
            File.Delete(mazPath);
            if (File.Exists(outPath))
                File.Delete(outPath);
        }
    }

    [Fact]
    public void BuildReport_ClipsLastHexLineToBlockSize()
    {
        var data = MazFileBuilder.Header(blockCount: 2);
        MazFileBuilder.WriteBlock(data, 0, 0x03, 1);
        MazFileBuilder.WriteBlock(data, 1, 0xA0, 2);

        var (program, path) = MazFileBuilder.ParseKeepingFile(data);
        try
        {
            string report = MazDump.BuildReport(path, program);
            int firstBlock = report.IndexOf("--- Offset 0x0064", StringComparison.Ordinal);
            int secondBlock = report.IndexOf("--- Offset 0x00C8", StringComparison.Ordinal);
            Assert.InRange(firstBlock, 0, secondBlock);

            string firstSection = report[firstBlock..secondBlock];
            Assert.DoesNotContain("0x00C8", firstSection);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
