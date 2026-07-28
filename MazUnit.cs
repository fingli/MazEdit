namespace MazEdit
{
    /// <summary>
    /// One decoded 100-byte unit block from a .maz sub-program.
    /// </summary>
    public class MazUnit
    {
        public int SequenceNo { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string GCodeLine { get; set; } = string.Empty;
        public float X_Coord { get; set; }
        public float Y_Coord { get; set; }
        public float Z_Coord { get; set; }

        /// <summary>Depth, feed override, or index angle depending on unit type.</summary>
        public float Parameter { get; set; }

        public int FileOffset { get; set; }
    }
}
