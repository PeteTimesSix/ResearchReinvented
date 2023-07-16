using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.Data
{
    /*
     * kept in one file for ease of comparison
     * 
     * these values are mostly arbitrary, just trying to get them to *feel* right.
     * dont forget to account for the additional research speed multipliers
     * which are usually in the range of 200% to 300%.
    */
    public static class BaseResearchAmounts
    {
        public static float AdministerIngestibleObserver => 50f;
        public static float OnIngestIngester => 25f;
        public static float OnTendObserver => 25f;

        public static float InteractionBrainstorm => 15f;
        public static float InteractionLearnFromPrisoner => 50f;

                                                  /*from ticks to seconds, as in the UI*/
        public static float DoneWorkMultiplier => (1f / 60f) * 0.25f;
    }
}
