using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.ModCompat;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers
{
    public class JobDriver_ResearchRR : JobDriver_RRBase
	{
		protected Building_ResearchBench ResearchBench => job.targetA.Thing as Building_ResearchBench;

		private const TargetIndex ResearchBenchIndex = TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			ResearchOpportunity opportunity = WorkGiver_ResearcherRR.OpportunityCache;
			//ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
			ResearchProjectDef currentProject = Find.ResearchManager.currentProj;

			if (currentProject == null || opportunity == null)
			{
				if (currentProject == null)
					Log.WarningOnce("RR: Generated JobDriver_ResearchRR job with no active project!", 456654 + pawn.thingIDNumber);
				else
					Log.WarningOnce($"RR: Generated JobDriver_ResearchRR job {this.job} but could not find the matching opportunity!", 456654 + pawn.thingIDNumber);
				yield return Toils_General.Wait(1);
				yield break;
			}

            this.FailOn(() => currentProject != Find.ResearchManager.currentProj);
            this.FailOn(() => opportunity.CurrentAvailability != OpportunityAvailability.Available);
            if (ResearchData.active)
            {
                this.FailOn(() => ResearchBench.TryGetComp<CompRefuelable>() is CompRefuelable comp && comp.HasFuel is false);
            }

            this.FailOnDespawnedNullOrForbidden(ResearchBenchIndex);
			this.FailOnBurningImmobile(ResearchBenchIndex);
			this.FailOn(() => !ResearchBench.CanUseNow());
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil research = new Toil();
			research.tickAction = delegate ()
			{
				Pawn actor = research.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				num *= ResearchBench.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
				actor.GainComfortFromCellIfPossible(true);
                if (ResearchData.active)
                {
                    var fuelComp = ResearchBench.GetComp<CompRefuelable>();
                    if (fuelComp != null && fuelComp.Props.consumeFuelOnlyWhenUsed)
                    {
                        fuelComp.ConsumeTickFuel();
                    }
                }
                bool finished = opportunity.ResearchTickPerformed(num, actor, SkillDefOf.Intellectual);
				if (finished)
					this.ReadyForNextToil();
			};
			research.FailOn(() => ResearchBench.IsForbidden(pawn));
			research.FailOn(() => {
				CompPowerTrader comp = ResearchBench.GetComp<CompPowerTrader>();
				if (comp != null && !comp.PowerOn)
					return true;
				else
					return false;
			});
			research.AddEndCondition(() => opportunity.IsFinished || opportunity.CurrentAvailability != OpportunityAvailability.Available ? JobCondition.Succeeded : JobCondition.Ongoing);
			research.WithEffect(EffecterDefOf.Research, ResearchBenchIndex);
			research.AddFailCondition(() => !ResearchBench.CanUseNow());
			research.defaultCompleteMode = ToilCompleteMode.Never;
			research.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			research.activeSkill = (() => SkillDefOf.Intellectual);
			yield return research;
			yield break;
		}
	}
}
