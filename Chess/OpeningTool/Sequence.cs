using System.Collections.Generic;

namespace OpeningTool
{
    class Sequence
    {
        public Sequence()
        {
            Attacks = new List<short>();
            Moves = new List<short>();
        }

        public string Key { get; set; }
        public List<short> Attacks { get; set; }
        public List<short> Moves { get; set; }
    }
}