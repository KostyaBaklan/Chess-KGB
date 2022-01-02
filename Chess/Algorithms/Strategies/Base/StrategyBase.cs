using Algorithms.Interfaces;
using CommonServiceLocator;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Base
{
    public abstract class StrategyBase : IStrategy
    {
        protected short Depth;
        protected IMoveSorter Sorter;
        protected IPosition Position;
        protected IEvaluationService EvaluationService;

        protected StrategyBase(short depth, IPosition position)
        {
            Depth = depth;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
        }

        protected StrategyBase(short depth, IMoveSorter sorter, IPosition position)
        {
            Depth = depth;
            Sorter = sorter;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
        }

        public abstract IResult GetResult();

        public abstract IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null);

        public virtual int Size => 0;

        protected virtual void OnCutOff(IMove move, int depth)
        {
        }

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
