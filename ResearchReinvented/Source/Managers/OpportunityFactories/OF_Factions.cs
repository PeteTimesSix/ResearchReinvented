using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public static class OF_Factions
    {
        public static Dictionary<(TechLevel factionLevel, TechLevel projectLevel), float> Modifiers { get; } = new Dictionary<(TechLevel projectLevel, TechLevel factionLevel), float>();

        static OF_Factions() 
        {
            var values = Enum.GetValues(typeof(TechLevel));
            //default to zero
            foreach (TechLevel factionLevel in values)
            {
                foreach (TechLevel projectLevel in values)
                {
                    Modifiers[(factionLevel, projectLevel)] = 0f;
                }
            }

            for(int i = 0; i < values.Length; i++)
            {
                if(i < values.Length - 1)
                {
                    Modifiers[((TechLevel)i, (TechLevel)(i + 1))] = 0.25f; 
                }
                Modifiers[((TechLevel)i, (TechLevel)(i))] = 1f;
                for (int j = i - 1; j >= 0; j--) 
                {
                    float diff = ((i - j) + 1);
                    diff *= (float)Math.Log(diff);

                    var fraction = 1f / diff;
                    if (fraction < 0.1f)
                        fraction = 0;
                    if(fraction > 0)
                        Modifiers[((TechLevel)i, (TechLevel)j)] = fraction; 
                }
            }

            foreach (TechLevel factionLevel in values)
            {
                foreach (TechLevel projectLevel in values)
                {
                    Log.Message($"faction {factionLevel} to project {projectLevel} = {Modifiers[(factionLevel, projectLevel)]}");
                }
            }
        }
    }
}
