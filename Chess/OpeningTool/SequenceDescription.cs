using System.Collections.Generic;

namespace OpeningTool
{
    class SequenceDescription
    {
        public SequenceDescription()
        {
            Attacks = new List<string>();
            Moves = new List<string>();
        }

        public string Key { get; set; }
        public List<string> Attacks { get; set; }
        public List<string> Moves { get; set; }
    }
}