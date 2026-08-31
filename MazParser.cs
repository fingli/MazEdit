using System.Globalization;
using System.IO;
using System.Text;

namespace MazEdit
{
    /// <summary>
    /// Reads packed Mazatrol .maz files, or EIA/PAD headers (O-number, MG3-xxx, program name).
    /// </summary>
    public class MazParser
    {
        internal const int AtcModeOffset = 0x08;
        internal const int MultiModeOffset = 0x09;
        internal const int InitialZOffset = 0x28;
        internal const int MaterialOffset = 0x54;
        internal const int MaterialLength = 12;
        internal const int UnitNameOffset = 36;
        internal const int UnitNameLength = 16;
        internal const int UnitBlockStart = 0x64;
        internal const int UnitBlockSize = 100;
        internal const int CoordinateScale = 10000;

        public MazProgram ParseSubProgram(string filePath)
        {
            var program = new MazProgram();

            if (!File.Exists(filePath))
                return program;

            byte[] data = File.ReadAllBytes(filePath);
            program.ProgramName = Path.GetFileNameWithoutExtension(filePath);

            if (MazEiaHeader.LooksLikeEia(data) && MazEiaHeader.TryParse(data, out MazEiaHeader eia))
            {
                program.ProgramNumber = eia.ProgramNumber;
                program.FormatId = eia.FormatId;
                if (!string.IsNullOrEmpty(eia.ProgramName))
                    program.ProgramName = eia.ProgramName;

                program.Units.Add(new MazUnit
                {
                    UnitNo = 0,
                    TypeName = MazatrolCatalog.Setup.TypeName,
                    Summary = FormatEiaSetup(program)
                });
                return program;
            }

            if (data.Length < MaterialOffset + MaterialLength)
                return program;

            program.AtcMode = data[AtcModeOffset];
            program.PackedHeader08 = BitConverter.ToInt32(data, AtcModeOffset);
            program.MultiMode = data[MultiModeOffset];
            program.InitialZ = Coord(data, 0, InitialZOffset);
            program.Material = Encoding.ASCII.GetString(data, MaterialOffset, MaterialLength).TrimEnd('\0');

            var setup = new MazUnit
            {
                UnitNo = 0,
                SequenceNo = 0,
                TypeName = MazatrolCatalog.Setup.TypeName,
                FileOffset = 0,
                Summary = Format("NAME={0}  MAT={1}  INITIAL-Z={2}  ATC MODE={3}  MULTI MODE={4}",
                    program.ProgramName, program.Material, Num(program.InitialZ),
                    program.AtcMode, MazatrolCatalog.MultiMode(program.MultiMode)),
                Z_Coord = program.InitialZ
            };
            program.Units.Add(setup);

            MazUnit parent = setup;

            for (int i = UnitBlockStart; i <= data.Length - UnitBlockSize; i += UnitBlockSize)
            {
                byte marker = data[i];
                if (marker == 0x00)
                    continue;

                var unit = DecodeBlock(data, i, marker);

                if (IsChildMarker(marker))
                {
                    unit.IsChild = true;
                    unit.UnitNo = parent.UnitNo;
                    program.Units.Add(unit);
                    continue;
                }

                parent = unit;
                program.Units.Add(unit);
            }

            return program;
        }

        private static MazUnit DecodeBlock(byte[] data, int i, byte marker)
        {
            short seq = BitConverter.ToInt16(data, i + 2);
            var unit = new MazUnit
            {
                Marker = marker,
                SequenceNo = seq,
                UnitNo = seq,
                FileOffset = i,
                X_Coord = Coord(data, i, 36),
                Y_Coord = Coord(data, i, 40),
                Z_Coord = Coord(data, i, 44),
                Parameter = Coord(data, i, 48)
            };

            switch (marker)
            {
                case 0xA0:
                    unit.TypeName = MazatrolCatalog.Ofs.TypeName;
                    unit.Summary = Format("X={0}  Y={1}  th={2}  Z={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;

                case 0x0C:
                    unit.TypeName = MazatrolCatalog.Index.TypeName;
                    unit.Parameter = unit.Y_Coord;
                    unit.Y_Coord = 0;
                    unit.Summary = Format("TURN X={0}  Y={1}  Z={2}  ANGLE={3}  DIR={4}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter),
                        MazatrolCatalog.TurnDir(data[i + 8]));
                    break;

                case 0x0A:
                    int processNo = BitConverter.ToInt16(data, i + 4);
                    unit.TypeName = MazatrolCatalog.Process.TypeName;
                    unit.Summary = Format("P={0}", processNo);
                    break;

                case 0x05:
                    int subL = BitConverter.ToInt32(data, i + 20);
                    string subName = AsciiZ(data, i + UnitNameOffset, UnitNameLength);
                    unit.TypeName = MazatrolCatalog.SubPro.TypeName;
                    unit.Summary = Format("NAME={0}  L={1}", subName, subL);
                    break;

                case 0x02:
                    int wpcNo = BitConverter.ToInt32(data, i + 8);
                    unit.TypeName = $"WPC-{wpcNo}";
                    unit.Summary = Format("X={0}  Y={1}  th={2}  Z={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;

                case 0x03:
                    unit.TypeName = MazatrolCatalog.Offset.TypeName;
                    unit.Summary = Format("U(X)={0}  V(Y)={1}  D(th)={2}  W(Z)={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;

                case 0x40:
                    int rgh = data[i + 17];
                    unit.TypeName = MazatrolCatalog.CentralLinear.TypeName;
                    unit.Summary = Format("DEPTH={0}  SRV-Z={1}  SRV-R={2}  RGH={3}  FIN-Z={4}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), rgh, Num(unit.Parameter));
                    break;

                case 0x20:
                    unit.TypeName = MazatrolCatalog.Drilling.TypeName;
                    unit.Summary = Format("DIA={0}  DEPTH={1}  CHMF={2}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord));
                    break;

                case 0x24:
                    int nom = BitConverter.ToInt16(data, i + 36);
                    float major = Coord(data, i, 40);
                    float pitch = Coord(data, i, 44);
                    float tapDep = Coord(data, i, 48);
                    float tapChmf = Coord(data, i, 52);
                    unit.TypeName = MazatrolCatalog.Tapping.TypeName;
                    unit.Summary = Format("NOM={0}  MAJOR-φ={1}  PITCH={2}  TAP-DEP={3}  CHMF={4}",
                        MazatrolCatalog.TapNom(nom), Num(major), Num(pitch), Num(tapDep), Num(tapChmf));
                    break;

                case 0x06:
                    byte mnlType = data[i + 9];
                    int mnlS = data[i + 11];
                    int mnlP = BitConverter.ToInt32(data, i + 24);
                    float mnlDia = Coord(data, i, 36);
                    unit.TypeName = MazatrolCatalog.Manual.TypeName;
                    unit.Summary = Format("{0}  Φ={1}  S={2}  P={3}",
                        MazatrolCatalog.ToolType(mnlType), Num(mnlDia), mnlS, mnlP);
                    break;

                case 0xB1:
                    float diameter = Coord(data, i, 36);
                    float aprchX = Coord(data, i, 40);
                    float aprchY = Coord(data, i, 44);
                    float depZ = Coord(data, i, 48);
                    unit.X_Coord = diameter;
                    unit.Y_Coord = aprchX;
                    unit.Z_Coord = aprchY;
                    unit.Parameter = depZ;
                    int zfd = BitConverter.ToInt16(data, i + 20);
                    int operationNo = BitConverter.ToInt16(data, i + 22);
                    int m1 = BitConverter.ToInt16(data, i + 24);
                    int m2 = BitConverter.ToInt16(data, i + 26);
                    int m3 = BitConverter.ToInt16(data, i + 28);
                    int csp = BitConverter.ToInt32(data, i + 60);
                    float fr = Coord(data, i, 64);
                    byte toolType = data[i + 9];
                    string toolLetter = DecodeToolLetter(data[i + 11]);
                    unit.TypeName = MazatrolCatalog.Tool.TypeName;
                    unit.Summary = Format("{0}  Φ={1}  {2}  No={3}{4}  ZFD={5}  DEP-Z={6}  C-SP={7}  FR={8}  {9}",
                        MazatrolCatalog.ToolType(toolType), Num(diameter), toolLetter, operationNo,
                        FormatApproach(aprchX, aprchY),
                        MazatrolCatalog.Zfd(zfd), Num(depZ),
                        csp, Num(fr), FormatMCodes(m1, m2, m3));
                    break;

                case 0xB0:
                    float pDia = Coord(data, i, 36);
                    float pE = Coord(data, i, 40);
                    float pH = Coord(data, i, 44);
                    float pDepZ = Coord(data, i, 56);
                    int pCsp = BitConverter.ToInt32(data, i + 60);
                    float pFr = Coord(data, i, 64);
                    int pNo = BitConverter.ToInt16(data, i + 22);
                    int pM1 = BitConverter.ToInt16(data, i + 24);
                    int pM2 = BitConverter.ToInt16(data, i + 26);
                    int pAh = BitConverter.ToInt32(data, i + 52);
                    unit.TypeName = MazatrolCatalog.PointTool.TypeName;
                    unit.Summary = Format("{0}  Φ={1}  S={2}  No={3}{4}{5}{6}  DEP-Z={7}  C-SP={8}  FR={9}  {10}",
                        MazatrolCatalog.ToolType(data[i + 9]), Num(pDia), data[i + 11], pNo,
                        pE == 0 ? "" : Format("  E={0}", Num(pE)),
                        pH == 0 ? "" : Format("  H={0}", Num(pH)),
                        pAh == 0 ? "" : Format("  &H={0}", pAh),
                        Num(pDepZ), pCsp, Num(pFr), FormatMCodes(pM1, pM2));
                    break;

                case 0xC2:
                    unit.X_Coord = Coord(data, i, 40);
                    unit.Y_Coord = Coord(data, i, 36);
                    unit.Parameter = Coord(data, i, 48);
                    unit.Z_Coord = 0;
                    unit.TypeName = MazatrolCatalog.FigureType(data[i + 8]);
                    unit.Summary = Format("X={0}  Y={1}  R/th={2}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Parameter));
                    break;

                case 0xC0:
                    float pntX = Coord(data, i, 36);
                    float pntY = Coord(data, i, 40);
                    float da = Coord(data, i, 44);
                    float db = Coord(data, i, 48);
                    float ta = Coord(data, i, 52);
                    float tb = Coord(data, i, 56);
                    unit.TypeName = MazatrolCatalog.PointFigure.TypeName;
                    unit.Summary = Format("A={0}  X={1}  Y={2}{3}{4}{5}{6}",
                        data[i + 8], Num(pntX), Num(pntY),
                        da == 0 ? "" : Format("  DA={0}", Num(da)),
                        db == 0 ? "" : Format("  DB={0}", Num(db)),
                        ta == 0 ? "" : Format("  TA={0}", Num(ta)),
                        tb == 0 ? "" : Format("  TB={0}", Num(tb)));
                    break;

                case 0xA1:
                    int pathF = BitConverter.ToInt32(data, i + 60);
                    unit.TypeName = MazatrolCatalog.ManualPath.TypeName;
                    unit.Summary = Format("X={0}  Y={1}  Z={2}{3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord),
                        pathF == 0 ? "" : Format("  F={0}", pathF));
                    break;

                case 0x04:
                    unit.TypeName = MazatrolCatalog.End.TypeName;
                    int endReturn = data[i + 8];
                    int conti = data[i + 9];
                    int number = data[i + 10];
                    int atc = data[i + 11];
                    int workNo = BitConverter.ToInt16(data, i + 16);
                    int execute = data[i + 20];
                    string endName = AsciiZ(data, i + UnitNameOffset, UnitNameLength);
                    unit.Summary = Format("CONTI={0}  NUMBER={1}  ATC={2}  RETURN={3}{4}{5}  EXECUTE={6}",
                        conti, number, atc, MazatrolCatalog.EndReturn(endReturn),
                        workNo == 0 ? "" : Format("  WORK No.={0}", workNo),
                        string.IsNullOrEmpty(endName) ? "" : Format("  NAME={0}", endName),
                        MazatrolCatalog.Execute(execute));
                    break;

                case 0xB2:
                    unit.TypeName = "TOOL DATA";
                    break;
                case 0x66:
                    unit.TypeName = "TOOL PATH";
                    break;
                default:
                    unit.TypeName = $"CODE {marker:X2}";
                    unit.Summary = Format("X={0}  Y={1}  Z={2}  P={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;
            }

            return unit;
        }

        private static bool IsChildMarker(byte marker) =>
            marker is 0xA0 or 0xA1 or 0xB0 or 0xB1 or 0xC0 or 0xC2;

        private static string FormatEiaSetup(MazProgram program)
        {
            string o = program.ProgramNumber is int n
                ? n.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "-";
            string format = string.IsNullOrEmpty(program.FormatId) ? "-" : program.FormatId;
            return Format("NAME={0}  O={1}  FORMAT={2}", program.ProgramName, o, format);
        }

        private static string AsciiZ(byte[] data, int offset, int maxLength)
        {
            int limit = Math.Min(data.Length, offset + maxLength);
            int end = offset;
            while (end < limit && data[end] != 0)
                end++;
            return Encoding.ASCII.GetString(data, offset, end - offset).Trim();
        }

        private static string DecodeToolLetter(byte index)
        {
            if (index <= 25)
                return ((char)('A' + index)).ToString();
            return $"L{index}";
        }

        private static string FormatApproach(float aprchX, float aprchY)
        {
            if (aprchX == 0 && aprchY == 0)
                return string.Empty;

            return Format("  APRCH-X={0}  APRCH-Y={1}", Num(aprchX), Num(aprchY));
        }

        private static string FormatMCodes(params int[] codes)
        {
            var parts = codes.Where(c => c != 0).Select(c => $"M{c:D2}");
            return string.Join(" ", parts);
        }

        private static float Coord(byte[] data, int block, int offset)
            => BitConverter.ToInt32(data, block + offset) / (float)CoordinateScale;

        private static string Num(float value)
            => value.ToString("0.####", CultureInfo.InvariantCulture);

        private static string Format(string format, params object[] args)
            => string.Format(CultureInfo.InvariantCulture, format, args);
    }
}
