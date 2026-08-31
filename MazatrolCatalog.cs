namespace MazEdit
{
    /// <summary>
    /// Mazatrol mill unit names, field lists, and option labels that several units share.
    /// Packed byte markers are filled in only where the .maz layout is known.
    /// </summary>
    public sealed class MazatrolUnitKind
    {
        public MazatrolUnitKind(string typeName, string title, IReadOnlyList<string> fields,
            byte? packedMarker = null)
        {
            TypeName = typeName;
            Title = title;
            Fields = fields;
            PackedMarker = packedMarker;
        }

        public string TypeName { get; }
        public string Title { get; }
        public IReadOnlyList<string> Fields { get; }
        public byte? PackedMarker { get; }
    }

    public static class MazatrolCatalog
    {
        public static class Field
        {
            public const string Uno = "UNo.";
            public const string Dia = "DIA";
            public const string Depth = "DEPTH";
            public const string Chmf = "CHMF";
            public const string CbDia = "CB-DIA";
            public const string CbDep = "CB-DEP";
            public const string Btm = "BTM";
            public const string Wal = "WAL";
            public const string PreDia = "PRE-DIA";
            public const string PreDep = "PRE-DEP";
            public const string PreReam = "PRE-REAM";
            public const string Chp = "CHP";
            public const string Nom = "NOM-";
            public const string MajorPhi = "MAJOR-φ";
            public const string Pitch = "PITCH";
            public const string TapDep = "TAP-DEP";
            public const string Torna = "TORNA.";
            public const string Pitch1 = "PITCH1";
            public const string Pitch2 = "PITCH2";
            public const string SrvZ = "SRV-Z";
            public const string SrvR = "SRV-R";
            public const string Rgh = "RGH";
            public const string FinZ = "FIN-Z";
            public const string FinR = "FIN-R";
            public const string Start = "START";
            public const string End = "END";
            public const string InterR = "INTER-R";
            public const string InterZ = "INTER-Z";
            public const string RChamferFlag = "R-chamfering flag";
        }

        public static string MultiMode(int code) => code switch
        {
            1 => "OFF",
            2 => "5 * 2",
            3 => "OFFSET TYPE",
            _ => $"MODE {code}"
        };

        public static readonly IReadOnlyList<string> Materials =
        [
            "CST IRN",
            "DUCT IRN",
            "CBN STL",
            "ALY STL",
            "STNLESS",
            "ALUMINUM",
            "L.C.STL",
            "AL CAST"
        ];

        public static string Material(int code)
            => code is >= 1 and <= 8 ? Materials[code - 1] : $"MAT {code}";

        public static string EndReturn(int code) => code switch
        {
            0 => "None",
            1 => "Machine zero point",
            2 => "Fixed point",
            3 => "Arbitrary",
            _ => $"RETURN {code}"
        };

        public static string Execute(int code) => code switch
        {
            0 => "YES",
            1 => "NO",
            _ => $"EXECUTE {code}"
        };

        public static string TurnDir(byte code) => code switch
        {
            2 => "NEAR DIR",
            _ => $"DIR {code}"
        };

        public static string ToolType(byte code) => code switch
        {
            1 => "CTR-DR",
            2 => "DRILL",
            3 => "REAMER",
            4 => "TAP (M)",
            5 => "TAP (UN)",
            6 => "TAP (PT)",
            7 => "TAP (PF)",
            8 => "TAP (PS)",
            9 => "TAP (OTHER)",
            10 => "BCK FACE",
            11 => "BOR BAR",
            12 => "B-B BAR",
            13 => "CHAMFER",
            14 => "FCE MILL",
            15 => "END MILL",
            16 => "OTHER",
            17 => "CHIP VAC",
            18 => "T. SENS.",
            19 => "BAL EMIL",
            _ => $"T{code}"
        };

        public static string Zfd(int code) => code switch
        {
            0 => "G00",
            1 => "G01",
            0x40 => "G01",
            _ => $"G{code:D2}"
        };

        public static string FigureType(byte code) => (code & 0x01) == 0 ? "LINE" : "CW";

        public static string PreReam(int code) => code switch
        {
            0 => "Drilling",
            1 => "Boring",
            2 => "End milling",
            _ => $"PRE-REAM {code}"
        };

        public static string TapNom(int code) => code switch
        {
            1 => "M",
            2 => "UNn",
            3 => "UN",
            4 => "PT",
            5 => "PF",
            6 => "PS",
            7 => "OTHER",
            _ => $"NOM {code}"
        };

        public static string TapFraction(int code) => code switch
        {
            1 => "1/2",
            2 => "1/4",
            3 => "1/8",
            4 => "1/16",
            _ => $"FRAC {code}"
        };

        public static string Torna(int code) => code switch
        {
            0 => "CIRCUL",
            1 => "TORNADO",
            _ => $"TORNA {code}"
        };

        public static string OpenClosed(int bit) => bit switch
        {
            0 => "OPEN",
            1 => "CLOSED",
            _ => $"OPEN/CLOSED {bit}"
        };

        public static string ChamferOrRound(int code) => code switch
        {
            0 => "Chamfering",
            1 => "Rounding",
            _ => $"R-CHMF {code}"
        };

        public static readonly MazatrolUnitKind Setup = new(
            "SETUP", "Common unit",
            ["NAME", "MAT", "INITIAL-Z", "ATC MODE", "MULTI MODE"]);

        public static readonly MazatrolUnitKind Ofs = new(
            "OFS", "Offset point",
            ["X", "Y", "th", "Z"], packedMarker: 0xA0);

        public static readonly MazatrolUnitKind Index = new(
            "INDEX", "Index",
            ["TURN X", "Y", "Z", "ANGLE", "DIR"], packedMarker: 0x0C);

        public static readonly MazatrolUnitKind Process = new(
            "PROCESS", "Process unit (main program)",
            ["UNo.", "P"], packedMarker: 0x0A);

        public static readonly MazatrolUnitKind SubPro = new(
            "SUB PRO", "Subprogram call",
            ["UNo.", "NAME", "L", "F", "K"], packedMarker: 0x05);

        public static readonly MazatrolUnitKind Wpc = new(
            "WPC", "Workpiece coordinates",
            ["X", "Y", "th", "Z"], packedMarker: 0x02);

        public static readonly MazatrolUnitKind Offset = new(
            "OFFSET", "Offset",
            ["U(X)", "V(Y)", "D(th)", "W(Z)"], packedMarker: 0x03);

        public static readonly MazatrolUnitKind Tool = new(
            "TOOL", "Tool",
            ["T", "Φ", "letter", "No", "ZFD", "DEP-Z", "C-SP", "FR", "M"], packedMarker: 0xB1);

        public static readonly MazatrolUnitKind PointTool = new(
            "TOOL", "Point-machining tool",
            ["T", "S", "No", "Φ", "E", "H", "DEP-Z", "C-SP", "FR", "M"], packedMarker: 0xB0);

        public static readonly MazatrolUnitKind Figure = new(
            "FIGURE", "Figure line / arc",
            ["X", "Y", "R/th"], packedMarker: 0xC2);

        public static readonly MazatrolUnitKind PointFigure = new(
            "PNT", "Point/hole figure",
            ["A", "X", "Y", "DA", "DB", "TA", "TB"], packedMarker: 0xC0);

        public static readonly MazatrolUnitKind Manual = new(
            "MANUAL", "Manual program unit",
            ["T", "Φ", "S", "P"], packedMarker: 0x06);

        public static readonly MazatrolUnitKind ManualPath = new(
            "PATH", "Manual path segment",
            ["X", "Y", "Z", "F"], packedMarker: 0xA1);

        public static readonly MazatrolUnitKind End = new(
            "END", "End unit",
            ["CONTI.", "NUMBER", "ATC", "RETURN", "WORK No.", "NAME", "EXECUTE"], packedMarker: 0x04);

        public static readonly MazatrolUnitKind Drilling = new(
            "DRILL", "Drilling unit",
            [Field.Uno, Field.Dia, Field.Depth, Field.Chmf], packedMarker: 0x20);

        public static readonly MazatrolUnitKind RghCbor = new(
            "RGH CBOR", "RGH CBOR machining unit",
            [Field.Uno, Field.CbDia, Field.CbDep, Field.Chmf, Field.Btm, Field.Dia, Field.Depth]);

        public static readonly MazatrolUnitKind RghBcb = new(
            "RGH BCB", "RGH BCB machining unit",
            [Field.Uno, Field.CbDia, Field.CbDep, Field.Dia, Field.Depth, Field.Chmf]);

        public static readonly MazatrolUnitKind Reaming = new(
            "REAM", "Reaming unit",
            [Field.Uno, Field.Dia, Field.Depth, Field.Chmf, Field.PreReam, Field.Chp]);

        public static readonly MazatrolUnitKind Tapping = new(
            "TAP", "Tapping unit",
            [Field.Uno, Field.Nom, Field.MajorPhi, Field.Pitch, Field.TapDep, Field.Chmf, Field.Chp],
            packedMarker: 0x24);

        public static readonly MazatrolUnitKind BackBoring = new(
            "BCK BOR", "Back boring unit",
            [Field.Uno, Field.Dia, Field.Depth, Field.Btm, Field.Wal, Field.PreDia, Field.PreDep, Field.Chmf, Field.Wal]);

        public static readonly MazatrolUnitKind CircularMilling = new(
            "CIRC MILL", "Circular milling unit",
            [Field.Uno, Field.Dia, Field.Depth, Field.Chmf, Field.Torna, Field.Btm, Field.PreDia, Field.Chmf,
                Field.Pitch1, Field.Pitch2]);

        public static readonly MazatrolUnitKind CounterboreTapping = new(
            "CBOR TAP", "Counterbore-tapping unit",
            [Field.Uno, Field.Nom, Field.MajorPhi, Field.Pitch, Field.TapDep, Field.Chmf, Field.CbDia, Field.CbDep,
                Field.Chmf, Field.Btm, Field.Chp]);

        public static readonly MazatrolUnitKind ThruBoring = new(
            "THRU BOR", "Through hole boring unit",
            [Field.Uno, Field.Dia, Field.Depth, Field.Chmf, Field.Wal]);

        public static readonly MazatrolUnitKind NonThruBoring = new(
            "NTHRU BOR", "Non-through hole boring unit",
            [Field.Uno, Field.Dia, Field.Depth, Field.Chmf, Field.Btm, Field.Wal, Field.PreDia]);

        public static readonly MazatrolUnitKind SteppedThruBoring = new(
            "STP THRU", "Stepped through hole boring unit",
            [Field.Uno, Field.CbDia, Field.CbDep, Field.Chmf, Field.Btm, Field.Wal, Field.Dia, Field.Depth,
                Field.Chmf, Field.Wal]);

        public static readonly MazatrolUnitKind SteppedNonThruBoring = new(
            "STP NTHRU", "Stepped non-through hole boring unit",
            [Field.Uno, Field.CbDia, Field.CbDep, Field.Chmf, Field.Btm, Field.Wal, Field.PreDia, Field.Dia,
                Field.Depth, Field.Chmf, Field.Btm, Field.Wal]);

        public static readonly MazatrolUnitKind CentralLinear = new(
            "LINE CTR", "Central linear machining unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.SrvR, Field.Rgh, Field.FinZ, Field.Start, Field.End],
            packedMarker: 0x40);

        public static readonly MazatrolUnitKind RightHandLinear = new(
            "LINE RGH", "Right-hand linear machining unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.SrvR, Field.Rgh, Field.FinZ, Field.FinR, Field.Start, Field.End,
                Field.InterR, Field.RChamferFlag, Field.Chmf]);

        public static readonly MazatrolUnitKind OutsideLinear = new(
            "LINE OS", "Outside linear machining unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.SrvR, Field.Rgh, Field.FinZ, Field.FinR, Field.InterR,
                Field.RChamferFlag, Field.Chmf]);

        public static readonly MazatrolUnitKind InsideLinear = new(
            "LINE IS", "Inside linear machining unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.SrvR, Field.Rgh, Field.FinZ, Field.FinR, Field.InterR,
                Field.RChamferFlag, Field.Chmf]);

        public static readonly MazatrolUnitKind RightHandChamfering = new(
            "CHMF RGH", "Right-hand chamfering unit",
            [Field.Uno, Field.Depth, Field.InterZ, Field.InterR, Field.Chmf, Field.Start, Field.End, Field.RChamferFlag]);

        public static readonly MazatrolUnitKind LeftHandChamfering = new(
            "CHMF L", "Left-hand chamfering unit",
            [Field.Uno, Field.Depth, Field.InterZ, Field.InterR, Field.Chmf, Field.Start, Field.End, Field.RChamferFlag]);

        public static readonly MazatrolUnitKind OutsideChamfering = new(
            "CHMF OS", "Outside chamfering unit",
            [Field.Uno, Field.Depth, Field.InterZ, Field.InterR, Field.Chmf, Field.RChamferFlag]);

        public static readonly MazatrolUnitKind InsideChamfering = new(
            "CHMF IS", "Inside chamfering unit",
            [Field.Uno, Field.Depth, Field.InterZ, Field.InterR, Field.Chmf, Field.RChamferFlag]);

        public static readonly MazatrolUnitKind FaceMilling = new(
            "FACE", "Face milling unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.Btm, Field.FinZ]);

        public static readonly MazatrolUnitKind EndMillingTop = new(
            "TOP EMILL", "End milling-top unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.Btm, Field.FinZ]);

        public static readonly MazatrolUnitKind EndMillingStep = new(
            "STEP EMILL", "End milling-step unit",
            [Field.Uno, Field.Depth, Field.SrvZ, Field.Btm, Field.Wal, Field.FinZ, Field.FinR]);

        public static readonly IReadOnlyList<MazatrolUnitKind> ProgramUnits =
        [
            Setup, Ofs, Index, Process, SubPro, Wpc, Offset, Tool, PointTool, Figure, PointFigure,
            Manual, ManualPath, End
        ];

        public static readonly IReadOnlyList<MazatrolUnitKind> MachiningUnits =
        [
            Drilling, RghCbor, RghBcb, Reaming, Tapping, BackBoring, CircularMilling, CounterboreTapping,
            ThruBoring, NonThruBoring, SteppedThruBoring, SteppedNonThruBoring,
            CentralLinear, RightHandLinear, OutsideLinear, InsideLinear,
            RightHandChamfering, LeftHandChamfering, OutsideChamfering, InsideChamfering,
            FaceMilling, EndMillingTop, EndMillingStep
        ];

        public static readonly IReadOnlyList<MazatrolUnitKind> AllUnits =
            ProgramUnits.Concat(MachiningUnits).ToArray();
    }
}
