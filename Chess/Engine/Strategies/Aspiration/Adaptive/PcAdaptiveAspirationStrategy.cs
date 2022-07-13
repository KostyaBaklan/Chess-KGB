using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.ProbCut;

namespace Engine.Strategies.Aspiration.Adaptive
{
    public class PcAdaptiveAspirationStrategy : AdaptiveAspirationStrategyBase
    {
        public PcAdaptiveAspirationStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        #region Overrides of AdaptiveAspirationStrategyBase

        protected override void InitializeModels(TranspositionTable table)
        {
            foreach (var aspirationModel in Models)
            {
                aspirationModel.Strategy = new ProbCutLmrDeepStrategy((short)aspirationModel.Depth, Position, table);
            }
        }

        #endregion
    }
}