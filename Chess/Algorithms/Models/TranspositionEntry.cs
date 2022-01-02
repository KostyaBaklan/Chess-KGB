using Infrastructure.Interfaces.Moves;

namespace Algorithms.Models
{
    public class TranspositionEntry
    {
        public int Value { get; set; }
        public int Depth { get; set; }
        public TranspositionEntryType Type { get; set; }
        public IMove PvMove { get; set; }
        public IMove CutMove { get; set; }
    }
}
