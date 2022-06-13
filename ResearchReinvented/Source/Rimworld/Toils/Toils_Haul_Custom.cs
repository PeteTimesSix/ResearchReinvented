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
					job.SetTarget(destinationCellIndex, new LocalTargetInfo(foundCell));
					job.SetTarget(thingIndex, new LocalTargetInfo(thing));
					job.count = 99999;
					return;
				}
				else
				{
					//no valid stockpile etc.

					//actually just leave it where it is
					/*var fallbackPlace = GenPlace.TryPlaceThing(thing, actor.PositionHeld, actor.Map, ThingPlaceMode.Near);
					if (!fallbackPlace)
					{
						Log.Error($"Store toil of {job} could not fallback drop {thing} near {actor.PositionHeld}");
					}
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);*/
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.FailOnDespawnedOrNull(thingIndex);
			return toil;
		}
	}
}
