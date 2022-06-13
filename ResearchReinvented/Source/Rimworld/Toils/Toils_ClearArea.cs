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
	public static class Toils_ClearCell
	{
		public static Toil ClearDefaultIngredientPlaceCell(TargetIndex buildingIndex)
		{
			Toil toil = new Toil();
			toil.initAction = delegate ()
			{
				//Log.Message("clearing cells");
				Pawn actor = toil.actor;
				var underlyingBuilding = actor.jobs.curJob.GetTarget(buildingIndex).Thing;
				var ingredientPlaceCells = IngredientPlaceCellsInOrder(underlyingBuilding);
				if (!ingredientPlaceCells.Any())
					return;
				var cell = ingredientPlaceCells.First();
				IEnumerable<Thing> obstructions = cell.GetThingList(actor.MapHeld).Where(t => t != underlyingBuilding);
				if (obstructions.Any())
				{
					//Log.Message("obstructions: " + String.Join(", ", obstructions.Select(o => o.LabelCap)));

					var cellsToPlaceIn = IngredientPlaceCellsInOrder(underlyingBuilding).Skip(1);
					var enumerator = cellsToPlaceIn.GetEnumerator();

					foreach (var obstruction in obstructions.ToList())
					{
						if (!obstruction.Spawned)
							continue;
						if (!obstruction.def.EverHaulable)
							continue;

                        while (enumerator.MoveNext()) 
						{
							var candidate = enumerator.Current;
							if (IsEmptyEnoughSpot(obstruction, underlyingBuilding.MapHeld, candidate))
							{
								var thingsAlreadyAt = candidate.GetThingList(actor.MapHeld).Where(t => t.def.EverHaulable).ToList();
                                if (thingsAlreadyAt.Any())
								{
									var fullyAbsorbed = false;
									foreach (var thing in thingsAlreadyAt)
									{
										fullyAbsorbed = thing.TryAbsorbStack(obstruction, true);
										if (fullyAbsorbed)
											break;
									}
									if (fullyAbsorbed)
									{
										//Log.Message($"{obstruction} absorbed into stack(s) at {candidate}");
										break;
									}
								}
                                else
								{
									//Log.Message($"relocating {obstruction} to {candidate}");
									obstruction.Position = candidate;
									break;
								}
							}
						}
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		private static IEnumerable<IntVec3> IngredientPlaceCellsInOrder(Thing destination, int fallBackRadius = 15)
		{
			//Log.Message("gathering cells");
			var interactCell = destination.Position;
			IBillGiver billGiver = destination as IBillGiver;
			var cellsAlreadySent = new List<IntVec3>();
			IEnumerable<IntVec3> desiredPlaceCells;
			if (billGiver != null)
			{
				//Log.Message("using billGiver cells");
				interactCell = (billGiver as Thing).InteractionCell;
				desiredPlaceCells = billGiver.IngredientStackCells.OrderBy(c => c.LengthHorizontalSquared);
			}
            else 
			{
				//Log.Message("defaulting to covered cells");
				desiredPlaceCells = GenAdj.CellsOccupiedBy(destination).OrderBy(c => c.DistanceTo(destination.Position));
			}

			//Log.Message("candidates (ordered): " + String.Join(", ", desiredPlaceCells.Select(c => c.ToString())));

			foreach (var cell in desiredPlaceCells)
			{
				//Log.Message("yielding candidate "+cell);
				yield return cell;
			}
			cellsAlreadySent.AddRange(desiredPlaceCells);

			for (int i = 0; i < fallBackRadius; i++)
			{
				var cell = interactCell + GenRadial.RadialPattern[i];
                if (!cellsAlreadySent.Contains(cell))
				{
					//Log.Message("yielding backup candidate " + cell);
					Building edifice = cell.GetEdifice(destination.Map);
					if (edifice == null || edifice.def.passability != Traversability.Impassable || edifice.def.surfaceType != SurfaceType.None)
					{
						yield return cell;
					}
				}
			}
		}

		private static bool IsEmptyEnoughSpot(Thing haulable, Map map, IntVec3 c)
		{
			if (GenPlace.HaulPlaceBlockerIn(haulable, c, map, true) != null)
			{
				//Log.Message("blocked by haulPlaceBlockerIn");
				return false;
			}
			/*if (!c.Standable(map))
			{
				Log.Message("blocked by standable");
				return false;
			}*/
			if (c == haulable.Position && haulable.Spawned)
			{
				//Log.Message("blocked by being the original position");
				return false;
			}
			if (c.ContainsStaticFire(map))
			{
				//Log.Message("blocked by fire");
				return false;
			}
			if (haulable != null && haulable.def.BlocksPlanting(false) && map.zoneManager.ZoneAt(c) is Zone_Growing)
			{
				//Log.Message("blocked by plants????");
				return false;
			}
			/*if (haulable.def.passability != Traversability.Standable)
			{
				Log.Message("blocked by passability issues");
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c2 = c + GenAdj.AdjacentCells[i];
					if (map.designationManager.DesignationAt(c2, DesignationDefOf.Mine) != null)
					{
						return false;
					}
				}
			}*/
			Building edifice = c.GetEdifice(map);
			if(edifice != null && edifice is Building_Trap)
            {
				//Log.Message("blocked by trap");
				return false;
			}
			return true;
		}
	}
}
