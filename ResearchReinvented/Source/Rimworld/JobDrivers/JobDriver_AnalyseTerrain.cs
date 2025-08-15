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


	public class JobDriver_AnalyseTerrain: JobDriver_RRBase
	{
		public virtual IntVec3 TargetCell => job.targetA.Cell;
		public virtual TargetIndex TargetCellIndex => TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
        {
			var opportunityAt = WorkGiver_AnalyseTerrain.FindOpportunityAt(pawn, TargetCell);
			var opportunity = opportunityAt?.opportunity;

			//ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
			ResearchProjectDef currentProject = Find.ResearchManager.GetProject();

			if (currentProject == null || opportunity == null)
			{
				if (currentProject == null)
					Log.WarningOnce("RR: Generated JobDriver_AnalyseTerrain job with no active project!", 456654 + pawn.thingIDNumber);
				else
					Log.WarningOnce($"RR: Generated JobDriver_AnalyseTerrain job {this.job} but could not find the matching opportunity!", 456654 + pawn.thingIDNumber);
				yield return Toils_General.Wait(1);
				yield break;
			}

            this.FailOn(() => currentProject != Find.ResearchManager.GetProject());
            this.FailOn(() => opportunity.CurrentAvailability != OpportunityAvailability.Available);

            Toil walkTo = Toils_Goto.GotoCell(TargetCellIndex, PathEndMode.Touch);
			yield return walkTo;

			Toil research = new Toil();
			research.WithEffect(() => FieldResearchHelper.GetResearchEffect(GetActor()), TargetCellIndex);
			research.defaultCompleteMode = ToilCompleteMode.Delay;
			research.defaultDuration = 60 * 10;
			research.FailOnCannotTouch(TargetCellIndex, PathEndMode.Touch);
			research.activeSkill = (() => SkillDefOf.Intellectual);
			research.tickIntervalAction = (int delta) =>
			{
				Pawn actor = research.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true) * 0.00825f;
				num *= FieldResearchHelper.GetFieldResearchSpeedFactor(actor, opportunity.project);
				bool finished = opportunity.ResearchTickPerformed(num, actor, delta);
				if (finished)
					this.ReadyForNextToil();
			};
			research.FailOn(() => TargetCell.IsForbidden(pawn));
			research.AddEndCondition(() => opportunity.IsFinished || opportunity.CurrentAvailability != OpportunityAvailability.Available ? JobCondition.Succeeded : JobCondition.Ongoing);
			yield return research;

			yield break;
		}
	}
}
