namespace MazEdit
{
    /// <summary>
    /// One decoded 100-byte unit block, or a synthetic program-header row.
    /// </summary>
    public class MazUnit
    {
        public int UnitNo { get; set; }
        public int SequenceNo { get; set; }
        public byte Marker { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public float X_Coord { get; set; }
        public float Y_Coord { get; set; }
        public float Z_Coord { get; set; }
        public float Parameter { get; set; }
        public int FileOffset { get; set; }
        public bool IsChild { get; set; }
    }
}
