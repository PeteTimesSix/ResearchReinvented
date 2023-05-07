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
    public class ROComp_RequiresTerrain : ResearchOpportunityComp
    {
        public TerrainDef terrainDef;

        public override string ShortDesc => String.Concat(terrainDef?.label);
        public override TaggedString Subject => new TaggedString(terrainDef?.label).Colorize(Color.cyan);
        public override bool TargetIsNull => terrainDef is null;

        public override bool IsRare => terrainDef.HasModExtension<RarityMarker>();
        public override bool MetBy(Def def) => def == terrainDef;
        public override bool MetBy(Thing thing) => false;
        public override bool IsValid => terrainDef != null;

        public ROComp_RequiresTerrain() 
        {
            //for deserialization
        }

        public ROComp_RequiresTerrain(TerrainDef terrainDef)
        {
            this.terrainDef = terrainDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref terrainDef, "terrainDef");
        }
    }
}
