using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.Comps.CompProperties
{
    public class CompProperties_ResearchKit : Verse.CompProperties
    {
        public EffecterDef fieldworkEffect;

        public ThingDef substitutedResearchBench;
        public List<ThingDef> substitutedFacilities;
        public ThingDef remotesThrough;

        public CompProperties_ResearchKit()
        {
            this.compClass = typeof(Comp_ResearchKit);
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            var benchHyperlink = new Dialog_InfoCard.Hyperlink(substitutedResearchBench, -1);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "RR_EquivalentResearchBench".Translate(), substitutedResearchBench.LabelCap, "RR_EquivalentResearchBench_desc".Translate(), -1, hyperlinks: new List<Dialog_InfoCard.Hyperlink>() { benchHyperlink });

            if (!substitutedFacilities.NullOrEmpty())
            {
                List<Dialog_InfoCard.Hyperlink> hyperlinks = new List<Dialog_InfoCard.Hyperlink>();
                List<string> labels = new List<string>();
                foreach (var facility in substitutedFacilities)
                {
                    hyperlinks.Add(new Dialog_InfoCard.Hyperlink(facility, -1));
                    labels.Add(facility.LabelCap);
                }

                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "RR_EquivalentResearchFacilities".Translate(), string.Join(", ", labels), "RR_EquivalentResearchFacilities_desc".Translate(), -1, hyperlinks: hyperlinks);
            }

            if (remotesThrough != null)
            {
                var remoteHyperlink = new Dialog_InfoCard.Hyperlink(remotesThrough, -1);
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "RR_RemoteConnectionFacility".Translate(), remotesThrough.LabelCap, "RR_RemoteConnectionFacility_desc".Translate(), -1, hyperlinks: new List<Dialog_InfoCard.Hyperlink>() { remoteHyperlink });
            }
        }
    }
}
