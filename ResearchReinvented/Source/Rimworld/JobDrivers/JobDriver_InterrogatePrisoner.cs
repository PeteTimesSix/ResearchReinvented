using PeteTimesSix.ResearchReinvented.DefOfs;
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
    public class JobDriver_InterrogatePrisoner : JobDriver
    {
        public static readonly int REPETITIONS = 4;
        protected Pawn Talkee => (Pawn)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override string ReportStringProcessed(string str)
        {
            if (this.Talkee.guest.interactionMode == PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation)
            {
                return "RR_JobReport_ScienceInterrogation".Translate(this.Talkee);
            }
            return base.ReportStringProcessed(str);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnMentalState(TargetIndex.A);
            this.FailOnNotAwake(TargetIndex.A);
            this.FailOn(() => !this.Talkee.IsPrisonerOfColony || !this.Talkee.guest.PrisonerIsSecure);

            yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
            for (int i = 0; i < REPETITIONS; i++)
            {
                yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
                yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
                yield return ScienceInterrogation(this.pawn, this.Talkee);
            }
            yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A).FailOn(() => !this.Talkee.guest.ScheduledForInteraction);
            yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
            yield return NoteResultsOfInterrogation(this.pawn, this.Talkee);
            yield break;
        }

        public static Toil ScienceInterrogation(Pawn warden, Pawn talkee)
        {
            Toil toil = ToilMaker.MakeToil("ScienceInterrogation");
            toil.initAction = delegate ()
            {
                Log.Message($"warden: {warden} {warden?.interactions} {warden?.jobs} {warden?.jobs?.curDriver} {warden?.records} talkee: {talkee} {talkee?.guest} def: {InteractionDefOf_Custom.RR_ScienceInterrogation}");
                if (!warden.interactions.TryInteractWith(talkee, InteractionDefOf_Custom.RR_ScienceInterrogation))
                {
                    warden.jobs.curDriver.ReadyForNextToil();
                    return;
                }
                warden.records.Increment(RecordDefOf.PrisonersChatted);
            };
            toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 350;
            toil.activeSkill = (() => SkillDefOf.Social);
            return toil;
        }

        public static Toil NoteResultsOfInterrogation(Pawn pawn, Pawn talkee)
        {
            Toil toil = ToilMaker.MakeToil("NoteResultsOfInterrogation");
            toil.initAction = delegate ()
            {
                pawn.interactions.TryInteractWith(talkee, InteractionDefOf_Custom.RR_ScienceInterrogationFinalize);
            };
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 350;
            toil.activeSkill = (() => SkillDefOf.Social);
            return toil;
        }
    }
}
