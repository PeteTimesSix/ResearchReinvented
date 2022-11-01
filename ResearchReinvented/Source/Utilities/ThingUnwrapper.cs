using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class ThingUnwrapper
    {
		public static Thing UnwrapIfWrapped(this Thing outerThing)
		{
            if (outerThing is MinifiedThing minified)
            {
                return minified.InnerThing;
            }
            else if (outerThing is Corpse corpse)
            {
                return corpse.InnerPawn;
            }
            return outerThing;
        }
	}
}
