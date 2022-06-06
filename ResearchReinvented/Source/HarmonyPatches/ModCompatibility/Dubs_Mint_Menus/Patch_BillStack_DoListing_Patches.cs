using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using UnityEngine;
using PeteTimesSix.ResearchReinvented.Extensions;
using System.Reflection.Emit;
using PeteTimesSix.ResearchReinvented.Utilities;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility.Dubs_Mint_Menus
{
    public static class Patch_BillStack_DoListing_Patches 
    {
        //Ill get back to this probably maybe

        /*
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            //search for !this.researchPrerequisite.IsFinished
            var finally_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(IDisposable), "Dispose")),
                new CodeInstruction(OpCodes.Endfinally),
            };

            var add_prototypes_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_BillStack_DoListing_Patches), nameof(Patch_BillStack_DoListing_Patches.AddPrototypeRows))),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, finally_instructions, out CodeInstruction[] matchedInstructions, out bool found);
            foreach (var instruction in iteratedOver)
            {
                yield return instruction;
            }

            if (!found)
            {
                Log.Warning("failed to apply patch (instructions not found)");
                goto finalize;
            }
            else
            {
                foreach (var checkInstruction in add_prototypes_instructions)
                    yield return checkInstruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static void AddPrototypeRows()
        {
            throw new NotImplementedException();
        }*/
    }
}
