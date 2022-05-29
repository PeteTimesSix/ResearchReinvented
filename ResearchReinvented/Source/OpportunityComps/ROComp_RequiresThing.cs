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
    class ROComp_RequiresThing : ResearchOpportunityComp
    {
        public ThingDef targetDef;

        public override string ShortDesc
        {
            get
            {
                return String.Concat(targetDef.label);
            }
        }
        public override bool TargetIsNull => targetDef is null;

        public override bool IsRare => targetDef.HasModExtension<RarityMarker>();

        public ROComp_RequiresThing() 
        {
            //for deserialization
        }

        public ROComp_RequiresThing(ThingDef targetDef)
        {
            this.targetDef = targetDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref targetDef, "targetDef");
        }
    }
}
