using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove
{
    public abstract class LmrDeepStrategyBase : LmrStrategyBase
    {
        protected int LmrLateDepthThreshold;
        protected MoveSorter InitialSorter;
        protected MoveSorter MainSorter;

        protected LmrDeepStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            LmrLateDepthThreshold = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.LmrLateDepthThreshold;
        }

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove;
            var key = Position.GetKey();
            if (pv == null)
            {
                var isNotEndGame = Position.GetPhase() != Phase.End;
                if (isNotEndGame && Table.TryGet(key, out var entry))
                {
                    if ((entry.Depth - depth) % 2 == 0)
                    {
                        pv = MoveProvider.Get(entry.PvMove);
                    }
                }
            }

            Sorter = MoveHistory.GetPly() > 4 ? MainSorter : InitialSorter;

            var moves = Position.GetAllMoves(Sorter, pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Count > 1)
            {
                var isCheck = MoveHistory.GetLastMove().IsCheck();
                for (var i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int value;
                    if (alpha > -SearchValue && IsLmr(i) && CanReduce(move)&&!isCheck)
                    {
                        var reduction = i > LmrLateDepthThreshold ? DepthReduction + 1 : DepthReduction;
                        value = -Search(-beta, -alpha, depth - reduction);
                        if (value > alpha)
                        {
                            value = -Search(-beta, -alpha, depth - 1);
                        }
                    }
                    else
                    {
                        value = -Search(-beta, -alpha, depth - 1);
                    }

                    Position.UnMake();
                    if (value > result.Value)
                    {
                        result.Value = value;
                        result.Move = move;
                    }

                    if (value > alpha)
                    {
                        alpha = value;
                    }

                    if (alpha < beta) continue;
                    break;
                }
            }
            else
            {
                result.Move = moves[0];
            }

            result.Move.History++;
            return result;
        }
    }
}