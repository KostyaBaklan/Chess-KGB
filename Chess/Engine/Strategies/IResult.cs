using Engine.Interfaces;

namespace Engine.Strategies
{
    class Result : IResult
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

    public interface IResult
    {
        int Value { get; }
        IMove Move { get; }
        IMove Cut { get; }
    }
}