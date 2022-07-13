using System;
using CommonServiceLocator;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Strategies.AlphaBeta;
using Engine.Strategies.Models;

namespace Engine.Strategies.ProbCut
{
    public sealed class ProbCutStrategy : AlphaBetaStrategy
    {
        private readonly ProbCutModel[] _models;

        public ProbCutStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            var probCutModelProvider = ServiceLocator.Current.GetInstance<IProbCutModelProvider>();
            _models = probCutModelProvider.CreateModels(depth);

            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }

        public override int Search(int alpha, int beta, int depth)
        {
            var probCutModel = _models[depth];
            if (probCutModel.CanReduce && Math.Abs(alpha - beta) > 1)
            {
                double percentile = probCutModel.Percentile;
                double sigma = probCutModel.Sigma;
                double b = probCutModel.B;
                double a = probCutModel.A;
                var d = probCutModel.Depth;

                int betaBound = (int) (Math.Round(percentile * sigma + beta - b) / a);
                var x = base.Search(betaBound - 1, betaBound, d);
                if (x >= betaBound)
                {
                    return beta;
                }

                //int alphaBound = (int) (Math.Round(-percentile * sigma + alpha - b) / a);
                //var z = base.Search(alphaBound, alphaBound + 1, d);
                //if (z <= alphaBound)
                //{
                //    return alpha;
                //}
            }

            return base.Search(alpha, beta, depth);
        }
    }
}
