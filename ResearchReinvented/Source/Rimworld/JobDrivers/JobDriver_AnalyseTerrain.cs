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
			ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
			ResearchProjectDef currentProject = Find.ResearchManager.currentProj;

			if (currentProject == null || opportunity == null)
				yield break;

			this.FailOn(() => { return currentProject != Find.ResearchManager.currentProj; });

			Toil walkTo = Toils_Goto.GotoCell(TargetCellIndex, PathEndMode.Touch);
			yield return walkTo;

			Toil research = new Toil();
			research.WithEffect(() => FieldResearchHelper.GetResearchEffect(GetActor()), TargetCellIndex);
			research.defaultCompleteMode = ToilCompleteMode.Delay;
			research.defaultDuration = 60 * 10;
			research.FailOnCannotTouch(TargetCellIndex, PathEndMode.Touch);
			research.activeSkill = (() => SkillDefOf.Intellectual);
			research.tickAction = delegate ()
			{
				Pawn actor = research.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				var speedMult = opportunity.def.GetCategory(opportunity.relation).Settings.researchSpeedMultiplier;
				num *= speedMult;
				num *= FieldResearchHelper.GetFieldResearchSpeedFactor(actor, opportunity.project);
				actor.skills.Learn(SkillDefOf.Intellectual, 0.1f * speedMult, false);
				bool finished = opportunity.ResearchTickPerformed(num, actor);
				if (finished)
					this.ReadyForNextToil();
			}; 
			research.FailOn(() => Find.ResearchManager.currentProj != currentProject || opportunity.CurrentAvailability != OpportunityAvailability.Available);
			research.AddEndCondition(() => opportunity.IsFinished || opportunity.CurrentAvailability != OpportunityAvailability.Available ? JobCondition.Succeeded : JobCondition.Ongoing);
			yield return research;

			yield break;
		}
	}
}
