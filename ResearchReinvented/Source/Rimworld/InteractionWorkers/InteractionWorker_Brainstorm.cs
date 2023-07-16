using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
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
            if (initiator.WorkTypeIsDisabled(WorkTypeDefOf.Research) || recipient.WorkTypeIsDisabled(WorkTypeDefOf.Research))
                return 0f;

            return 0.25f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            var opportunity = ResearchOpportunityManager.Instance
                .GetCurrentlyAvailableOpportunitiesFiltered(false, HandlingMode.Social, recipient)
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Social) && o.requirement.MetBy(recipient))
            .FirstOrDefault();

            if (opportunity != null)
            {
                var amount = BaseResearchAmounts.InteractionBrainstorm;
                var modifier = Math.Max(initiator.GetStatValue(StatDefOf.ResearchSpeed), recipient.GetStatValue(StatDefOf.ResearchSpeed));
                opportunity.ResearchChunkPerformed(initiator, HandlingMode.Social, amount, modifier, SkillDefOf.Intellectual);
            }
        }
    }
}
