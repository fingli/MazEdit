using System.Collections.Generic;

namespace MazEdit
{
    public class MazUnit
    {
        public int SequenceNo { get; set; }
        public string TypeName { get; set; } = "";
        public string Name { get; set; } = ""; // For "P2_M120_B180" etc.

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Param { get; set; }

        public int FileOffset { get; set; }
        public byte Marker { get; set; }

        // THE HIERARCHY: Each unit can have sub-lines
        public List<MazUnit> Children { get; set; } = new List<MazUnit>();
    }
}