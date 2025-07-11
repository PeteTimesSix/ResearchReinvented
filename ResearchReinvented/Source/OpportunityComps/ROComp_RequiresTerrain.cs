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
    public class ROComp_RequiresTerrain : ResearchOpportunityComp
    {
        public AlternatesMode altsMode;
        private bool alternatesCached = false;
        private TerrainDef primaryTerrainDef;
        private TerrainDef[] alternates;
        public TerrainDef[] Alternates
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
        private TerrainDef[] allTerrains;
        public TerrainDef[] AllTerrains
        {
            get
            {
                if (allTerrains == null)
                {
                    var tempList = new List<TerrainDef>() { primaryTerrainDef };
                    if (Alternates != null)
                        tempList.AddRange(Alternates);
                    allTerrains = tempList.ToArray();
                }
                return allTerrains;
            }
        }

        public TerrainDef ShownCycledTerrain
        {
            get {
                var terrainDef = primaryTerrainDef;
                if (AlternateCount > 0 && Time.realtimeSinceStartupAsDouble % 2 > 1.0)
                {
                    terrainDef = Alternates[(int)((Time.realtimeSinceStartupAsDouble / 2.0) % AlternateCount)];
                }
                return terrainDef;
            }
        }

        public override string ShortDesc => String.Concat(ShownCycledTerrain?.label);
        public override TaggedString Subject => new TaggedString(ShownCycledTerrain?.label).Colorize(Color.cyan);
        public override bool TargetIsNull => primaryTerrainDef is null;

        public override bool IsRare => primaryTerrainDef.HasModExtension<RarityMarker>();
        public override bool IsFreebie => primaryTerrainDef.HasModExtension<FreebieMarker>();
        public override bool MetBy(Def def)
        {
            if (def is not TerrainDef)
                return false;
            if (def == primaryTerrainDef)
                return true;
            if (!alternatesCached)
                CacheAlternates();
            if (alternates != null && alternates.Contains(def))
                return true;
            return false;
        }
        public override bool MetBy(Thing thing) => false;
        public override void ListAlts()
        {
            Log.Message($"alts for {primaryTerrainDef.defName}: {string.Join(",", alternates.Select(a => a.defName))}");
        }
        public override bool IsValid => primaryTerrainDef != null;

        public ROComp_RequiresTerrain() 
        {
            //for deserialization
        }

        public ROComp_RequiresTerrain(TerrainDef terrainDef, AlternatesMode altsMode)
        {
            this.primaryTerrainDef = terrainDef;
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
                        if (AlternatesKeeper.alternateEquivalentTerrains.TryGetValue(primaryTerrainDef, out TerrainDef[] alts))
                            alternates = alts;
                    }
                    break;
                case AlternatesMode.SIMILAR:
                    {
                        if (AlternatesKeeper.alternateSimilarTerrains.TryGetValue(primaryTerrainDef, out TerrainDef[] alts))
                            alternates = alts;
                    }
                    break;
            }
            alternatesCached = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref primaryTerrainDef, "terrainDef");
            Scribe_Values.Look(ref altsMode, "altsMode");
        }
    }
}
