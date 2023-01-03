using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;

namespace Engine.Strategies.LMR
{
    public class LateMoveReductionClassicStrategy : LateMoveReductionStrategyBase
    {
        public LateMoveReductionClassicStrategy(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position, table)
        {
        }

        protected override StrategyBase CreateSubSearchStrategy()
        {
            return new LateMoveReductionClassicStrategy((short)(Depth - LmrSubSearchDepth), Position);
        }

        protected override bool[][] InitializeReducableTable()
        {
            var result = new bool[2 * Depth][];
            for(int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = new bool[128];
                for (int move = 0; move < result[depth].Length; move++)
                {
                    result[depth][move] = depth > 3 && move > 3;
                }
            }

            return result;
        }

        protected override byte[][] InitializeReductionTable()
        {
            var result = new byte[2 * Depth][];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = new byte[128];
                for (int move = 0; move < result[depth].Length; move++)
                {
                    if (depth > 3 && move > 3)
                    {
                        result[depth][move] = (byte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move] = (byte)(depth - 1);
                    }
                }
            }

            return result;
        }
    }
}
