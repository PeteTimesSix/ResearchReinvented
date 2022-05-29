using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

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
				var adjCells = GenAdj.CellsAdjacent8Way(actor.jobs.curJob.GetTarget(ind).Thing).InRandomOrder(); //
				var reachable = adjCells.Where(c => c.Standable(actor.Map)).Where(c => actor.Map.reachability.CanReach(actor.Position, c, PathEndMode.OnCell, TraverseMode.PassDoors));
				if(reachable.Any())
                {
					var cell = reachable.First();
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

	public static class Toils_Haul_Custom
	{
		public static Toil FindStorageForThing(TargetIndex thingIndex, TargetIndex destinationCellIndex)
		{
			Toil toil = new Toil();
			toil.initAction = delegate ()
			{
				Pawn actor = toil.actor;
				Job job = actor.jobs.curJob;
				Thing thing = actor.CurJob.GetTarget(thingIndex).Thing;
				StoreUtility.TryFindBestBetterStoreCellFor(thing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out IntVec3 foundCell, true);
				if (foundCell.IsValid)
				{
					//actor.carryTracker.TryStartCarry(thing);
					job.SetTarget(destinationCellIndex, new LocalTargetInfo(foundCell));
					job.SetTarget(thingIndex, new LocalTargetInfo(thing));
					job.count = 99999;
					return;
				}
				else if (!GenPlace.TryPlaceThing(thing, actor.PositionHeld, actor.Map, ThingPlaceMode.Near, null, null, default(Rot4)))
				{
					Log.Error($"Store toil of {job} could not fallback drop {thing} near {actor.PositionHeld}");
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.FailOnDespawnedOrNull(thingIndex);
			return toil;
		}
	}


	
}
