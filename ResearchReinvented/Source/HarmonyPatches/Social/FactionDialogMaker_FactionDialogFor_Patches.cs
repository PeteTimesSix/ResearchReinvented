using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;
using static HarmonyLib.AccessTools;
using static UnityEngine.GraphicsBuffer;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Social
{

    [HarmonyPatch(typeof(FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor))]
    public static class FactionDialogMaker_FactionDialogFor_Patches
    {
        public static FieldRef<DiaOption, string> optionText;
        public static string disconnectTranslated;

        public static int RequiredGoodwill => 5;

        static FactionDialogMaker_FactionDialogFor_Patches() 
        {
            optionText = AccessTools.FieldRefAccess<DiaOption, string>("text");
            disconnectTranslated = "(" + "Disconnect".Translate() + ")";
        }


        [HarmonyPostfix]
        public static void Postfix(Pawn negotiator, Faction faction, DiaNode __result)
        {

            if (__result.options.Count == 0)
                return; //something weird, dont touch it.

            var map = negotiator.Map;
            if (map == null || !map.IsPlayerHome)
                return; //not home

            var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                   .Where(o => o.def.handledBy.HasFlag(HandlingMode.Social) 
                        && o.requirement is ROComp_RequiresFaction requiresFaction 
                        && requiresFaction.MetByFaction(faction))
                   .Where(o => !o.IsFinished)
            .FirstOrDefault();

            if (opportunity == null)
                return; //no opportunity to learn

            if (faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile)
                return;

            DiaOption askForScience = RequestLectureSimple(map, negotiator, faction, opportunity);

            var disconnectOption = __result.options.LastOrDefault(o => optionText(o) == disconnectTranslated);
            if (disconnectOption != null)
                __result.options.Insert(__result.options.IndexOf(disconnectOption), askForScience);
            else
                __result.options.Add(askForScience);
        }

        public static DiaOption RequestLectureSimple(Map map, Pawn negotiator, Faction faction, ResearchOpportunity opportunity)
        {
            bool allies = faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally;
            TaggedString requestlecture = allies ? 
                "RR_RequestLectureAlly".Translate(opportunity.project.label) : 
                "RR_RequestLectureNeutral".Translate(RequiredGoodwill, opportunity.project.label);

            if (negotiator.WorkTypeIsDisabled(WorkTypeDefOf.Research))
            {
                DiaOption diaOption = new DiaOption(requestlecture);
                diaOption.Disable("RR_RequestLecture_FailNotResearcher".Translate(negotiator.LabelCap));
                return diaOption;
            }

            if(FactionLectureManager.Instance.IsOnCooldown(faction))
            {
                DiaOption diaOption = new DiaOption(requestlecture);
                diaOption.Disable("RR_RequestLecture_FailTooRecent".Translate());
                return diaOption;
            }

            if (!allies && faction.PlayerGoodwill < RequiredGoodwill)
            {
                DiaOption diaOption = new DiaOption(requestlecture);
                diaOption.Disable("NeedGoodwill".Translate(RequiredGoodwill));
                return diaOption;
            }

            return new DiaOption(requestlecture)
            {
                link = new DiaNode("RR_RequestLecture_Reply".Translate(faction.leader, opportunity.project.label).CapitalizeFirst())
                {
                    options =
                    {
                        new DiaOption("Confirm".Translate())
                        {
                            action = () =>
                            {
                                if(!allies)
                                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -RequiredGoodwill);

                                var commsConsole = FindNearestCommsConsole(negotiator, map);
                                //if((negotiator.CurJob?.targetA).HasValue) ?????
                                Job job = JobMaker.MakeJob(JobDefOf_Custom.RR_LearnRemotely, commsConsole);
                                job.commTarget = faction;
                                negotiator.jobs.TryTakeOrderedJob(job);
                            },
                            resolveTree = true
                        }, 
                        new DiaOption("GoBack".Translate())
                        {
                            linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
                        }
                    }
                }
            };
        }

        public static Thing FindNearestCommsConsole(Pawn pawn, Map map)
        {
            var usableConsoles = new List<Building_CommsConsole>();
            foreach (var building in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
            {
                if(building.def.IsCommsConsole && building is Building_CommsConsole commsConsole && commsConsole.CanUseCommsNow)
                    usableConsoles.Add(commsConsole);
            }
            return GenClosest.ClosestThing_Global_Reachable(pawn.PositionHeld, map, usableConsoles, PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), validator: (Thing thing) => pawn.CanReserve(thing));
        }
    }
}
