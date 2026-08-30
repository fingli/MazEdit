using System.IO;
using System.Text;
using Xunit;

namespace MazEdit.Tests;

public class MazEiaHeaderTests
{
    [Fact]
    public void TryParse_ONumberWithoutName()
    {
        Assert.True(MazEiaHeader.TryParse("O12 (MG3-251)\nG300U0", out MazEiaHeader header));
        Assert.Equal(12, header.ProgramNumber);
        Assert.Equal("MG3-251", header.FormatId);
        Assert.Equal("", header.ProgramName);
    }

    [Fact]
    public void TryParse_ONumberWithName_TruncatesAt48Characters()
    {
        string longName = new('A', 60);
        Assert.True(MazEiaHeader.TryParse($"O99999999 (MG3-251 : {longName})", out MazEiaHeader header));
        Assert.Equal(99999999, header.ProgramNumber);
        Assert.Equal(48, header.ProgramName.Length);
        Assert.Equal("MG3-251", header.FormatId);
    }

    [Fact]
    public void TryParse_AngleBracketName_Mg3252()
    {
        Assert.True(MazEiaHeader.TryParse("<TEST>(MG3-252)\r\nG300U0", out MazEiaHeader header));
        Assert.Null(header.ProgramNumber);
        Assert.Equal("TEST", header.ProgramName);
        Assert.Equal("MG3-252", header.FormatId);
    }

    [Fact]
    public void ParseSubProgram_EiaFile_DoesNotScanBinaryUnits()
    {
        byte[] pad = Encoding.ASCII.GetBytes("O42 (MG3-251 : DEMO)\r\nG300U0P0\r\n%\r\n");
        string path = Path.Combine(Path.GetTempPath(), "demo.pad");
        File.WriteAllBytes(path, pad);
        try
        {
            var program = new MazParser().ParseSubProgram(path);
            Assert.Equal(42, program.ProgramNumber);
            Assert.Equal("DEMO", program.ProgramName);
            Assert.Equal("MG3-251", program.FormatId);
            MazUnit setup = Assert.Single(program.Units);
            Assert.Equal("SETUP", setup.TypeName);
            Assert.Contains("NAME=DEMO", setup.Summary);
            Assert.Contains("O=42", setup.Summary);
            Assert.Contains("FORMAT=MG3-251", setup.Summary);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ParseSubProgram_PackedMaz_UsesFileNameAsProgramName()
    {
        var program = MazFileBuilder.Parse(MazFileBuilder.Header(), "TEST.maz");
        Assert.Equal("TEST", program.ProgramName);
        Assert.Null(program.ProgramNumber);
        Assert.Contains("NAME=TEST", program.Units[0].Summary);
    }
}
