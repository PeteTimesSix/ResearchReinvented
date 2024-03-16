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
            if (this.Talkee.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation))
            {
                return "RR_jobReport_ScienceInterrogation".Translate(this.Talkee);
            }
            return base.ReportStringProcessed(str);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnMentalState(TargetIndex.A);
            this.FailOnNotAwake(TargetIndex.A);
            this.FailOn(() => !this.Talkee.IsPrisonerOfColony || !this.Talkee.guest.PrisonerIsSecure);

            yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
            for (int i = 0; i < REPETITIONS; i++)
            {
                yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation);
                yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
                yield return ScienceInterrogationRequest(this.pawn, this.Talkee);
                yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation);
                yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
                yield return ScienceInterrogationReply(this.pawn, this.Talkee);
            }
            yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A).FailOn(() => !this.Talkee.guest.ScheduledForInteraction);
            yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
            yield return ScienceInterrogationFinalize(this.pawn, this.Talkee);
            yield break;
        }

        public static Toil ScienceInterrogationRequest(Pawn warden, Pawn talkee)
        {
            Toil toil = ToilMaker.MakeToil("ScienceInterrogation");
            toil.initAction = delegate ()
            {
                var interaction = InteractionDefOf_Custom.RR_ScienceInterrogation_Demand;

                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"warden to talkee: {warden} {warden?.interactions} {warden?.jobs} {warden?.jobs?.curDriver} {warden?.records} talkee: {talkee} {talkee?.guest} def: {interaction}");
                
                if (!warden.interactions.TryInteractWith(talkee, interaction))
                {
                    warden.jobs.curDriver.ReadyForNextToil();
                    return;
                }
                warden.records.Increment(RecordDefOf.PrisonersChatted);
            };
            toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 200;
            toil.activeSkill = (() => SkillDefOf.Social);
            return toil;
        }

        public static Toil ScienceInterrogationReply(Pawn warden, Pawn talkee)
        {
            Toil toil = ToilMaker.MakeToil("ScienceInterrogation");
            toil.initAction = delegate ()
            {
                var interaction = InteractionDefOf_Custom.RR_ScienceInterrogation_Reply_Reluctant;
                if (talkee.needs?.mood != null)
                {
                    var moodPercent = talkee.needs.mood.CurLevelPercentage;
                    if(moodPercent < 0.5)
                    {
                        var fraction = 1f - (moodPercent * 2f);
                        if (Rand.Chance(fraction))
                            interaction = InteractionDefOf_Custom.RR_ScienceInterrogation_Reply_Resistant;
                    }
                    else 
                    {
                        var fraction = ((moodPercent - 0.5f) * 2f);
                        if (Rand.Chance(fraction))
                            interaction = InteractionDefOf_Custom.RR_ScienceInterrogation_Reply_Cooperative;
                    }
                }

                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"talkee to warden: {warden} {warden?.interactions} {warden?.jobs} {warden?.jobs?.curDriver} {warden?.records} talkee: {talkee} {talkee?.guest} def: {interaction}");
                
                if (!warden.interactions.TryInteractWith(talkee, interaction))
                {
                    warden.jobs.curDriver.ReadyForNextToil();
                    return;
                }
                warden.records.Increment(RecordDefOf.PrisonersChatted);
            };
            toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 200;
            toil.activeSkill = (() => SkillDefOf.Social);
            return toil;
        }

        public static Toil ScienceInterrogationFinalize(Pawn pawn, Pawn talkee)
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
