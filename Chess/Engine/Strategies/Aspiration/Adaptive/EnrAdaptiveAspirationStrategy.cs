using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.LateMove.Deep.Null;

namespace Engine.Strategies.Aspiration.Adaptive
{
    public class EnrAdaptiveAspirationStrategy : AdaptiveAspirationStrategyBase
    {
        public EnrAdaptiveAspirationStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        #region Overrides of AdaptiveAspirationStrategyBase

        protected override void InitializeModels(TranspositionTable table)
        {
            foreach (var aspirationModel in Models)
            {
                aspirationModel.Strategy = new LmrEnrStrategy((short)aspirationModel.Depth, Position, table);
            }
        }

        #endregion
    }
}