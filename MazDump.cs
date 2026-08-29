using System.IO;
using System.Text;

namespace MazEdit
{
    /// <summary>
    /// Writes a text report from a .maz file for sharing or offline analysis.
    /// </summary>
    public static class MazDump
    {
        public static string BuildReport(string filePath, MazProgram program)
        {
            byte[] data = File.ReadAllBytes(filePath);
            var sb = new StringBuilder();

            sb.AppendLine($"File: {Path.GetFileName(filePath)}");
            sb.AppendLine($"Size: {data.Length} bytes");
            sb.AppendLine($"Material: {program.Material}");
            sb.AppendLine($"Initial-Z: {program.InitialZ}");
            sb.AppendLine($"Header+0x08: {program.ProgramNo}");
            sb.AppendLine();
            sb.AppendLine("=== Program (PAD-style) ===");

            foreach (var unit in program.Units)
            {
                string indent = unit.IsChild ? "  " : "";
                string line = $"{indent}U{unit.UnitNo}  {unit.TypeName}";
                if (unit.IsChild)
                    line = $"{indent}S{unit.SequenceNo}  {unit.TypeName}";
                if (!string.IsNullOrEmpty(unit.Summary))
                    line += "  " + unit.Summary;
                sb.AppendLine(line);
            }

            sb.AppendLine();
            sb.AppendLine("=== Header (first 0x100 bytes) ===");
            AppendHexBlock(sb, data, 0, Math.Min(data.Length, 0x100));
            sb.AppendLine();
            sb.AppendLine("=== Raw blocks ===");

            foreach (var unit in program.Units)
            {
                if (unit.FileOffset < MazParser.UnitBlockStart)
                    continue;

                sb.AppendLine();
                sb.AppendLine($"--- Offset 0x{unit.FileOffset:X4} | {unit.TypeName} | SEQ {unit.SequenceNo} ---");
                if (!string.IsNullOrEmpty(unit.Summary))
                    sb.AppendLine($"  {unit.Summary}");

                if (unit.FileOffset + MazParser.UnitBlockSize <= data.Length)
                {
                    sb.AppendLine("  Raw block:");
                    AppendHexBlock(sb, data, unit.FileOffset, MazParser.UnitBlockSize, "    ");
                }
            }

            return sb.ToString();
        }

        public static void WriteReport(string mazFilePath, MazProgram program, string outputPath)
        {
            File.WriteAllText(outputPath, BuildReport(mazFilePath, program), Encoding.UTF8);
        }

        private static void AppendHexBlock(StringBuilder sb, byte[] data, int offset, int length, string indent = "")
        {
            int end = Math.Min(offset + length, data.Length);
            for (int i = offset; i < end; i += 16)
            {
                int count = Math.Min(16, end - i);
                sb.Append(indent);
                sb.Append($"0x{i:X4}  ");
                sb.Append(HexSlice(data, i, count));
                sb.Append(' ', (16 - count) * 3);
                sb.Append("  ");
                sb.Append(AsciiSlice(data, i, count).PadRight(16));
                sb.AppendLine();
            }
        }

        private static string HexSlice(byte[] data, int offset, int count)
        {
            var chars = new char[count * 3 - 1];
            int pos = 0;
            for (int i = 0; i < count; i++)
            {
                int index = offset + i;
                if (index >= data.Length)
                    break;

                if (i > 0)
                    chars[pos++] = ' ';

                chars[pos++] = GetHexNibble(data[index] >> 4);
                chars[pos++] = GetHexNibble(data[index] & 0xF);
            }

            return new string(chars, 0, pos);
        }

        private static string AsciiSlice(byte[] data, int offset, int count)
        {
            var chars = new char[count];
            for (int i = 0; i < count; i++)
            {
                int index = offset + i;
                if (index >= data.Length)
                {
                    chars[i] = ' ';
                    continue;
                }

                byte b = data[index];
                chars[i] = b is >= 32 and <= 126 ? (char)b : '.';
            }

            return new string(chars);
        }

        private static char GetHexNibble(int value) => (char)(value < 10 ? '0' + value : 'A' + value - 10);
    }
}
