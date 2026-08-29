namespace MazEdit
{
    /// <summary>
    /// Parsed header and unit list from a Mazatrol sub-program file.
    /// </summary>
    public class MazProgram
    {
        public int ProgramNo { get; set; }
        public string Material { get; set; } = string.Empty;
        public float InitialZ { get; set; }
        public int MultiMode { get; set; }
        public List<MazUnit> Units { get; set; } = [];
    }
}
