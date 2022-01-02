using Algorithms.Interfaces;
using Infrastructure.Interfaces.Moves;

namespace Algorithms.Models
{
    class Result: IResult
    {
        public Result()
        {
            Value = int.MinValue;
        }

        #region Implementation of IResult

        public int Value { get; set; }
        public IMove Move { get; set; }
        public IMove Cut { get; set; }

        #endregion
    }
}
