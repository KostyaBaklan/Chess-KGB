using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IAttackEvaluationService
    {
        void Initialize(BitBoard[] boards, BitBoard occ, Phase phase, int[] pieceCount);
        int StaticExchange(AttackBase attack);
    }
}
