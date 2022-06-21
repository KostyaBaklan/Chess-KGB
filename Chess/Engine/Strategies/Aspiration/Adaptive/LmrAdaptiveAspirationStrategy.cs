using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.Aspiration.Adaptive
{
    public class LmrAdaptiveAspirationStrategy: AdaptiveAspirationStrategyBase
    {
        public LmrAdaptiveAspirationStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        #region Overrides of AdaptiveAspirationStrategyBase

        protected override void InitializeModels(TranspositionTable table)
        {
            foreach (var aspirationModel in Models)
            {
                aspirationModel.Strategy = new LmrDeepExtendedStrategy((short) aspirationModel.Depth,Position,table);
            }
        }

        #endregion
    }
}