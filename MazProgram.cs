namespace MazEdit
{
    /// <summary>
    /// Parsed header and unit list from a Mazatrol sub-program file.
    /// </summary>
    public class MazProgram
    {
        /// <summary>EIA/PAD O-number when present; packed .maz files usually omit this.</summary>
        public int? ProgramNumber { get; set; }

        /// <summary>Program name (EIA header, or the .maz file name without extension).</summary>
        public string ProgramName { get; set; } = string.Empty;

        /// <summary>Identifier such as MG3-251 (MATRIX) or MG3-252.</summary>
        public string FormatId { get; set; } = string.Empty;

        /// <summary>Raw Int32 at packed-file offset 0x08 (not the EIA O-number).</summary>
        public int PackedHeader08 { get; set; }

        public string Material { get; set; } = string.Empty;
        public float InitialZ { get; set; }
        public int MultiMode { get; set; }
        public List<MazUnit> Units { get; set; } = [];
    }
}
