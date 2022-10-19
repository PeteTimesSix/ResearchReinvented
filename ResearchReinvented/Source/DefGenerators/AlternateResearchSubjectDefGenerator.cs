using PeteTimesSix.ResearchReinvented.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.DefGenerators
{
    public static class AlternateResearchSubjectDefGenerator
    {
		public static IEnumerable<AlternateResearchSubjectsDef> AncientAlternateDefs()
		{
            string ancientString = "ancient";
            return AlternateDefsOfDefNamePrefix(ancientString);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> AdvancedAlternateDefs()
        {
            string ancientString = "advanced";
            return AlternateDefsOfDefNamePrefix(ancientString);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> AlternateDefsOfDefNamePrefix(string prefixString)
        {
            var matchingThings = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant().Contains(prefixString));
            foreach (var matchingThing in matchingThings)
            {
                var alternateFullString = matchingThing.defName.ToLowerInvariant();
                var alternateNameIndex = alternateFullString.IndexOf(prefixString);
                if (alternateNameIndex == -1)
                    continue;
                alternateNameIndex += prefixString.Length;
                if (alternateNameIndex >= alternateFullString.Length - 5) //lets avoid picking up single letters
                    continue;
                var alternateName = alternateFullString.Substring(alternateNameIndex);
                if (alternateName[0] == '_') //sometimes people separate with underscores
                    alternateName = alternateName.Substring(1);

                var matchingTargetThings = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant() == alternateName && d != matchingThing);
                foreach (var targetThing in matchingTargetThings)
                {
                    AlternateResearchSubjectsDef altDef = new AlternateResearchSubjectsDef();
                    altDef.defName = $"RR_auto_{prefixString}_{matchingThing.defName}_{targetThing}";
                    altDef.originals = new List<ThingDef>() { targetThing };
                    altDef.alternates = new List<ThingDef>() { matchingThing };
                    altDef.modContentPack = ResearchReinventedMod.ModSingleton.Content;

                    //Log.Message($"Associating {prefixString} {matchingThing} with {targetThing} as ({altDef.defName})");
                    yield return altDef;
                }
            }
        }
    }
}
