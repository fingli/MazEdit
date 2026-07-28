using System.IO;
using System.Text;

namespace MazEdit
{
    /// <summary>
    /// Reads Mazatrol Nexus 2 / Matrix sub-program (.maz) binary files.
    /// Offsets were determined by reverse-engineering; treat unknown markers as experimental.
    /// </summary>
    public class MazParser
    {
        private const int ProgramNoOffset = 0x08;
        private const int MaterialOffset = 0x54;
        private const int MaterialLength = 12;
        private const int UnitBlockStart = 0x64;
        private const int UnitBlockSize = 100;
        private const int CoordinateScale = 10000;

        public MazProgram ParseSubProgram(string filePath)
        {
            var program = new MazProgram();

            if (!File.Exists(filePath))
                return program;

            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length < MaterialOffset + MaterialLength)
                return program;

            program.ProgramNo = BitConverter.ToInt32(data, ProgramNoOffset);
            program.Material = Encoding.ASCII.GetString(data, MaterialOffset, MaterialLength).TrimEnd('\0');

            for (int i = UnitBlockStart; i <= data.Length - UnitBlockSize; i += UnitBlockSize)
            {
                byte marker = data[i];
                if (marker == 0x00)
                    continue;

                var unit = new MazUnit
                {
                    SequenceNo = BitConverter.ToInt16(data, i + 2),
                    TypeName = DecodeUnitType(marker),
                    FileOffset = i,
                    X_Coord = BitConverter.ToInt32(data, i + 36) / (float)CoordinateScale,
                    Y_Coord = BitConverter.ToInt32(data, i + 40) / (float)CoordinateScale,
                    Z_Coord = BitConverter.ToInt32(data, i + 44) / (float)CoordinateScale,
                    Parameter = BitConverter.ToInt32(data, i + 48) / (float)CoordinateScale
                };

                // Unit headers and sub-program calls store a name at offset +12 (e.g. P2_M120).
                if (marker is 0xA0 or 0x04)
                {
                    unit.GCodeLine = Encoding.ASCII.GetString(data, i + 12, 24).TrimEnd('\0').Trim();
                }

                program.Units.Add(unit);
            }

            return program;
        }

        private static string DecodeUnitType(byte code) => code switch
        {
            0xA0 => "UNIT HEADER",
            0x04 => "SUB CALL",
            0x0C => "WPC / COORD SHIFT",
            0x03 => "END UNIT",
            0x02 => "SHAPE / LINE",
            0xB2 => "TOOL DATA",
            0x66 => "TOOL PATH",
            0xC2 => "COORDINATE",
            0x20 => "POSITIONING",
            0x24 => "SPEED/FEED",
            _ => $"CODE {code:X2}"
        };
    }
}
