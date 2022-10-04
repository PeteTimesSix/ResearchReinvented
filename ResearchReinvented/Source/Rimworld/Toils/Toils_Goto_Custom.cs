using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using PeteTimesSix.ResearchReinvented.Utilities;

namespace PeteTimesSix.ResearchReinvented.Rimworld.Toils
{
    public static class Toils_Goto_Custom
    {
		public static Toil GoAdjacentToThing(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate ()
			{
				Pawn actor = toil.actor;
				var reachable = AdjacencyHelper.GenReachableAdjacentCells(actor.jobs.curJob.GetTarget(ind).Thing, actor);
				//var adjCells = GenAdj.CellsAdjacent8Way(actor.jobs.curJob.GetTarget(ind).Thing); 
				//var reachable = adjCells.Where(c => c.Standable(actor.Map)).Where(c => actor.CanReach(c, PathEndMode.OnCell, Danger.Deadly));
				// //var reachable = adjCells.Where(c => c.Standable(actor.Map)).Where(c => actor.Map.reachability.CanReach(actor.Position, c, PathEndMode.OnCell, TraverseMode.PassDoors));
				if(reachable.Any())
                {
					var cell = reachable.InRandomOrder().First(); //path next to, but not always the same spot
					actor.pather.StartPath(cell, PathEndMode.OnCell);
				}
                else 
				{

				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.FailOnDespawnedOrNull(ind);
			return toil;
		}
	}
}
