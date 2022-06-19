using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
    public static class DefExtensions
    {
        public static bool PassesIdeoCheck(this Def def)
        {
            if (!ModsConfig.IdeologyActive)
                return true;

            if(def is RecipeDef asRecipe) 
            {
                return asRecipe.PassesIdeoCheck();
            }
            else 
            {
                if(def is BuildableDef asBuildable && !asBuildable.canGenerateDefaultDesignator) 
                {
                    foreach (MemeDef memeDef in DefDatabase<MemeDef>.AllDefsListForReading)
                    {
                        if (memeDef.AllDesignatorBuildables.Contains(asBuildable) && !Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(memeDef))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
