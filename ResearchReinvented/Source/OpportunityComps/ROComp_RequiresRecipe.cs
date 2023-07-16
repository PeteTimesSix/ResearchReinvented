using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public class ROComp_RequiresRecipe : ResearchOpportunityComp
    {
        public RecipeDef recipeDef;

        public override string ShortDesc => String.Concat(recipeDef?.label);
        public override TaggedString Subject => new TaggedString(recipeDef?.ProducedThingDef?.label ?? recipeDef?.label).Colorize(Color.cyan);
        public override bool TargetIsNull => recipeDef is null;

        public override bool IsRare => recipeDef.HasModExtension<RarityMarker>();
        public override bool MetBy(Def def) => def == recipeDef;
        public override bool MetBy(Thing thing) => false;
        public override bool IsValid => recipeDef != null;

        public ROComp_RequiresRecipe() 
        {
            //for deserialization
        }

        public ROComp_RequiresRecipe(RecipeDef targetDef)
        {
            this.recipeDef = targetDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref recipeDef, "targetDef");
        }
    }
}
