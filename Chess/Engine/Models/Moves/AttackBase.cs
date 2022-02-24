using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public abstract class AttackBase : MoveBase
    {
        public Piece Captured;

        protected AttackBase()
        {
            IsAttack = true;
        }
    }
}