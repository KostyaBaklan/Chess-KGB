using Engine.DataStructures.Killers;
using Engine.Interfaces;
using Engine.Interfaces.Config;

namespace Engine.Services
{
    public class KillerMoveCollectionFactory : IKillerMoveCollectionFactory
    {
        private readonly int _killerCapacity;
        private readonly int _movesCount;

        public KillerMoveCollectionFactory(IConfigurationProvider configurationProvider, IMoveProvider moveProvider)
        {
            _killerCapacity = configurationProvider.GeneralConfiguration.KillerCapacity;
            _movesCount = moveProvider.MovesCount;
        }

        #region Implementation of IKillerMoveCollectionFactory

        public IKillerMoveCollection Create()
        {
            if (_killerCapacity == 2)
                return new BiKillerMoves(_movesCount);
            return new TiKillerMoves(_movesCount);
        }

        #endregion
    }
}