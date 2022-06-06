using PeteTimesSix.ResearchReinvented.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public static class OF_Specials
    {
        internal static void MakeFromSpecials(ResearchProjectDef project, OpportunityFactoryCollectionsSetForRelation collections)
        {
            var set = new HashSet<SpecialResearchOpportunityDef>();
            set.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.IsForRelation(collections.relation) && s.originalProject == project));

            foreach (var thingDef in collections.forDirectAnalysis)
            {
                set.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.IsForRelation(collections.relation) && s.originals != null && s.originals.Contains(thingDef)));
            }

            collections.specials.AddRange(set);
        }
    }
}
