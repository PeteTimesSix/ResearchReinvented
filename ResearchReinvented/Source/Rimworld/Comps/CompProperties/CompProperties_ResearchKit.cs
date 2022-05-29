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
    }
}
