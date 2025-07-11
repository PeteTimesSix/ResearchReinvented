using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
    public class AlternateResearchSubjectsDef : Def
    {
        public List<ThingDef> originals;
        public List<TerrainDef> originalTerrains;
        public List<RecipeDef> originalRecipes;
        public List<ThingDef> alternatesEquivalent;
        public List<TerrainDef> alternateEquivalentTerrains;
        public List<RecipeDef> alternateEquivalentRecipes;
        public List<ThingDef> alternatesSimilar;
        public List<TerrainDef> alternateSimilarTerrains;
        public List<RecipeDef> alternateSimilarRecipes;
    }
}
