using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MazEdit
{
    public class MazParser
    {
        public MazProgram ParseSubProgram(string filePath)
        {
            var program = new MazProgram();
            byte[] data = File.ReadAllBytes(filePath);

            program.ProgramNo = BitConverter.ToInt32(data, 0x08);
            program.Material = Encoding.ASCII.GetString(data, 0x54, 12).TrimEnd('\0');

            MazUnit currentParent = null;

            for (int i = 0x64; i < data.Length - 100; i += 100)
            {
                byte marker = data[i];
                if (marker == 0x00) continue;

                var line = new MazUnit
                {
                    SequenceNo = BitConverter.ToInt16(data, i + 2),
                    FileOffset = i,
                    TypeName = DecodeUnitType(marker),
                    X = BitConverter.ToInt32(data, i + 36) / 10000.0f,
                    Y = BitConverter.ToInt32(data, i + 40) / 10000.0f,
                    Z = BitConverter.ToInt32(data, i + 44) / 10000.0f,
                    Name = Encoding.ASCII.GetString(data, i + 12, 24).TrimEnd('\0').Trim()
                };

                // HIERARCHY LOGIC
                if (marker == 0xA0 || marker == 0x0C || marker == 0xB2 || marker == 0x02)
                {
                    // This is a Main Unit Header
                    currentParent = line;
                    program.Units.Add(line);
                }
                else if (currentParent != null)
                {
                    // This is a sub-line (Tool or Shape) belonging to the parent above
                    currentParent.Children.Add(line);
                }
            }
            return program;
        }

        private string DecodeUnitType(byte code)
        {
            return code switch
            {
                0xA0 => "UNIT HEADER",
                0x0C => "WPC-1",
                0x02 => "OFFSET",
                0xB2 => "SLOT",
                0x66 => "TOOL DATA",
                0xC2 => "SHAPE DATA",
                0x04 => "SUB CALL",
                0x03 => "END UNIT",
                _ => $"CODE {code:X2}"
            };
        }
    }
}