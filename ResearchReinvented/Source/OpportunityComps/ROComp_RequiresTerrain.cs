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
    class ROComp_RequiresTerrain : ResearchOpportunityComp
    {
        public TerrainDef terrainDef;

        public override string ShortDesc
        {
            get
            {
                return String.Concat(terrainDef.label);
            }
        }
        public override bool TargetIsNull => terrainDef is null;

        public override bool IsRare => terrainDef.HasModExtension<RarityMarker>();

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
