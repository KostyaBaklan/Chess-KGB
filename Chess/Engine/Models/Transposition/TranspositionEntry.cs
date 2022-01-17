using Engine.Interfaces;

namespace Engine.Models.Transposition
{
    public class TranspositionEntry
    {
        public short Value { get; set; }
        public byte Depth { get; set; }
        public TranspositionEntryType Type { get; set; }
        public IMove PvMove { get; set; }
    }
}
