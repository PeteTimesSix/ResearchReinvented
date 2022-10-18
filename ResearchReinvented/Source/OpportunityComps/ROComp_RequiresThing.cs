using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public class ROComp_RequiresThing : ResearchOpportunityComp
    {
        public ThingDef thingDef;

        public override string ShortDesc
        {
            get
            {
                return String.Concat(thingDef?.label);
            }
        }
        public override bool TargetIsNull => thingDef is null;

        public override bool IsRare => thingDef.HasModExtension<RarityMarker>();
        public override bool MetBy(Def def) => def == thingDef;
        public override bool MetBy(Thing thing) => thing.def == thingDef || ((thing is MinifiedThing minified) && minified.InnerThing.def == thingDef);
        public override bool IsValid => thingDef != null;

        public ROComp_RequiresThing() 
        {
            //for deserialization
        }

        public ROComp_RequiresThing(ThingDef targetDef)
        {
            this.thingDef = targetDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref thingDef, "targetDef");
        }
    }
}
