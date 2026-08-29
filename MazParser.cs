using System.Globalization;
using System.IO;
using System.Text;

namespace MazEdit
{
    /// <summary>
    /// Reads Mazatrol Nexus 2 / Matrix sub-program (.maz) binary files.
    /// Layout confirmed against TEST.MAZ and its PAD listing (MG3-252).
    /// </summary>
    public class MazParser
    {
        internal const int ProgramNoOffset = 0x08;
        internal const int InitialZOffset = 0x28;
        internal const int MaterialOffset = 0x54;
        internal const int MaterialLength = 12;
        internal const int UnitBlockStart = 0x64;
        internal const int UnitBlockSize = 100;
        internal const int CoordinateScale = 10000;

        public MazProgram ParseSubProgram(string filePath)
        {
            var program = new MazProgram();

            if (!File.Exists(filePath))
                return program;

            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length < MaterialOffset + MaterialLength)
                return program;

            program.ProgramNo = BitConverter.ToInt32(data, ProgramNoOffset);
            program.InitialZ = Coord(data, 0, InitialZOffset);
            program.Material = Encoding.ASCII.GetString(data, MaterialOffset, MaterialLength).TrimEnd('\0');

            var setup = new MazUnit
            {
                UnitNo = 0,
                SequenceNo = 0,
                TypeName = "SETUP",
                FileOffset = 0,
                Summary = Format("MAT={0}  INITIAL-Z={1}", program.Material, Num(program.InitialZ)),
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
                    unit.TypeName = "OFS";
                    unit.Summary = Format("X={0}  Y={1}  th={2}  Z={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;

                case 0x0C:
                    unit.TypeName = "INDEX";
                    unit.Parameter = unit.Y_Coord;
                    unit.Y_Coord = 0;
                    unit.Summary = Format("TURN X={0}  Y={1}  Z={2}  ANGLE={3}  DIR={4}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter),
                        DecodeTurnDir(data[i + 8]));
                    break;

                case 0x02:
                    int wpcNo = BitConverter.ToInt32(data, i + 8);
                    unit.TypeName = $"WPC-{wpcNo}";
                    unit.Summary = Format("X={0}  Y={1}  th={2}  Z={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;

                case 0x03:
                    unit.TypeName = "OFFSET";
                    unit.Summary = Format("U={0}  V={1}  D={2}  W={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;

                case 0x40:
                    int rgh = data[i + 17];
                    unit.TypeName = "LINE CTR";
                    unit.Summary = Format("DEPTH={0}  SRV-Z={1}  SRV-R={2}  RGH={3}  FIN-Z={4}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), rgh, Num(unit.Parameter));
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
                    unit.TypeName = "TOOL";
                    unit.Summary = Format("{0}  Φ={1}  {2}  No={3}{4}  ZFD={5}  DEP-Z={6}  C-SP={7}  FR={8}  {9}",
                        DecodeToolType(toolType), Num(diameter), toolLetter, operationNo,
                        FormatApproach(aprchX, aprchY),
                        DecodeZfd(zfd), Num(depZ),
                        csp, Num(fr), FormatMCodes(m1, m2, m3));
                    break;

                case 0xC2:
                    unit.X_Coord = Coord(data, i, 40);
                    unit.Y_Coord = Coord(data, i, 36);
                    unit.Parameter = Coord(data, i, 48);
                    unit.Z_Coord = 0;
                    unit.TypeName = DecodeFigureType(data[i + 8]);
                    unit.Summary = Format("X={0}  Y={1}  R/th={2}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Parameter));
                    break;

                case 0x04:
                    unit.TypeName = "END";
                    int conti = data[i + 9];
                    int number = data[i + 10];
                    int dir = data[i + 8];
                    unit.Summary = Format("CONTI={0}  NUMBER={1}  DIR={2}",
                        conti, number, DecodeTurnDir((byte)dir));
                    break;

                case 0xB2:
                    unit.TypeName = "TOOL DATA";
                    break;
                case 0x66:
                    unit.TypeName = "TOOL PATH";
                    break;
                case 0x20:
                    unit.TypeName = "POSITIONING";
                    break;
                case 0x24:
                    unit.TypeName = "SPEED/FEED";
                    break;
                default:
                    unit.TypeName = $"CODE {marker:X2}";
                    unit.Summary = Format("X={0}  Y={1}  Z={2}  P={3}",
                        Num(unit.X_Coord), Num(unit.Y_Coord), Num(unit.Z_Coord), Num(unit.Parameter));
                    break;
            }

            return unit;
        }

        private static bool IsChildMarker(byte marker) => marker is 0xA0 or 0xB1 or 0xC2;

        private static string DecodeTurnDir(byte code) => code switch
        {
            2 => "NEAR DIR",
            _ => $"DIR {code}"
        };

        private static string DecodeToolLetter(byte index)
        {
            if (index <= 25)
                return ((char)('A' + index)).ToString();
            return $"L{index}";
        }

        private static string DecodeToolType(byte code) => code switch
        {
            15 => "END MILL",
            _ => $"T{code}"
        };

        private static string DecodeZfd(int code) => code switch
        {
            0 => "G00",
            1 => "G01",
            0x40 => "G01",
            _ => $"G{code:D2}"
        };

        private static string DecodeFigureType(byte code) => (code & 0x01) == 0 ? "LINE" : "CW";

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
