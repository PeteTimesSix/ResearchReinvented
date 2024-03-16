using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Rimworld.Comps;
using PeteTimesSix.ResearchReinvented.Rimworld.Comps.CompProperties;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.MiscData
{
    public class FieldResearchHelper
    {
        public static IEnumerable<ThingWithComps> GetValidResearchKits(Pawn pawn, ResearchProjectDef project) 
        {
            if(pawn?.apparel?.WornApparel == null)
                return Enumerable.Empty<ThingWithComps>();
            var kits = pawn.apparel.WornApparel.Where(a => a.TryGetComp<Comp_ResearchKit>() != null).Where(a => a.GetComp<Comp_ResearchKit>().MeetsProjectRequirements(project));
            //Log.Message($"pawn {pawn.Name} has kits: {string.Join(", ", kits.ToList().Select(k => k.LabelShortCap))}");
            return kits;
        }

        public static float GetFieldResearchSpeedFactor(Pawn pawn, ResearchProjectDef project = null)
        {
            float localFactor = pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true);

            float bestRemote = -1f;

            var bestRemoteKit = BestRemoteResearchKit(pawn, project);
            if (bestRemoteKit != null)
                bestRemote = bestRemoteKit.GetComp<Comp_ResearchKit>().GetRemoteResearchSpeedFactor(project);

            if (bestRemote >= 1f)
                localFactor *= bestRemote;

            return localFactor;
        }

        private static ThingWithComps BestRemoteResearchKit(Pawn pawn, ResearchProjectDef project = null) 
        {
            var remoteResearchKits = pawn.apparel.WornApparel.Where(a =>
            {
                var comp = a.TryGetComp<Comp_ResearchKit>();
                return (comp != null && comp.Props.remotesThrough != null);
            });
            if (remoteResearchKits.Any()) 
            {
                var bestKit = remoteResearchKits.OrderByDescending(kit => kit.GetComp<Comp_ResearchKit>().GetRemoteResearchSpeedFactor(project)).First();
                return bestKit;
            }
            return null;
        }

        public static EffecterDef GetResearchEffect(Pawn pawn)
        {
            var researchKits = pawn.apparel.WornApparel.Where(a => a.TryGetComp<Comp_ResearchKit>() != null);
            if (!researchKits.Any())
            {
                return EffecterDefOf_Custom.RR_NoResearchKitEffect;
            }
            else
            {
                return researchKits.OrderBy(k => k.GetComp<Comp_ResearchKit>().GetTotalResearchSpeedFactor()).First().GetComp<Comp_ResearchKit>().Props.fieldworkEffect;
            }
        }

    }
}
