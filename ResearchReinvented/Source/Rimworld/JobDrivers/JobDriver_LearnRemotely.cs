using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
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
    public class JobDriver_LearnRemotely : JobDriver
    {
        public virtual Thing TargetThing => job.targetA.Thing;
        public virtual TargetIndex TargetThingIndex => TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }

        public class ExceptionToken
        {
            public bool hasException = false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var faction = job.commTarget as Faction;

            if(faction == null)
            {
                Log.WarningOnce("RR: Generated JobDriver_LearnRemotely job but commsTarget was not a faction!", 451654 + pawn.thingIDNumber);
                yield return Toils_General.Wait(1);
                yield break;
            }

            var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Social, faction);
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Social) && o.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.MetByFaction(faction))
                //.Where(o => !o.IsFinished)
                //.FirstOrDefault();

            ResearchProjectDef currentProject = Find.ResearchManager.GetProject();

            if (currentProject == null || opportunity == null)
            {
                if (currentProject == null)
                    Log.WarningOnce("RR: Generated JobDriver_LearnRemotely job with no active project!", 453654 + pawn.thingIDNumber);
                else
                    Log.WarningOnce($"RR: Generated JobDriver_LearnRemotely job {this.job} but could not find the matching opportunity!", 456654 + pawn.thingIDNumber);
                yield return Toils_General.Wait(1);
                yield break;
            }

            this.FailOn(() => currentProject != Find.ResearchManager.GetProject());
            this.FailOn(() => opportunity.CurrentAvailability != OpportunityAvailability.Available);

            ExceptionToken token = new ExceptionToken();
            this.FailOn(() => !token.hasException && FactionLectureManager.Instance.IsOnCooldown((opportunity.requirement as ROComp_RequiresFaction).faction));

            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil walkTo = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOn((Toil to) => !((Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow);

            yield return walkTo;

            Toil research = new Toil();
            //research.WithEffect(() => FieldResearchHelper.GetResearchEffect(GetActor()), TargetThing);
            //research.WithEffect(EffecterDefOf_Custom.RR_LessonOverRadio, TargetThingIndex);
            research.FailOnDespawnedNullOrForbidden(TargetThingIndex);
            research.defaultCompleteMode = ToilCompleteMode.Delay;
            research.defaultDuration = 3000;
            research.FailOnCannotTouch(TargetThingIndex, PathEndMode.InteractionCell);
            research.activeSkill = (() => SkillDefOf.Intellectual);
            research.initAction = () =>
            {
                token.hasException = true;
                FactionLectureManager.Instance.PutOnCooldown((opportunity.requirement as ROComp_RequiresFaction).faction);
            };
            research.tickAction = () =>
            {
                Pawn actor = research.actor;
                float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true) * 0.00825f;
                actor.GainComfortFromCellIfPossible(true);
                bool finished = opportunity.ResearchTickPerformed(num, actor);
                if (finished)
                    this.ReadyForNextToil();
            };
            research.WithProgressBarToilDelay(TargetThingIndex);
            research.FailOn(() => TargetThing.IsForbidden(pawn));
            research.FailOn((Toil to) => !((Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow);
            research.AddEndCondition(() => opportunity.IsFinished || opportunity.CurrentAvailability != OpportunityAvailability.Available ? JobCondition.Succeeded : JobCondition.Ongoing);
            yield return research;

            //yield return Toils_Jump.JumpIf(walkTo, () => { return !opportunity.IsFinished; });

            yield return Toils_General.Wait(2, TargetIndex.None);

            yield break;
        }
    }
}
