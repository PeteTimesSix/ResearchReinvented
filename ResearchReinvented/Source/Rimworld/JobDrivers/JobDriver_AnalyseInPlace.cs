using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
using PeteTimesSix.ResearchReinvented.Rimworld.Toils;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers
{


	public class JobDriver_AnalyseInPlace: JobDriver_RRBase
	{
		public virtual Thing TargetThing => job.targetA.Thing;
		public virtual TargetIndex TargetThingIndex => TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			ResearchOpportunity opportunity = WorkGiver_AnalyseInPlace.OpportunityCache[TargetThing.def].FirstOrDefault();
			//ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
			ResearchProjectDef currentProject = Find.ResearchManager.GetProject();

			if (currentProject == null || opportunity == null)
			{
				if (currentProject == null)
					Log.WarningOnce("RR: Generated JobDriver_AnalyseInPlace job with no active project!", 456654 + pawn.thingIDNumber);
				else
					Log.WarningOnce($"RR: Generated JobDriver_AnalyseInPlace job {this.job} but could not find the matching opportunity!", 456654 + pawn.thingIDNumber);
				yield return Toils_General.Wait(1);
				yield break;
			}

            this.FailOn(() => currentProject != Find.ResearchManager.GetProject());
            this.FailOn(() => opportunity.CurrentAvailability != OpportunityAvailability.Available);

            var pokeMode = TargetThing.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch;
			this.FailOnDespawnedNullOrForbidden(TargetThingIndex);
			if(TargetThing?.def != ThingDefOf.Fire)
				this.FailOnBurningImmobile(TargetThingIndex);

			Toil walkTo;

			if (pokeMode == PathEndMode.InteractionCell)
				walkTo = Toils_Goto.GotoThing(TargetThingIndex, PathEndMode.InteractionCell);
			else
				walkTo = Toils_Goto_Custom.GoAdjacentToThing(TargetThingIndex, true);

			yield return walkTo;

			Toil research = new Toil(); 
			research.WithEffect(() => FieldResearchHelper.GetResearchEffect(GetActor()), TargetThing);
			research.WithEffect(EffecterDefOf.Research, TargetThingIndex);
			research.FailOnDespawnedNullOrForbidden(TargetThingIndex);
			research.defaultCompleteMode = ToilCompleteMode.Delay;
			research.defaultDuration = 60 * 10;
			research.FailOnCannotTouch(TargetThingIndex, pokeMode);
			research.activeSkill = (() => SkillDefOf.Intellectual);
			research.tickIntervalAction = (int delta) =>
			{
				Pawn actor = research.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true) * 0.00825f;
				num *= FieldResearchHelper.GetFieldResearchSpeedFactor(actor, opportunity.project);
				actor.GainComfortFromCellIfPossible(1, true);
				bool finished = opportunity.ResearchTickPerformed(num, actor, delta);
				if (finished)
					this.ReadyForNextToil();
			};
			research.FailOn(() => TargetThing.IsForbidden(pawn));
			research.AddEndCondition(() => opportunity.IsFinished || opportunity.CurrentAvailability != OpportunityAvailability.Available ? JobCondition.Succeeded : JobCondition.Ongoing);
			yield return research;

			yield return Toils_Jump.JumpIf(walkTo, () => { return !opportunity.IsFinished; });

			yield return Toils_General.Wait(2, TargetIndex.None);

			yield break;
		}
	}
}
