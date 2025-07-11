using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public class ROComp_RequiresRecipe : ResearchOpportunityComp
    {
        public AlternatesMode altsMode;
        private bool alternatesCached = false;
        private RecipeDef primaryRecipeDef;
        private RecipeDef[] alternates;
        public RecipeDef[] Alternates
        {
            get
            {
                if (!alternatesCached)
                    CacheAlternates();
                return alternates;
            }
        }
        public override int AlternateCount
        {
            get
            {
                if (!alternatesCached)
                    CacheAlternates();
                return alternates?.Length ?? 0;
            }
        }
        private RecipeDef[] allRecipes;
        public RecipeDef[] AllRecipes
        {
            get
            {
                if (allRecipes == null)
                {
                    var tempList = new List<RecipeDef>() { primaryRecipeDef };
                    if (Alternates != null)
                        tempList.AddRange(Alternates);
                    allRecipes = tempList.ToArray();
                }
                return allRecipes;
            }
        }

        public RecipeDef ShownCycledRecipe
        {
            get
            {
                var recipeDef = primaryRecipeDef;
                if (AlternateCount > 0 && Time.realtimeSinceStartupAsDouble % 2 > 1.0)
                {
                    recipeDef = Alternates[(int)((Time.realtimeSinceStartupAsDouble / 2.0) % AlternateCount)];
                }
                return recipeDef;
            }
        }

        public override string ShortDesc => String.Concat(ShownCycledRecipe?.label);
        public override TaggedString Subject => new TaggedString(ShownCycledRecipe?.ProducedThingDef?.label ?? ShownCycledRecipe?.label).Colorize(Color.cyan);
        public override bool TargetIsNull => primaryRecipeDef is null;

        public override bool IsRare => primaryRecipeDef.HasModExtension<RarityMarker>();
        public override bool IsFreebie => primaryRecipeDef.HasModExtension<FreebieMarker>();

        public override bool MetBy(Def def)
        {
            if (def is not RecipeDef)
                return false;
            if (def == primaryRecipeDef)
                return true;
            if (!alternatesCached)
                CacheAlternates();
            if (alternates != null && alternates.Contains(def))
                return true;
            return false;
        }

        public override bool MetBy(Thing thing) => false;
        public override bool IsValid => primaryRecipeDef != null;

        public ROComp_RequiresRecipe() 
        {
            //for deserialization
        }

        public ROComp_RequiresRecipe(RecipeDef targetDef, AlternatesMode altsMode)
        {
            this.primaryRecipeDef = targetDef;
            this.altsMode = altsMode;
        }


        public void CacheAlternates()
        {
            switch (altsMode)
            {
                case AlternatesMode.NONE:
                    alternates = null;
                    break;
                case AlternatesMode.EQUIVALENT:
                    {
                        if (AlternatesKeeper.alternateEquivalentRecipes.TryGetValue(primaryRecipeDef, out RecipeDef[] alts))
                            alternates = alts;
                    }
                    break;
                case AlternatesMode.SIMILAR:
                    {
                        if (AlternatesKeeper.alternateSimilarRecipes.TryGetValue(primaryRecipeDef, out RecipeDef[] alts))
                            alternates = alts;
                    }
                    break;
            }
            //if(alternates != null) 
            //    Log.Message($"alts for {primaryRecipeDef.defName}: {string.Join(",", alternates.Select(a => a.defName))}");
            alternatesCached = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref primaryRecipeDef, "targetDef");
            Scribe_Values.Look(ref altsMode, "altsMode");
        }
    }
}
