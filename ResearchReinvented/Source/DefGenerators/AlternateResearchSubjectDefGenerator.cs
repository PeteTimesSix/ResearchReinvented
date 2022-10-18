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
            var ancientThings = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant().Contains(ancientString));
            foreach (var ancientThing in ancientThings)
            {
                var unancientFullString = ancientThing.defName.ToLowerInvariant();
                var unancientNameIndex = unancientFullString.IndexOf(ancientString);
                if (unancientNameIndex == -1)
                    continue;
                unancientNameIndex += ancientString.Length;
                if (unancientNameIndex >= unancientFullString.Length - 5) //lets avoid picking up single letters
                    continue;
                var unancientName = unancientFullString.Substring(unancientNameIndex);
                if (unancientName[0] == '_') //sometimes people separate with underscores
                    unancientName = unancientName.Substring(1);

                var unancientMatchingThings = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant() == unancientName && d != ancientThing);
                foreach (var unancientThing in unancientMatchingThings)
                {
                    AlternateResearchSubjectsDef altDef = new AlternateResearchSubjectsDef();
                    altDef.defName = $"RR_auto_{ancientThing.defName}_{unancientThing}";
                    altDef.originals = new List<ThingDef>() { unancientThing };
                    altDef.alternates = new List<ThingDef>() { ancientThing };
                    altDef.modContentPack = ResearchReinventedMod.ModSingleton.Content;

                    //Log.Message($"Associating ancient {ancientThing} with non-ancient {unancientThing} as ({altDef.defName})");

                    //Debug.LogMessage
                    yield return altDef;
                }
            }
		}
	}
}
