using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.Toils;
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

    public class JobDriver_Analyse : JobDriver_RRBase
    {
        public const int JobEndInterval = 4000;

        protected Thing TargetThing => job.targetA.Thing;
        protected Building_ResearchBench ResearchBench => job.targetB.Thing as Building_ResearchBench;

        private const TargetIndex TargetThingIndex = TargetIndex.A;
        private const TargetIndex ResearchBenchIndex = TargetIndex.B;
        private const TargetIndex StorageCellWhenDoneIndex = TargetIndex.B;
        private const TargetIndex TargetThingPlacementIndex = TargetIndex.C;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(this.job.GetTarget(ResearchBenchIndex), this.job, 1, -1, null, errorOnFailed) && pawn.Reserve(this.job.GetTarget(TargetThingIndex), this.job, 1, 1, null, errorOnFailed))
            {
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
            ResearchProjectDef currentProject = Find.ResearchManager.currentProj;

            if (currentProject == null || opportunity == null)
                yield break;

            this.FailOn(() => { return currentProject != Find.ResearchManager.currentProj; });

            this.FailOnBurningImmobile(ResearchBenchIndex);

            yield return Toils_Reserve.Reserve(TargetThingIndex);
            yield return Toils_Reserve.Reserve(ResearchBenchIndex);
            yield return Toils_Goto.GotoThing(TargetThingIndex, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetThingIndex).FailOnDestroyedNullOrForbidden(TargetThingIndex);
            yield return Toils_Haul.StartCarryThing(TargetThingIndex, putRemainderInQueue: false);
            yield return Toils_Goto.GotoThing(ResearchBenchIndex, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetThingIndex);
            yield return Toils_ClearCell.ClearDefaultIngredientPlaceCell(ResearchBenchIndex);
            Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(ResearchBenchIndex, TargetThingIndex, TargetThingPlacementIndex);
            yield return findPlaceTarget;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetThingPlacementIndex, findPlaceTarget, false, false);
            Toil research = new Toil();
            research.tickAction = () =>
            {
                Pawn actor = research.actor;
                float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				var speedMult = opportunity.def.GetCategory(opportunity.relation).Settings.researchSpeedMultiplier;
				num *= speedMult;
                num *= ResearchBench.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.1f * speedMult, false);
                actor.GainComfortFromCellIfPossible(true);
                bool finished = opportunity.ResearchTickPerformed(num, actor);
                if (finished)
                    this.ReadyForNextToil();
            };
            research.WithEffect(EffecterDefOf.Research, ResearchBenchIndex);
            //research.FailOnDespawnedNullOrForbiddenPlacedThings();                                    //TODO: this is hardcoded nonsense, see Toils_Haul: Action<Thing, int> placedAction = null;
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
            //research.FailOnCannotTouch(ResearchBenchIndex, PathEndMode.InteractionCell);
            research.WithProgressBar(ResearchBenchIndex, () =>
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
            yield return Toils_Jump.JumpIf(research, () => !opportunity.IsFinished);
            yield return Toils_Haul_Custom.FindStorageForThing(TargetThingIndex, StorageCellWhenDoneIndex); 
            yield return Toils_Haul.StartCarryThing(TargetThingIndex, putRemainderInQueue: false);
            yield return Toils_Goto.GotoCell(StorageCellWhenDoneIndex, PathEndMode.Touch).FailOnDestroyedOrNull(TargetThingIndex);
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellWhenDoneIndex, null, true);
            //yield return Toils_General.Wait(2, TargetIndex.None);
            yield break;
        }
    }
}
