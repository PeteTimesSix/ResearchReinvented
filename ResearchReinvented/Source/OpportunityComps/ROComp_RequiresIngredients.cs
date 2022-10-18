using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    /*  Currently unused
    
    public class ROComp_RequiresIngredients : ResearchOpportunityComp
    {
        public List<IngredientCountExposable> ingredients;

        public override string ShortDesc
        {
            get
            {
                return String.Concat(ingredients?.Select(i => i.Value.Summary));
            }
        }

        public override bool TargetIsNull => ingredients is null;
        public override bool IsRare => false;
        public override bool MetBy(Def def) => false;
        public override bool MetBy(Thing thing) => false;
        public override bool IsValid => ingredients != null;

        public ROComp_RequiresIngredients() : base()
        {
            this.ingredients = new List<IngredientCountExposable>();
        }

        public ROComp_RequiresIngredients(List<IngredientCountExposable> ingredients) : base()
        {
            this.ingredients = ingredients;
        }

        public ROComp_RequiresIngredients(List<IngredientCount> ingredients) : base()
        {
            this.ingredients = ingredients.Select(i => new IngredientCountExposable(i)).ToList();
        }


        public ROComp_RequiresIngredients(List<ThingDef> things)
        {
            this.ingredients = things.Select(thing => {
                var ing = new IngredientCount();
                ing.filter = new ThingFilter();
                ing.filter.SetAllow(thing, true);
                ing.SetBaseCount(1);
                return new IngredientCountExposable(ing);
            }).ToList();
        }

        public ROComp_RequiresIngredients(List<ThingDefCountClass> thingCounts)
        {
            this.ingredients = thingCounts.Select(thingCount => {
                var ing = new IngredientCount();
                ing.filter = new ThingFilter();
                ing.filter.SetAllow(thingCount.thingDef, true);
                ing.SetBaseCount(thingCount.count);
                return new IngredientCountExposable(ing);
            }).ToList();
        }
    }*/
}
