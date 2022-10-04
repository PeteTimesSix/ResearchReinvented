using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class AdjacencyHelper
    {
        public static IEnumerable<IntVec3> GenReachableAdjacentCells(Thing thing, Pawn pawn) 
        {
            if(thing.Map != pawn.Map)
                return Enumerable.Empty<IntVec3>();
            var adjCells = GenAdj.CellsAdjacent8Way(thing);
            var reachable = adjCells.Where(c => c.Standable(pawn.Map)).Where(c => pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly));
            return reachable;
        }
    }
}
