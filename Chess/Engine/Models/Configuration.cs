using Engine.Interfaces;

namespace Engine.Models
{
    public class Configuration : IConfiguration
    {
        public bool UseEvaluationCache { get; set; }
        public int InitialDepth { get; set; }
    }
}
