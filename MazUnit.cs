using System.Collections.Generic;

namespace MazEdit
{
    public class MazUnit
    {
        public int SequenceNo { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Param { get; set; }
        public int FileOffset { get; set; }

        // This is the key for Hierarchy
        public List<MazUnit> Children { get; set; } = new List<MazUnit>();

        // Helper to show/hide the arrow in the UI
        public bool HasChildren => Children.Count > 0;
    }
}