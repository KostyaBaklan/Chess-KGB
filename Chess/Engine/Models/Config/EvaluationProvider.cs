using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class EvaluationProvider: IEvaluationProvider
    {
        public EvaluationProvider(IStaticEvaluation @static, IPieceEvaluation piece)
        {
            Static = @static;
            Piece = piece;
        }

        #region Implementation of IEvaluationProvider

        public IStaticEvaluation Static { get; }
        public IPieceEvaluation Piece { get; }

        #endregion
    }
}