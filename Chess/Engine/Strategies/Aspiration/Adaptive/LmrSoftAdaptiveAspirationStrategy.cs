using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.LateMove.Base;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.Aspiration.Adaptive
{
    public class LmrSoftAdaptiveAspirationStrategy : AdaptiveAspirationStrategyBase
    {
        public LmrSoftAdaptiveAspirationStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        #region Overrides of AdaptiveAspirationStrategyBase

        protected override void InitializeModels(TranspositionTable table)
        {
            Models[0].Strategy = new LmrExtendedHistoryStrategy((short)Models[0].Depth, Position, table);
            for (var index = 1; index < Models.Count; index++)
            {
                var aspirationModel = Models[index];
                aspirationModel.Strategy = new LmrDeepExtendedStrategy((short) aspirationModel.Depth, Position, table);
            }
        }

        #endregion
    }
}