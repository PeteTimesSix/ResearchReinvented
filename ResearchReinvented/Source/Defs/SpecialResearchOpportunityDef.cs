using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
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
        public ResearchProjectDef project;
        public ResearchOpportunityTypeDef opportunityType;
        public ResearchRelation? relationOverride = null;
        public AlternatesMode altsMode = AlternatesMode.NONE;
        public bool forDirect = true;
        public bool forAncestor = false;
        public bool forDescendant = false;
        public List<ThingDef> things;
        public List<TerrainDef> terrains;
        public List<RecipeDef> recipes;
        public float importanceMultiplier = 1f;
        public bool rare = false;
        public bool freebie = true;

        public bool IsForRelation(ResearchRelation relation)
        {
            switch (relation)
            {
                case ResearchRelation.Ancestor:
                    return forAncestor;
                case ResearchRelation.Direct:
                    return forDirect;
                case ResearchRelation.Descendant:
                    return forDescendant;
                default:
                    throw new ArgumentException("Invalid relation");
            }
        }
    }
}
