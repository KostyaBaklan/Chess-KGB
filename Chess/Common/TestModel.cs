using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using Newtonsoft.Json;

namespace Common
{
    public class TestModel
    {
        public TestModel()
        {
            Moves = new List<MoveModel>();
        }

        public string Strategy { get; set; }
        public int Depth { get; set; }
        public TimeSpan Total { get; set; }
        public TimeSpan Min { get; set; }
        public TimeSpan Max { get; set; }
        public TimeSpan Average { get; set; }
        public TimeSpan Std { get; set; }

        [JsonIgnore]
        public long Table
        {
            get { return Moves.Max(m => m.Table); }
        }

        [JsonIgnore]
        public long Evaluation
        {
            get { return Moves.Max(m => m.Evaluation); }
        }

        [JsonIgnore]
        public long Memory
        {
            get { return Moves.Max(m => m.Memory); }
        }

        public List<MoveModel> Moves { get; set; }

        public void Calculate()
        {
            var movesCount = Moves.Count;
            List<TimeSpan> times = new List<TimeSpan>(movesCount);
            TimeSpan total = new TimeSpan();
            foreach (var moveModel in Moves)
            {
                total += moveModel.Time;
                times.Add(moveModel.Time);
            }

            var doubles = times.Select(t => t.TotalMilliseconds).ToArray();
            Min = TimeSpan.FromMilliseconds(ArrayStatistics.Minimum(doubles));
            Max = TimeSpan.FromMilliseconds(ArrayStatistics.Maximum(doubles));
            var (m, s) = ArrayStatistics.MeanStandardDeviation(doubles);
            Average = TimeSpan.FromMilliseconds(m);
            Std = TimeSpan.FromMilliseconds(s);

            Total = total;
        }
    }
}