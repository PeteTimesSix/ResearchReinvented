using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.AccessTools;
using static HarmonyLib.Code;
using static System.Net.Mime.MediaTypeNames;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Books
{
    public static class ReadingOutcomeDoerGainResearch_Proxies
    {
        public delegate bool IsProjectVisibleDelegate(ReadingOutcomeDoerGainResearch comp, ResearchProjectDef project);

        public static FieldRef<ReadingOutcomeDoerGainResearch, Dictionary<ResearchProjectDef, float>> Values { get; set; }
        public static IsProjectVisibleDelegate IsProjectVisible { get; set; }

        static ReadingOutcomeDoerGainResearch_Proxies()
        {
            Values = AccessTools.FieldRefAccess<Dictionary<ResearchProjectDef, float>>(typeof(ReadingOutcomeDoerGainResearch), "values");
            IsProjectVisible = AccessTools.MethodDelegate<IsProjectVisibleDelegate>(AccessTools.Method(typeof(ReadingOutcomeDoerGainResearch), "IsProjectVisible"));
        }
    }

    [HarmonyPatch(typeof(ReadingOutcomeDoerGainResearch), nameof(ReadingOutcomeDoerGainResearch.DoesProvidesOutcome))]
    public static class ReadingOutcomeDoerGainResearch_DoesProvidesOutcome_Patches
    {

        [HarmonyPrefix]
        public static bool ReadingOutcomeDoerGainResearch_DoesProvidesOutcome_Prefix(ReadingOutcomeDoerGainResearch __instance, ref bool __result, Pawn reader)
        {
            if (__instance is ReadingOutcomeDoerGainAnomalyResearch)
            {
                return true;
            }

            var values = ReadingOutcomeDoerGainResearch_Proxies.Values(__instance);

            foreach (var (project, speed) in values)
            {
                var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfProjects(values.Keys, OpportunityAvailability.Available, HandlingMode.Special_Books, (op) => op.requirement.MetBy(project)).FirstOrDefault();
                if(opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {reader.Label} wants to read book {__instance.Parent.Label} and there's opportunity {opportunity}");

                    __result = true;
                    return false;
                }
            }
            __result = false;
            return false;
        }
    }


    [HarmonyPatch(typeof(ReadingOutcomeDoerGainResearch), nameof(ReadingOutcomeDoerGainResearch.OnReadingTick))]
    public static class ReadingOutcomeDoerGainResearch_OnReadingTick_Patches
    {
        [HarmonyPrefix]
        public static bool ReadingOutcomeDoerGainResearch_OnReadingTick_Prefix(ReadingOutcomeDoerGainResearch __instance, Pawn reader, float factor)
        {
            if (__instance is ReadingOutcomeDoerGainAnomalyResearch)
            {
                return true;
            }

            var values = ReadingOutcomeDoerGainResearch_Proxies.Values(__instance);

            foreach (var (project, speed) in values)
            {
                if (!ReadingOutcomeDoerGainResearch_Proxies.IsProjectVisible(__instance, project) || project.IsFinished)
                    continue;

                var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfProjects(values.Keys, OpportunityAvailability.Available, HandlingMode.Special_Books, (op) => op.requirement.MetBy(project)).FirstOrDefault();
                if (opportunity != null)
                {
                    opportunity.ResearchTickPerformed(speed * factor, reader);
                }
            }

            return false;
        }
    }


    [HarmonyPatch(typeof(ReadingOutcomeDoerGainResearch), nameof(ReadingOutcomeDoerGainResearch.GetBenefitsString))]
    public static class ReadingOutcomeDoerGainResearch_GetBenefitsString_Patches
    {

        [HarmonyPrefix]
        public static bool ReadingOutcomeDoerGainResearch_GetBenefitsString_Prefix(ReadingOutcomeDoerGainResearch __instance, ref string __result, Pawn reader)
        {
            if (__instance is ReadingOutcomeDoerGainAnomalyResearch)
            {
                return true;
            }

            var values = ReadingOutcomeDoerGainResearch_Proxies.Values(__instance);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var (project, speed) in values)
            {
                float amount = speed * 2500f;
                if (__instance.RoundTo != 0)
                {
                    amount = Mathf.Round(amount / (float)__instance.RoundTo) * (float)__instance.RoundTo;
                }

                string text;
                if(project.IsFinished)
                {
                    text = string.Format($" - {project.LabelCap}: {"PerHour".Translate(amount.ToStringDecimalIfSmall())} ({"AlreadyUnlocked".Translate()})");
                    text = text.Colorize(Color.gray);
                }
                else
                {
                    var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfProjects(values.Keys, OpportunityAvailability.Available, HandlingMode.Special_Books, (op) => op.requirement.MetBy(project)).FirstOrDefault();
                    if (opportunity == null)
                    {
                        text = string.Format($" - {project.LabelCap}: {"PerHour".Translate(amount.ToStringDecimalIfSmall())} ({"RR_CategoryFinishedBook".Translate()})");
                        text = text.Colorize(Color.gray);
                    }
                    else
                    {
                        amount *= opportunity.def.GetCategory(opportunity.relation).Settings.researchSpeedMultiplier;
                        if (!ReadingOutcomeDoerGainResearch_Proxies.IsProjectVisible(__instance, project))
                        {
                            text = string.Format($" - {project.LabelCap}: {"PerHour".Translate(amount.ToStringDecimalIfSmall())} ({"WhenDiscovered".Translate()})");
                            text = text.Colorize(Color.gray);
                        }
                        else
                        {
                            text = string.Format($" - {project.LabelCap}: {"PerHour".Translate(amount.ToStringDecimalIfSmall())}");
                        }
                    }
                }

                stringBuilder.AppendLine(text);
            }
            __result = stringBuilder.ToString();
            return false;
        }
    }
}
