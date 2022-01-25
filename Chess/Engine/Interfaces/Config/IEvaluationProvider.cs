namespace Engine.Interfaces.Config
{
    public interface IEvaluationProvider
    {
        IStaticEvaluation Static { get; }
        IPieceEvaluation Piece { get; }
    }
}