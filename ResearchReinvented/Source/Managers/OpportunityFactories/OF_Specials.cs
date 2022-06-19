using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    /*public static class OF_Specials
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
    }*/

    public static class OF_Specials
    {
        public static void MakeFromSpecials(ResearchProjectDef project, OpportunityFactoryCollectionsSet allCollections)
        {
            var setAncestor = new HashSet<SpecialResearchOpportunityDef>();
            var setDirect = new HashSet<SpecialResearchOpportunityDef>();
            var setDescendant = new HashSet<SpecialResearchOpportunityDef>();

            var projectMatches = DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.originalProject == project);
            setAncestor.AddRange(projectMatches.Where(m => m.IsForRelation(ResearchRelation.Ancestor)));
            setDirect.AddRange(projectMatches.Where(m => m.IsForRelation(ResearchRelation.Direct)));
            setDescendant.AddRange(projectMatches.Where(m => m.IsForRelation(ResearchRelation.Descendant)));

            DoAlternates(allCollections, setAncestor, ResearchRelation.Ancestor);
            DoAlternates(allCollections, setDirect, ResearchRelation.Direct);
            DoAlternates(allCollections, setDescendant, ResearchRelation.Descendant);

            allCollections.GetSet(ResearchRelation.Ancestor).specials.AddRange(setAncestor);
            allCollections.GetSet(ResearchRelation.Direct).specials.AddRange(setDirect);
            allCollections.GetSet(ResearchRelation.Descendant).specials.AddRange(setDescendant);
        }

        private static void DoAlternates(OpportunityFactoryCollectionsSet allCollections, HashSet<SpecialResearchOpportunityDef> set, ResearchRelation relation)
        {
            foreach (var thingDef in allCollections.GetSet(relation).forDirectAnalysis)
            {
                set.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.originals != null && s.IsForRelation(relation) && s.originals.Contains(thingDef)));
            }
        }
    }
}
