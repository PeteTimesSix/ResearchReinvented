using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
    public class SpecialResearchOpportunityDef : Def
    {
        public ResearchOpportunityTypeDef opportunityType;
        public ResearchProjectDef originalProject;
        public ResearchRelation relation = ResearchRelation.Direct;
        public List<ThingDef> originals;
        public List<ThingDef> alternates;
        public List<TerrainDef> alternateTerrains;
        public float importanceMultiplier = 1f;
        public bool rareForThis = false;
        public bool markAsRare = false;
    }
}
