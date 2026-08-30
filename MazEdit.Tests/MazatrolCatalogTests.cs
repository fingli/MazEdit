using Xunit;

namespace MazEdit.Tests;

public class MazatrolCatalogTests
{
    [Fact]
    public void MachiningUnits_ListsAllPointLineAndFaceOperations()
    {
        string[] names = MazatrolCatalog.MachiningUnits.Select(u => u.TypeName).ToArray();

        Assert.Equal(23, names.Length);
        Assert.Equal(names.Distinct().Count(), names.Length);
        Assert.Contains("DRILL", names);
        Assert.Contains("LINE CTR", names);
        Assert.Contains("STEP EMILL", names);
        Assert.All(MazatrolCatalog.MachiningUnits, u => Assert.Equal("UNo.", u.Fields[0]));
    }

    [Fact]
    public void SharedOptions_AreReusedAcrossUnits()
    {
        Assert.Contains(MazatrolCatalog.Field.Nom, MazatrolCatalog.Tapping.Fields);
        Assert.Contains(MazatrolCatalog.Field.Nom, MazatrolCatalog.CounterboreTapping.Fields);
        Assert.Contains(MazatrolCatalog.Field.RChamferFlag, MazatrolCatalog.RightHandLinear.Fields);
        Assert.Contains(MazatrolCatalog.Field.RChamferFlag, MazatrolCatalog.InsideLinear.Fields);
        Assert.Contains(MazatrolCatalog.Field.Start, MazatrolCatalog.CentralLinear.Fields);
        Assert.Contains(MazatrolCatalog.Field.Start, MazatrolCatalog.RightHandChamfering.Fields);
    }

    [Theory]
    [InlineData(0, "Drilling")]
    [InlineData(1, "Boring")]
    [InlineData(2, "End milling")]
    public void PreReam_MapsSharedOptions(int code, string label)
        => Assert.Equal(label, MazatrolCatalog.PreReam(code));

    [Theory]
    [InlineData(1, "M")]
    [InlineData(2, "UNn")]
    [InlineData(7, "OTHER")]
    public void TapNom_MapsSharedScrewTypes(int code, string label)
        => Assert.Equal(label, MazatrolCatalog.TapNom(code));

    [Theory]
    [InlineData(0, "CIRCUL")]
    [InlineData(1, "TORNADO")]
    public void Torna_MapsSharedOptions(int code, string label)
        => Assert.Equal(label, MazatrolCatalog.Torna(code));

    [Theory]
    [InlineData(0, "OPEN")]
    [InlineData(1, "CLOSED")]
    public void OpenClosed_MapsStartEndBits(int bit, string label)
        => Assert.Equal(label, MazatrolCatalog.OpenClosed(bit));

    [Theory]
    [InlineData(0, "Chamfering")]
    [InlineData(1, "Rounding")]
    public void ChamferOrRound_MapsRChamferFlag(int code, string label)
        => Assert.Equal(label, MazatrolCatalog.ChamferOrRound(code));

    [Fact]
    public void PackedMarkers_StayOnKnownProgramAndLineCtrUnits()
    {
        Assert.Equal((byte)0x40, MazatrolCatalog.CentralLinear.PackedMarker);
        Assert.Equal((byte)0x04, MazatrolCatalog.End.PackedMarker);
        Assert.Equal("G366", MazatrolCatalog.InsideLinear.GCode);
        Assert.Equal("G367", MazatrolCatalog.RightHandChamfering.GCode);
        Assert.Null(MazatrolCatalog.Drilling.PackedMarker);
    }
}
