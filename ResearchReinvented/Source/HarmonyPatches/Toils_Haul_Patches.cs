using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    /*public static class Toils_Haul_Patches
    {
        //[HarmonyPatch(typeof(Toils_Haul), nameof(Toils_Haul.PlaceHauledThingInCell))]
        public static class Toils_Haul_PlaceHauledThingInCell_Transpiler
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                JobDef recolorApprarelDef = JobDefOf.RecolorApparel;
                Log.Warning($"jobDef: {recolorApprarelDef}");

                var found = false;
                var codes = instructions.ToList();
                CodeInstruction[] buffer = new CodeInstruction[3];
                for (int i = 0; i < codes.Count; i++)
                {
                    if (found)
                    {
                        yield return codes[i];
                    }
                    else{
                        if(buffer[0] != null)
                            yield return buffer[0];

                        buffer[0] = buffer[1];
                        buffer[1] = buffer[2];
                        buffer[2] = codes[i];
                        if (i > 2 && buffer[0].opcode == OpCodes.Ldfld && buffer[1].opcode == OpCodes.Ldfld && (buffer[2].opcode == OpCodes.Ldsfld && ((buffer[2]?.operand as FieldInfo)?.IsStatic ?? false) && (buffer[2]?.operand as FieldInfo)?.GetValue(null) == recolorApprarelDef))
                        {
                            Log.Warning($"found check for recolorApparel jobDefOf");
                            found = true;

                            Log.Warning($"{buffer[0].opcode} {buffer[0].operand}");
                            yield return buffer[0];

                            Log.Warning($"{buffer[1].opcode} {buffer[1].operand}");
                            yield return buffer[1];

                            MethodInfo method_IsNonVanillaPlacedThingJob = typeof(Toils_Haul_PlaceHauledThingInCell_Transpiler).GetMethod(nameof(Toils_Haul_PlaceHauledThingInCell_Transpiler.IsNonVanillaPlacedThingJob));
                            var callToModdedCheck = new CodeInstruction(OpCodes.Call, method_IsNonVanillaPlacedThingJob);
                            Log.Warning($"replacing {buffer[2].opcode} {buffer[2].operand} with {callToModdedCheck.opcode} {callToModdedCheck.operand}");
                            yield return callToModdedCheck;
                            //Log.Warning($"{buffer[2].opcode} {buffer[2].operand}");
                            //yield return buffer[2];

                            i++;
                            CodeInstruction branchInstruction = codes[i];
                            var newJump = new CodeInstruction(OpCodes.Brtrue_S, branchInstruction.operand);
                            Log.Warning($"replacing {branchInstruction.opcode} {branchInstruction.operand} with {newJump.opcode} {newJump.operand}");
                            yield return newJump;
                            //yield return branchInstruction;
                        }
                    }
                }
            }
            public static int IsNonVanillaPlacedThingJob(JobDef def)
            {
                Log.Warning($"Transpiled IsNonVanillaPlacedThingJob, got {def}");
                return 0;
            }
        }

        public static void DoPatch(Harmony harmony)
        {
            harmony.Patch(typeof(Toils_Haul).
                GetNestedType("<>c__DisplayClass6_0", BindingFlags.NonPublic | BindingFlags.Instance).
                GetMethod("<PlaceHauledThingInCell>b__0", BindingFlags.NonPublic | BindingFlags.Instance),
                transpiler: new HarmonyMethod(typeof(Toils_Haul_PlaceHauledThingInCell_Transpiler).GetMethod(nameof(Toils_Haul_PlaceHauledThingInCell_Transpiler.Transpiler))));
        }
    }*/
}
