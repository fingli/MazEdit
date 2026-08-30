using System.Text;
using System.Text.RegularExpressions;

namespace MazEdit
{
    /// <summary>
    /// Program number and name from Mazatrol three-digit G-format (EIA/PAD) I/O.
    /// MATRIX identifier is MG3-251; Nexus samples may use MG3-252.
    /// </summary>
    public sealed class MazEiaHeader
    {
        public int? ProgramNumber { get; init; }
        public string ProgramName { get; init; } = string.Empty;
        public string FormatId { get; init; } = string.Empty;

        private static readonly Regex NamedBlock = new(
            @"O(\d{1,8})\s*\(\s*(MG3-\d+)\s*(?::\s*([^)]*?))?\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex AngleName = new(
            @"<([^>]{1,48})>\s*\(\s*(MG3-\d+)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool LooksLikeEia(byte[] data)
        {
            if (data.Length == 0)
                return false;

            byte b = data[0];
            return b is (byte)'O' or (byte)'<' or (byte)'%' or (byte)'\r' or (byte)'\n';
        }

        public static bool TryParse(byte[] data, out MazEiaHeader header)
            => TryParse(Encoding.ASCII.GetString(data), out header);

        public static bool TryParse(string text, out MazEiaHeader header)
        {
            header = new MazEiaHeader();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            Match angle = AngleName.Match(text);
            if (angle.Success)
            {
                header = new MazEiaHeader
                {
                    ProgramName = TruncateName(angle.Groups[1].Value.Trim()),
                    FormatId = angle.Groups[2].Value.ToUpperInvariant()
                };
                return true;
            }

            Match named = NamedBlock.Match(text);
            if (!named.Success)
                return false;

            header = new MazEiaHeader
            {
                ProgramNumber = int.Parse(named.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                FormatId = named.Groups[2].Value.ToUpperInvariant(),
                ProgramName = TruncateName(named.Groups[3].Success ? named.Groups[3].Value.Trim() : "")
            };
            return true;
        }

        private static string TruncateName(string name)
            => name.Length <= 48 ? name : name[..48];
    }
}
