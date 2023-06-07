using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.InteractionWorkers
{
    public class InteractionWorker_LearnScience : InteractionWorker
    {
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                   .Where(o => o.def.handledBy.HasFlag(HandlingMode.Social) && o.requirement.MetBy(recipient))
            .FirstOrDefault();

            if (opportunity != null)
            {
                opportunity.ResearchChunkPerformed(initiator, recipient.Faction.Name, HandlingMode.Social, modifier: initiator.GetStatValue(StatDefOf.NegotiationAbility, true));
            }
        }
    }
}
