﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.Aspiration.Adaptive
{
    public abstract class AdaptiveAspirationStrategyBase : StrategyBase
    {
        protected int AspirationDepth;
        protected int AspirationMinDepth;
        protected TranspositionTable Table;

        protected List<AspirationModel> Models;

        protected AdaptiveAspirationStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

            var table = service.Create(depth);
            Table = table;
            Models = new List<AspirationModel>();

            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
            AspirationDepth = configuration.AspirationDepth;
            AspirationMinDepth = configuration.AspirationMinDepth;

            var t = Depth - AspirationDepth * configuration.AspirationIterations[depth];

            var model = new AspirationModel {Window  = SearchValue, Depth = t};
            Models.Add(model);

            for (int d = t+ AspirationDepth; d <= depth; d += AspirationDepth)
            {
                var aspirationModel = new AspirationModel { Window = configuration.AspirationWindow[d], Depth = d };
                Models.Add(aspirationModel);
            }

            InitializeModels(table);
        }

        protected abstract void InitializeModels(TranspositionTable table);


        #region Overrides of StrategyBase

        public override int Size => Table.Count;

        public override IResult GetResult()
        {
            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.GetResult(-SearchValue, SearchValue, Math.Min(Depth + 1, MaxEndGameDepth));
            }

            IResult result = new Result
            {
                GameResult = GameResult.Continue,
                Move = null,
                Value = 0
            };

            foreach (var model in Models)
            {
                var alpha = result.Value - model.Window;
                var beta = result.Value + model.Window;

                result = model.Strategy.GetResult(alpha, beta, model.Depth, result.Move);

                if (result.Value >= beta)
                {
                    result = model.Strategy.GetResult(result.Value, SearchValue, model.Depth, result.Move);
                }
                else if (result.Value <= alpha)
                {
                    result = model.Strategy.GetResult(-SearchValue, result.Value, model.Depth, result.Move);
                }
            }

            return result;
        }

        #endregion

        #region Overrides of StrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            return Models.Last().Strategy.GetResult(alpha, beta, depth, pvMove);
        }

        public override int Search(int alpha, int beta, int depth)
        {
            return Models.Last().Strategy.Search(alpha, beta, depth);
        }

        #endregion
    }
}