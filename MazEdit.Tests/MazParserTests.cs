using Xunit;

namespace MazEdit.Tests;

public class MazParserTests
{
    [Fact]
    public void ParseSubProgram_MissingFile_ReturnsEmptyProgram()
    {
        var program = new MazParser().ParseSubProgram(@"C:\no-such-file\missing.maz");

        Assert.Empty(program.Units);
        Assert.Equal(string.Empty, program.Material);
        Assert.Equal(0, program.InitialZ);
    }

    [Fact]
    public void ParseSubProgram_FileShorterThanHeader_ReturnsEmptyProgram()
    {
        var program = MazFileBuilder.Parse(new byte[0x50]);

        Assert.Empty(program.Units);
    }

    [Fact]
    public void ParseSubProgram_HeaderOnly_AddsSetupRow()
    {
        var program = MazFileBuilder.Parse(MazFileBuilder.Header());

        MazUnit setup = Assert.Single(program.Units);
        Assert.Equal("SETUP", setup.TypeName);
        Assert.False(setup.IsChild);
        Assert.Equal(0, setup.UnitNo);
        Assert.Contains("MAT=CST IRN", setup.Summary);
        Assert.Contains("INITIAL-Z=200", setup.Summary);
        Assert.Contains("ATC MODE=0", setup.Summary);
        Assert.Contains("MULTI MODE=OFFSET TYPE", setup.Summary);
        Assert.Equal(200, program.InitialZ);
        Assert.Equal(0, program.AtcMode);
        Assert.Equal(3, program.MultiMode);
        Assert.Equal("CST IRN", program.Material);
    }

    [Theory]
    [InlineData(1, "OFF")]
    [InlineData(2, "5 * 2")]
    [InlineData(3, "OFFSET TYPE")]
    public void ParseSubProgram_MultiMode_MapsKnownOptions(byte mode, string label)
    {
        var data = MazFileBuilder.Header(multiMode: mode);
        var program = MazFileBuilder.Parse(data);

        Assert.Contains($"MULTI MODE={label}", program.Units[0].Summary);
    }

    [Fact]
    public void ParseSubProgram_UnknownMultiMode_UsesNumericLabel()
    {
        var data = MazFileBuilder.Header(multiMode: 9);
        var program = MazFileBuilder.Parse(data);

        Assert.Contains("MULTI MODE=MODE 9", program.Units[0].Summary);
    }

    [Fact]
    public void ParseSubProgram_TrimsNullPaddedMaterial()
    {
        var data = MazFileBuilder.Header(material: "AL");
        var program = MazFileBuilder.Parse(data);

        Assert.Equal("AL", program.Material);
    }

    [Fact]
    public void ParseSubProgram_SkipsZeroMarkerBlocks()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        var program = MazFileBuilder.Parse(data);

        Assert.Single(program.Units);
        Assert.Equal("SETUP", program.Units[0].TypeName);
    }

    [Fact]
    public void ParseSubProgram_IncludesLastFullBlock()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x03, 1);

        var program = MazFileBuilder.Parse(data);

        Assert.Equal(2, program.Units.Count);
        Assert.Equal("OFFSET", program.Units[1].TypeName);
        Assert.Equal(MazFileBuilder.BlockStart, program.Units[1].FileOffset);
    }

    [Fact]
    public void ParseSubProgram_OfsAndToolAreChildren()
    {
        var data = MazFileBuilder.Header(blockCount: 3);
        MazFileBuilder.WriteBlock(data, 0, 0xA0, 1);
        MazFileBuilder.WriteBlock(data, 1, 0x40, 4);
        MazFileBuilder.WriteBlock(data, 2, 0xB1, 1);

        var program = MazFileBuilder.Parse(data);

        Assert.Equal("OFS", program.Units[1].TypeName);
        Assert.True(program.Units[1].IsChild);
        Assert.Equal(0, program.Units[1].UnitNo);

        Assert.Equal("LINE CTR", program.Units[2].TypeName);
        Assert.False(program.Units[2].IsChild);

        Assert.Equal("TOOL", program.Units[3].TypeName);
        Assert.True(program.Units[3].IsChild);
        Assert.Equal(4, program.Units[3].UnitNo);
    }

    [Fact]
    public void ParseSubProgram_Index_MapsAngleAndNearDir()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x0C, 1);
        MazFileBuilder.WriteByte(data, 0, 8, 2);
        MazFileBuilder.WriteCoord(data, 0, 40, 180);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("INDEX", unit.TypeName);
        Assert.Equal(180, unit.Parameter);
        Assert.Equal(0, unit.Y_Coord);
        Assert.Contains("ANGLE=180", unit.Summary);
        Assert.Contains("DIR=NEAR DIR", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Process_ReadsProcessNumber()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x0A, 1);
        MazFileBuilder.WriteInt16(data, 0, 4, 1);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("PROCESS", unit.TypeName);
        Assert.False(unit.IsChild);
        Assert.Equal("P=1", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_SubPro_ReadsNameAndL()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x05, 3);
        MazFileBuilder.WriteInt32(data, 0, 20, 1);
        MazFileBuilder.WriteAscii(data, 0, 36, "PUSHKA_25");

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("SUB PRO", unit.TypeName);
        Assert.False(unit.IsChild);
        Assert.Equal("NAME=PUSHKA_25  L=1", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Drill_ReadsDiaDepthChmf_AndKeepsPointChildren()
    {
        var data = MazFileBuilder.Header(blockCount: 3);
        MazFileBuilder.WriteBlock(data, 0, 0x20, 5);
        MazFileBuilder.WriteCoord(data, 0, 36, 54);
        MazFileBuilder.WriteCoord(data, 0, 40, 10);
        MazFileBuilder.WriteCoord(data, 0, 44, 2.5f);
        MazFileBuilder.WriteBlock(data, 1, 0xB0, 1);
        MazFileBuilder.WriteByte(data, 1, 9, 2);
        MazFileBuilder.WriteByte(data, 1, 11, 24);
        MazFileBuilder.WriteInt16(data, 1, 22, 3);
        MazFileBuilder.WriteCoord(data, 1, 36, 54);
        MazFileBuilder.WriteBlock(data, 2, 0xC0, 1);
        MazFileBuilder.WriteByte(data, 2, 8, 1);
        MazFileBuilder.WriteCoord(data, 2, 36, -0.15f);

        var program = MazFileBuilder.Parse(data);
        MazUnit drill = program.Units[1];
        MazUnit tool = program.Units[2];
        MazUnit pnt = program.Units[3];

        Assert.Equal("DRILL", drill.TypeName);
        Assert.Equal("DIA=54  DEPTH=10  CHMF=2.5", drill.Summary);
        Assert.False(drill.IsChild);
        Assert.Equal(5, tool.UnitNo);
        Assert.True(tool.IsChild);
        Assert.Equal("TOOL", tool.TypeName);
        Assert.Contains("DRILL", tool.Summary);
        Assert.Contains("S=24", tool.Summary);
        Assert.Equal(5, pnt.UnitNo);
        Assert.True(pnt.IsChild);
        Assert.Equal("PNT", pnt.TypeName);
        Assert.Contains("A=1", pnt.Summary);
        Assert.Contains("X=-0.15", pnt.Summary);
    }

    [Fact]
    public void ParseSubProgram_Tap_ReadsNomMajorPitch()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x24, 11);
        MazFileBuilder.WriteInt16(data, 0, 36, 1);
        MazFileBuilder.WriteCoord(data, 0, 40, 10);
        MazFileBuilder.WriteCoord(data, 0, 44, 1.5f);
        MazFileBuilder.WriteCoord(data, 0, 48, 23.5f);
        MazFileBuilder.WriteCoord(data, 0, 52, 1);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("TAP", unit.TypeName);
        Assert.Contains("NOM=M", unit.Summary);
        Assert.Contains("MAJOR-φ=10", unit.Summary);
        Assert.Contains("PITCH=1.5", unit.Summary);
        Assert.Contains("TAP-DEP=23.5", unit.Summary);
        Assert.Contains("CHMF=1", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Manual_ReadsToolAndPathChildren()
    {
        var data = MazFileBuilder.Header(blockCount: 2);
        MazFileBuilder.WriteBlock(data, 0, 0x06, 6);
        MazFileBuilder.WriteByte(data, 0, 9, 15);
        MazFileBuilder.WriteByte(data, 0, 11, 24);
        MazFileBuilder.WriteInt32(data, 0, 24, 4);
        MazFileBuilder.WriteCoord(data, 0, 36, 50);
        MazFileBuilder.WriteBlock(data, 1, 0xA1, 1);
        MazFileBuilder.WriteCoord(data, 1, 36, -0.15f);
        MazFileBuilder.WriteCoord(data, 1, 44, 200);
        MazFileBuilder.WriteInt32(data, 1, 60, 950);

        var program = MazFileBuilder.Parse(data);
        MazUnit manual = program.Units[1];
        MazUnit path = program.Units[2];

        Assert.Equal("MANUAL", manual.TypeName);
        Assert.Equal("END MILL  Φ=50  S=24  P=4", manual.Summary);
        Assert.Equal(6, path.UnitNo);
        Assert.True(path.IsChild);
        Assert.Equal("PATH", path.TypeName);
        Assert.Contains("X=-0.15", path.Summary);
        Assert.Contains("Z=200", path.Summary);
        Assert.Contains("F=950", path.Summary);
    }

    [Fact]
    public void ParseSubProgram_Wpc_UsesNumberAndCoords()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x02, 2);
        MazFileBuilder.WriteInt32(data, 0, 8, 2);
        MazFileBuilder.WriteCoord(data, 0, 36, -102.5f);
        MazFileBuilder.WriteCoord(data, 0, 40, -552f);
        MazFileBuilder.WriteCoord(data, 0, 48, -541.5f);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("WPC-2", unit.TypeName);
        Assert.Equal(-102.5f, unit.X_Coord);
        Assert.Equal(-552f, unit.Y_Coord);
        Assert.Equal(-541.5f, unit.Parameter);
        Assert.Contains("X=-102.5", unit.Summary);
        Assert.Contains("Z=-541.5", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Offset_UsesUvwLabels()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x03, 3);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("OFFSET", unit.TypeName);
        Assert.Contains("U(X)=0", unit.Summary);
        Assert.Contains("W(Z)=0", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_LineCtr_ReadsRghAndServes()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x40, 4);
        MazFileBuilder.WriteByte(data, 0, 17, 3);
        MazFileBuilder.WriteCoord(data, 0, 40, 3);
        MazFileBuilder.WriteCoord(data, 0, 44, 30);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("LINE CTR", unit.TypeName);
        Assert.Contains("SRV-Z=3", unit.Summary);
        Assert.Contains("SRV-R=30", unit.Summary);
        Assert.Contains("RGH=3", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Tool_MatchesControlRow()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xB1, 1);
        MazFileBuilder.WriteByte(data, 0, 9, 15);
        MazFileBuilder.WriteByte(data, 0, 11, 9);
        MazFileBuilder.WriteInt16(data, 0, 20, 0x40);
        MazFileBuilder.WriteInt16(data, 0, 22, 3);
        MazFileBuilder.WriteInt16(data, 0, 24, 8);
        MazFileBuilder.WriteInt16(data, 0, 26, 51);
        MazFileBuilder.WriteCoord(data, 0, 36, 63);
        MazFileBuilder.WriteCoord(data, 0, 48, 3);
        MazFileBuilder.WriteInt32(data, 0, 60, 180);
        MazFileBuilder.WriteCoord(data, 0, 64, 1.2f);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("TOOL", unit.TypeName);
        Assert.Equal("END MILL  Φ=63  J  No=3  ZFD=G01  DEP-Z=3  C-SP=180  FR=1.2  M08 M51", unit.Summary);
        Assert.DoesNotContain("APRCH", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Tool_ShowsApproachWhenSet()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xB1, 1);
        MazFileBuilder.WriteCoord(data, 0, 40, 1.5f);
        MazFileBuilder.WriteCoord(data, 0, 44, 2.5f);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Contains("APRCH-X=1.5", unit.Summary);
        Assert.Contains("APRCH-Y=2.5", unit.Summary);
        Assert.Equal(1.5f, unit.Y_Coord);
        Assert.Equal(2.5f, unit.Z_Coord);
    }

    [Fact]
    public void ParseSubProgram_Tool_OmitsZeroMCodes_AndUnknownType()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xB1, 1);
        MazFileBuilder.WriteByte(data, 0, 9, 99);
        MazFileBuilder.WriteInt16(data, 0, 20, 0);
        MazFileBuilder.WriteInt16(data, 0, 24, 0);
        MazFileBuilder.WriteInt16(data, 0, 26, 0);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Contains("T99", unit.Summary);
        Assert.Contains("ZFD=G00", unit.Summary);
        Assert.DoesNotContain("M00", unit.Summary);
        Assert.DoesNotContain("M 0", unit.Summary);
    }

    [Theory]
    [InlineData(1, "CTR-DR")]
    [InlineData(2, "DRILL")]
    [InlineData(3, "REAMER")]
    [InlineData(4, "TAP (M)")]
    [InlineData(5, "TAP (UN)")]
    [InlineData(6, "TAP (PT)")]
    [InlineData(7, "TAP (PF)")]
    [InlineData(8, "TAP (PS)")]
    [InlineData(9, "TAP (OTHER)")]
    [InlineData(10, "BCK FACE")]
    [InlineData(11, "BOR BAR")]
    [InlineData(12, "B-B BAR")]
    [InlineData(13, "CHAMFER")]
    [InlineData(14, "FCE MILL")]
    [InlineData(15, "END MILL")]
    [InlineData(16, "OTHER")]
    [InlineData(17, "CHIP VAC")]
    [InlineData(18, "T. SENS.")]
    [InlineData(19, "BAL EMIL")]
    public void ParseSubProgram_Tool_MapsMazatrolToolTypes(byte code, string name)
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xB1, 1);
        MazFileBuilder.WriteByte(data, 0, 9, code);

        Assert.Contains(name, MazFileBuilder.Parse(data).Units[1].Summary);
    }

    [Fact]
    public void ParseSubProgram_Figure_LineUsesSwappedXy()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xC2, 1);
        MazFileBuilder.WriteByte(data, 0, 8, 0x20);
        MazFileBuilder.WriteCoord(data, 0, 36, 0);
        MazFileBuilder.WriteCoord(data, 0, 40, 53.75f);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("LINE", unit.TypeName);
        Assert.Equal(53.75f, unit.X_Coord);
        Assert.Equal(0, unit.Y_Coord);
        Assert.Contains("X=53.75", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_Figure_OddTypeByteIsCw()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xC2, 2);
        MazFileBuilder.WriteByte(data, 0, 8, 0x21);
        MazFileBuilder.WriteCoord(data, 0, 48, 53.75f);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("CW", unit.TypeName);
        Assert.Contains("R/th=53.75", unit.Summary);
    }

    [Fact]
    public void ParseSubProgram_End_ReadsContiAtcReturnAndExecute()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x04, 5);
        MazFileBuilder.WriteByte(data, 0, 8, 2);
        MazFileBuilder.WriteByte(data, 0, 9, 1);
        MazFileBuilder.WriteByte(data, 0, 10, 1);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("END", unit.TypeName);
        Assert.False(unit.IsChild);
        Assert.Equal("CONTI=1  NUMBER=1  ATC=0  RETURN=Fixed point  EXECUTE=YES", unit.Summary);
        Assert.DoesNotContain("WORK No.", unit.Summary);
        Assert.DoesNotContain("DIR=", unit.Summary);
        Assert.DoesNotContain("NAME=", unit.Summary);
    }

    [Theory]
    [InlineData(0, "None")]
    [InlineData(1, "Machine zero point")]
    [InlineData(2, "Fixed point")]
    [InlineData(3, "Arbitrary")]
    public void ParseSubProgram_End_MapsReturnOptions(byte code, string label)
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x04, 1);
        MazFileBuilder.WriteByte(data, 0, 8, code);

        Assert.Contains($"RETURN={label}", MazFileBuilder.Parse(data).Units[1].Summary);
    }

    [Theory]
    [InlineData(0, "YES")]
    [InlineData(1, "NO")]
    public void ParseSubProgram_End_MapsExecuteOptions(byte code, string label)
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x04, 1);
        MazFileBuilder.WriteByte(data, 0, 20, code);

        Assert.Contains($"EXECUTE={label}", MazFileBuilder.Parse(data).Units[1].Summary);
    }

    [Fact]
    public void ParseSubProgram_End_ShowsWorkNoWhenSet()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x04, 1);
        MazFileBuilder.WriteInt16(data, 0, 16, 7);

        Assert.Contains("WORK No.=7", MazFileBuilder.Parse(data).Units[1].Summary);
    }

    [Fact]
    public void ParseSubProgram_End_ShowsNameWhenSet()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x04, 11);
        MazFileBuilder.WriteByte(data, 0, 8, 2);
        MazFileBuilder.WriteAscii(data, 0, 36, "2_TSK");

        Assert.Contains("NAME=2_TSK", MazFileBuilder.Parse(data).Units[1].Summary);
    }

    [Fact]
    public void ParseSubProgram_UnknownMarker_ShowsCodeXx()
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0x99, 1);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Equal("CODE 99", unit.TypeName);
    }

    [Fact]
    public void ParseSubProgram_TestMazLayout_MatchesControlListing()
    {
        var program = MazFileBuilder.Parse(MazFileBuilder.TestMazLayout());
        string[] types = program.Units.Select(u => u.TypeName).ToArray();

        Assert.Equal(
        [
            "SETUP", "OFS", "OFS", "INDEX", "WPC-2", "OFFSET",
            "LINE CTR", "TOOL", "LINE", "CW", "END"
        ], types);

        Assert.True(program.Units[1].IsChild);
        Assert.True(program.Units[7].IsChild);
        Assert.Equal(4, program.Units[7].UnitNo);
        Assert.Contains("Φ=63", program.Units[7].Summary);
        Assert.Contains("M08 M51", program.Units[7].Summary);
        Assert.Equal(-102.5f, program.Units[4].X_Coord);
    }

    [SkippableFact]
    public void ParseSubProgram_RealTestMaz_MatchesControlListing()
    {
        string? path = GoldenMaz.TryFindTestMaz();
        Skip.If(path is null,
            "Copy TEST.MAZ into TestData/ (gitignored) or set MAZEDIT_TEST_MAZ to the file path.");

        var program = new MazParser().ParseSubProgram(path);

        Assert.Equal("CST IRN", program.Material);
        Assert.Equal(200, program.InitialZ);
        Assert.Equal(3, program.MultiMode);
        Assert.Contains("MULTI MODE=OFFSET TYPE", program.Units[0].Summary);
        Assert.Contains($"NAME={program.ProgramName}", program.Units[0].Summary);

        string[] types = program.Units.Select(u => u.TypeName).ToArray();
        Assert.Equal(
        [
            "SETUP", "OFS", "OFS", "INDEX", "WPC-2", "OFFSET",
            "LINE CTR", "TOOL", "LINE", "CW", "END"
        ], types);

        MazUnit ofs2 = program.Units[2];
        Assert.True(ofs2.IsChild);
        Assert.Contains("X=205", ofs2.Summary);

        MazUnit index = program.Units[3];
        Assert.Contains("ANGLE=180", index.Summary);
        Assert.Contains("DIR=NEAR DIR", index.Summary);

        MazUnit wpc = program.Units[4];
        Assert.Equal(-102.5f, wpc.X_Coord);
        Assert.Equal(-552f, wpc.Y_Coord);
        Assert.Equal(-541.5f, wpc.Parameter);

        MazUnit tool = program.Units[7];
        Assert.True(tool.IsChild);
        Assert.Equal(4, tool.UnitNo);
        Assert.Equal("END MILL  Φ=63  J  No=3  ZFD=G01  DEP-Z=3  C-SP=180  FR=1.2  M08 M51", tool.Summary);

        MazUnit line = program.Units[8];
        Assert.Equal("LINE", line.TypeName);
        Assert.Equal(53.75f, line.X_Coord);

        MazUnit cw = program.Units[9];
        Assert.Equal("CW", cw.TypeName);
        Assert.Contains("R/th=53.75", cw.Summary);

        MazUnit end = program.Units[10];
        Assert.Contains("CONTI=1", end.Summary);
        Assert.Contains("NUMBER=1", end.Summary);
        Assert.Contains("RETURN=Fixed point", end.Summary);
        Assert.Contains("EXECUTE=YES", end.Summary);
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(9, "J")]
    [InlineData(25, "Z")]
    public void ParseSubProgram_ToolLetter_MapsIndexToAThroughZ(byte index, string letter)
    {
        var data = MazFileBuilder.Header(blockCount: 1);
        MazFileBuilder.WriteBlock(data, 0, 0xB1, 1);
        MazFileBuilder.WriteByte(data, 0, 11, index);

        var unit = MazFileBuilder.Parse(data).Units[1];

        Assert.Contains($"  {letter}  No=", unit.Summary);
    }
}
