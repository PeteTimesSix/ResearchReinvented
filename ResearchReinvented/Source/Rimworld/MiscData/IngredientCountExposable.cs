using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace PeteTimesSix.ResearchReinvented.Rimworld.MiscData
{
    public class IngredientCountExposable : IExposable
    {

        public IngredientCount Value { get; private set; }

        public IngredientCountExposable() : this(null) { }

        public IngredientCountExposable(IngredientCount value)
        {
            Value = value;
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Deep.Look(ref Value.filter, "filter");
                float count = 0f;
                Scribe_Values.Look(ref count, "count");
                Value.SetBaseCount(count);
            }
            else if (Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Deep.Look(ref Value.filter, "filter");
                float count = Value.GetBaseCount();
                Scribe_Values.Look(ref count, "count");
            }
        }
    }
}
