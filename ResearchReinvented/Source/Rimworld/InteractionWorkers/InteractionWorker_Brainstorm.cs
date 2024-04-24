using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.InteractionWorkers
{
    public class InteractionWorker_Brainstorm : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (!recipient.CanNowDoResearch(false) || !initiator.CanNowDoResearch(false) || (recipient.Faction != Faction.OfPlayer && initiator.Faction != Faction.OfPlayer))
                return 0f;

            return 0.25f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            var currentProject = Find.ResearchManager.GetProject();
            if (currentProject == null)
                return;

            var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfProject(currentProject, OpportunityAvailability.Available, HandlingMode.Social, recipient).FirstOrDefault();
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Social) && o.requirement.MetBy(recipient))
                //.FirstOrDefault();

            if (opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {initiator.LabelCap} brainstormed with {recipient.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                var amount = BaseResearchAmounts.InteractionBrainstorm;
                var modifier = Math.Max(initiator.GetStatValue(StatDefOf.ResearchSpeed), recipient.GetStatValue(StatDefOf.ResearchSpeed));
                var xp = 0; //handled in the interaction def
                opportunity.ResearchChunkPerformed(initiator, HandlingMode.Social, amount, modifier, xp);
            }
        }
    }
}
