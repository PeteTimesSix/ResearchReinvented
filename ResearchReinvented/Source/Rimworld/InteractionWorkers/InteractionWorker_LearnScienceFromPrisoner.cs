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
    public class InteractionWorker_LearnScienceFromPrisoner : InteractionWorker
    {
        public float BaseResearchAmount => 10f;

        public SimpleCurve moodCurve = new SimpleCurve()
        {
            Points = {
                new CurvePoint(0f, 0),
                new CurvePoint(0.1f, 0),
                new CurvePoint(0.75f, 1f),
                new CurvePoint(1f, 1f)
            }
        };

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
                    Log.Message($"pawn {initiator.LabelCap} learned science from prisoner {recipient.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                var amount = BaseResearchAmounts.InteractionLearnFromPrisoner;
                var modifier = initiator.GetStatValue(StatDefOf.NegotiationAbility) * Math.Max(initiator.GetStatValue(StatDefOf.ResearchSpeed), StatDefOf.ResearchSpeed.Worker.IsDisabledFor(recipient) ? 0 : recipient.GetStatValue(StatDefOf.ResearchSpeed));

                if(recipient.needs?.mood != null) 
                {
                    var moodPercent = recipient.needs.mood.CurLevelPercentage;
                    var moodModifier = moodCurve.Evaluate(moodPercent);
                    modifier *= moodModifier;
                }
                var xp = 0; //handled in the interaction def

                opportunity.ResearchChunkPerformed(initiator, HandlingMode.Social, amount, modifier, xp, recipient.Faction?.Name);
            }
        }
    }
}
