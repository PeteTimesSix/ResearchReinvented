using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.ModCompat;
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
using static UnityEngine.GridBrushBase;

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
			var unminifiedThing = TargetThing.GetInnerIfMinified();
			ResearchOpportunity opportunity = WorkGiver_Analyse.OpportunityCache[unminifiedThing.def].FirstOrDefault();
            //ResearchOpportunity opportunity = ResearchOpportunityManager.instance.GetOpportunityForJob(this.job);
            ResearchProjectDef currentProject = Find.ResearchManager.currentProj;

            if (currentProject == null || opportunity == null)
            {
                if (currentProject == null)
                    Log.WarningOnce("RR: Generated JobDriver_Analyse job with no active project!", 456654 + pawn.thingIDNumber);
                else
                    Log.WarningOnce($"RR: Generated JobDriver_Analyse job {this.job} but could not find the matching opportunity!", 456654 + pawn.thingIDNumber);
                yield return Toils_General.Wait(1);
                yield break;
            }

            this.FailOn(() => currentProject != Find.ResearchManager.currentProj);
            this.FailOn(() => opportunity.CurrentAvailability != OpportunityAvailability.Available);
            if(ResearchData.active && ResearchReinventedMod.Settings.researchDataCompatMode == ResearchData.ResearchDataCompatMode.AllBenchResearch)
            {
                this.FailOn(() => ResearchBench.GetComp<CompRefuelable>() is CompRefuelable comp && comp.HasFuel is false);
            }

            this.FailOnBurningImmobile(ResearchBenchIndex);

            yield return Toils_Reserve.Reserve(TargetThingIndex);
            yield return Toils_Reserve.Reserve(ResearchBenchIndex);
            yield return Toils_Goto.GotoThing(TargetThingIndex, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetThingIndex).FailOnDestroyedNullOrForbidden(TargetThingIndex);
            yield return Toils_Reserve.Release(TargetThingIndex);
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
                num *= ResearchBench.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
                actor.GainComfortFromCellIfPossible(true);
                if (ResearchData.active && ResearchReinventedMod.Settings.researchDataCompatMode == ResearchData.ResearchDataCompatMode.AllBenchResearch)
                {
                    var fuelComp = ResearchBench.GetComp<CompRefuelable>();
                    if (fuelComp != null && fuelComp.Props.consumeFuelOnlyWhenUsed)
                    {
                        fuelComp.ConsumeTickFuel();
                    }
                }
                bool finished = opportunity.ResearchTickPerformed(num, actor);
                if (finished)
                    this.ReadyForNextToil();
            };
            research.WithEffect(EffecterDefOf.Research, ResearchBenchIndex);
            //research.FailOnDespawnedNullOrForbiddenPlacedThings();                                    //TODO: this is hardcoded nonsense, see Toils_Haul: Action<Thing, int> placedAction = null;
            research.FailOn(() => ResearchBench.IsForbidden(pawn));
            research.FailOn(() => TargetThing.IsForbidden(pawn));
            research.FailOn(() => {
                CompPowerTrader comp = ResearchBench.GetComp<CompPowerTrader>();
                if (comp != null && !comp.PowerOn)
                    return true;
                else
                    return false;
            });
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
