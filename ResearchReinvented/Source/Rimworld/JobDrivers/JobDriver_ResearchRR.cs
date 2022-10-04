using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
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
			ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
			ResearchProjectDef currentProject = Find.ResearchManager.currentProj;

			if (currentProject == null || opportunity == null)
				yield break;

			this.FailOn(() => { return currentProject != Find.ResearchManager.currentProj; });

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
				var speedMult = opportunity.def.GetCategory(opportunity.relation).researchSpeedMultiplier;
				num *= speedMult;
				num *= ResearchReinventedMod.Settings.theoryResearchSpeedMult;
				actor.skills.Learn(SkillDefOf.Intellectual, 0.1f * speedMult, false);
				actor.GainComfortFromCellIfPossible(true);
				bool finished = opportunity.ResearchTickPerformed(num, actor);
				if (finished)
					this.ReadyForNextToil();
			};
			research.FailOn(() => Find.ResearchManager.currentProj != currentProject);
			research.FailOn(() => {
				CompPowerTrader comp = ResearchBench.GetComp<CompPowerTrader>();
				if (comp != null && !comp.PowerOn)
					return true;
				else
					return false;
			});
			research.FailOn(() => opportunity.CurrentAvailability != OpportunityAvailability.Available);
			research.AddEndCondition(() => opportunity.IsFinished || opportunity.CurrentAvailability != OpportunityAvailability.Available ? JobCondition.Succeeded : JobCondition.Ongoing);
			research.WithEffect(EffecterDefOf.Research, ResearchBenchIndex);
			research.AddFailCondition(() => !ResearchBench.CanUseNow());
			research.defaultCompleteMode = ToilCompleteMode.Never;
			research.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			research.activeSkill = (() => SkillDefOf.Intellectual);
			yield return research;
			yield break;
		}

		/*public const int JobEndInterval = 4000;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.ResearchBench, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForPawn(this.pawn);
			ResearchProjectDef currentProject = Find.ResearchManager.currentProj;

			this.FailOnDespawnedNullOrForbidden(ResearchBenchIndex);
			yield return Toils_Goto.GotoThing(ResearchBenchIndex, PathEndMode.InteractionCell);
			Toil research = new Toil();
			research.tickAction = delegate ()
			{
				Pawn actor = research.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				num *= ResearchBench.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
				actor.skills.Learn(SkillDefOf.Intellectual, 0.1f, false);
				actor.GainComfortFromCellIfPossible(true);
				bool finished = opportunity.ResearchPerformed(num, actor);
				if (finished)
					this.ReadyForNextToil();
			};
			research.FailOn(() => Find.ResearchManager.currentProj != currentProject);
			research.FailOn(() => !Find.ResearchManager.currentProj.CanBeResearchedAt(ResearchBench, false));
			research.AddEndCondition(() => opportunity.IsFinished ? JobCondition.Succeeded : JobCondition.Ongoing);
			research.FailOnCannotTouch(ResearchBenchIndex, PathEndMode.InteractionCell);
			research.WithEffect(EffecterDefOf.Research, ResearchBenchIndex);
			research.WithProgressBar(ResearchBenchIndex, delegate
			{
				if (opportunity == null)
				{
					return 0f;
				}
				return opportunity.ProgressFraction;
			}, false, -0.5f);
			research.defaultCompleteMode = ToilCompleteMode.Never;
			//research.defaultDuration = JobEndInterval;
			research.activeSkill = (() => SkillDefOf.Intellectual);
			yield return research;
			yield return Toils_General.Wait(2, TargetIndex.None);
			yield break;
		}*/

	}
}
