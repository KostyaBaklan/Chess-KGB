using Engine.Strategies.AlphaBeta;

namespace Engine.Strategies.Aspiration.Adaptive
{
    public class AspirationModel
    {
        public int Window { get; set; }
        public int Depth { get; set; }
        public AlphaBetaStrategy Strategy { get; set; }
    }
}