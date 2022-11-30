using Engine.Models.Enums;
using System.Collections.Generic;

namespace Engine.Models.Config
{
    public interface IPieceOrderConfiguration
    {
        Dictionary<Phase, Piece[]> Blacks { get; }
        Dictionary<Phase, Piece[]> BlacksAttacks { get;  }
        Dictionary<Phase, Piece[]> Whites { get;  }
        Dictionary<Phase, Piece[]> WhitesAttacks { get; }
    }
}