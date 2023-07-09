using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Managers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch]
    public static class JobDriver_ConstructFinishFrame_MakeNewToils_Patches
    {
        private static readonly CodeMatch[] toMatch = new CodeMatch[]
        {
            new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.ConstructSuccessChance))),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Ldc_I4_M1),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue))),
            new CodeMatch(OpCodes.Stloc_S)
        };

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> CalculateMethods(Harmony instance)
        {
            var candidates = typeof(JobDriver_ConstructFinishFrame).GetNestedTypes(AccessTools.all).SelectMany(t => AccessTools.GetDeclaredMethods(t));

            foreach (var method in candidates)
            {
                var instructions = PatchProcessor.GetCurrentInstructions(method);
                var matched = new CodeMatcher(instructions).MatchStartForward(toMatch).IsValid;
                if(matched)
                    yield return method;
            }
            yield break;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> JobDriver_ConstructFinishFrame_MakeNewToils_initAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobDriver_ConstructFinishFrame_MakeNewToils_Patches), nameof(PrototypeFailureChanceIncrease)))
            };

            codeMatcher.MatchEndForward(toMatch);
            codeMatcher.Insert(toInsert);
            codeMatcher.End();

            if(codeMatcher.IsInvalid) 
            {
                Log.Warning("RR: failed to apply transpiler on JobDriver_ConstructFinishFrame_MakeNewToils_initAction!");
                return instructions;
            }
            else
                return codeMatcher.InstructionEnumeration();
        }

        public static float PrototypeFailureChanceIncrease(float statValue, Frame frame, Pawn pawn) 
        {
            if(PrototypeKeeper.Instance.IsPrototype(frame))
            {
                float statValueModified = Math.Max(0f, statValue *= 0.5f);
                return statValueModified;
            }
            return statValue;
            //Log.Message($"Beep! {pawn.LabelCap} is building {frame.LabelCap} fail chance stat is: {statValue}, modified: {statValueModified}");
        }
    }
}
