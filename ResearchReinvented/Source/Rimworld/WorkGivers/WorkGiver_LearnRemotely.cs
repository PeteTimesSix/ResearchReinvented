using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using PeteTimesSix.ResearchReinvented.Rimworld.Jobs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static System.Collections.Specialized.BitVector32;

namespace PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers
{
	public class WorkGiver_LearnRemotely : WorkGiver_Scanner
	{
		public static Type DriverClass = typeof(JobDriver_LearnRemotely);

		private static ResearchProjectDef _matchingOpportunitiesCachedFor;
		private static ResearchOpportunity[] _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		public static IEnumerable<ResearchOpportunity> MatchingOpportunities
		{
			get
			{
				if (_matchingOpportunitiesCachedFor != Find.ResearchManager.currentProj)
				{
                    _matchingOpportunitesCache = ResearchOpportunityManager.Instance
                        .GetFilteredOpportunities(null, HandlingMode.Social, (op) => op.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.faction != Faction.OfPlayer).ToArray();
                        //.GetCurrentlyAvailableOpportunities(true)
						//.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Social) && o.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.faction != Faction.OfPlayer).ToArray();
                    _matchingOpportunitiesCachedFor = Find.ResearchManager.currentProj;
				}
				return _matchingOpportunitesCache;
			}
		}
		public static void ClearMatchingOpportunityCache()
		{
			_matchingOpportunitiesCachedFor = null;
			_matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		}

        private static ThingDef[] _commsConsoles;
        public static ThingDef[] CommsConsoles
        {
            get
            {
                if(_commsConsoles == null)
                {
                    List<ThingDef> consoles = new List<ThingDef>();
                    foreach(var def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if(def.IsCommsConsole)
                            consoles.Add(def);
                    }
                    _commsConsoles = consoles.ToArray();
                }
                return _commsConsoles;
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if(pawn.Map == null || CommsConsoles.Length == 0)
                return Enumerable.Empty<Thing>();

            List<Thing> consoles = new List<Thing>();
            foreach(var def in CommsConsoles)
            {
                consoles.AddRange(pawn.Map?.listerThings.ThingsOfDef(def));
            }
            return consoles;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (Find.ResearchManager.currentProj == null)
                return true;

            return !MatchingOpportunities.Any(o => !o.IsFinished);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (thing.IsForbidden(pawn))
                return false;

            if (!pawn.CanReserve(thing, 1, -1, null, forced))
                return false;

            if (!(thing is Building_CommsConsole commsConsole))
                return false;

            if (!commsConsole.CanUseCommsNow)
                return false;

            if (OpportunityCache == null)
                return false;

            if (!(pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced) &&
                new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job()))
                return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            var opportunity = OpportunityCache;

            JobDef jobDef = JobDefOf_Custom.RR_LearnRemotely;
            Job job = JobMaker.MakeJob(jobDef, thing, expiryInterval: 3000, checkOverrideOnExpiry: true);
            job.commTarget = (opportunity.requirement as ROComp_RequiresFaction).faction;
            return job;
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			return t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
		}

        //cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
        private static int cacheBuiltOnTick = -1;
        private static ResearchOpportunity _opportunityCache;

        public static ResearchOpportunity OpportunityCache
        {
            get
            {
                if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
                {
                    _opportunityCache = MatchingOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available 
                            && o.requirement is ROComp_RequiresFaction requiresFaction 
                            && !FactionLectureManager.Instance.IsOnCooldown(requiresFaction.faction)
                            && requiresFaction?.faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally)
                        .OrderByDescending(o => o.MaximumProgress)
                        .FirstOrDefault();
                }
                return _opportunityCache;
            }
        }
    }
}
