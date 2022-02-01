using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.Base
{
    public abstract class StrategyBase : IStrategy
    {
        protected short Depth;
        protected MoveSorter Sorter;
        protected IPosition Position;
        protected int SearchValue;
        protected int ThreefoldRepetitionValue;

        protected IEvaluationService EvaluationService;
        protected readonly IMoveHistoryService MoveHistory;
        protected readonly IMoveProvider MoveProvider;

        protected StrategyBase(short depth, IPosition position)
        {
            SearchValue = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .Evaluation.Static.Mate;
            ThreefoldRepetitionValue = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .Evaluation.Static.ThreefoldRepetitionValue;
            Depth = depth;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            MoveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
        }

        public abstract IResult GetResult();

        public abstract IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null);

        public virtual int Size => 0;

        public abstract int Search(int alpha, int beta, int depth);

        protected int Evaluate(int alpha, int beta)
        {
            int standPat = Position.GetValue();
            if (standPat >= beta)
            {
                return beta;
            }

            if (alpha < standPat)
                alpha = standPat;

            var moves = Position.GetAllAttacks(Sorter);
            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                int score = -Evaluate(-beta, -alpha);

                Position.UnMake();

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        public override string ToString()
        {
            return $"{GetType().Name}";
        }
    }
}
