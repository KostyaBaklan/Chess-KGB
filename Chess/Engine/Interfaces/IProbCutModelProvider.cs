using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Strategies.Models;

namespace Engine.Interfaces
{
    public interface IProbCutModelProvider
    {
        ProbCutModel[] CreateModels(short depth);
    }
}
