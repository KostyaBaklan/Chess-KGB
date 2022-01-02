using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Sorting.Sorters;

namespace Engine.Strategies
{
    public abstract class StrategyBase : IStrategy
    {
        protected short Depth;
        protected MoveSorter Sorter;
        protected IPosition Position;
        protected IEvaluationService EvaluationService;

        protected StrategyBase(short depth, IPosition position)
        {
            Depth = depth;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
        }

        public abstract IResult GetResult();

        public abstract IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null);

        public virtual int Size => 0;

        public abstract int Search(int alpha, int beta, int depth);

        protected int Evaluate(int alpha, int beta)
        {
            int standPat = EvaluationService.Evaluate(Position);
            if (standPat >= beta)
            {
                return beta;
            }

            if (alpha < standPat)
                alpha = standPat;

            foreach (var move in Position.GetAllAttacks())
            {
                try
                {
                    Position.Make(move);

                    var isCheck = Position.IsCheck();

                    if (isCheck) continue;

                    int score = -Evaluate(-beta, -alpha);

                    if (score >= beta)
                    {
                        return beta;
                    }

                    if (score > alpha)
                        alpha = score;
                }
                finally
                {
                    Position.UnMake();
                }
            }

            return alpha;
        }

        public override string ToString()
        {
            return $"{GetType().Name}";
        }
    }
}
