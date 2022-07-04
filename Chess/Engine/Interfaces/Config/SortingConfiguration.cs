namespace Engine.Interfaces.Config
{
    public class SortingConfiguration
    {
        public bool UseSortHard { get; set; }
        public int[] SortDepth { get; set; }
        public int[] SortHardDepth { get; set; }
    }
}