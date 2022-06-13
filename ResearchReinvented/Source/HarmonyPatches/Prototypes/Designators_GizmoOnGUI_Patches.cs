using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Extensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.GizmoOnGUI))]
    public static class Designator_Build_GizmoOnGUI_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(GizmoResult __result, Designator_Build __instance, BuildableDef ___entDef, Vector2 topLeft, float maxWidth)
        {
            if (__instance.PlacingDef.IsAvailableOnlyForPrototyping())
            {
                var width = __instance.GetWidth(maxWidth);
                DrawPrototypeLabel(topLeft, width);
            }
        }

        private static Color CyanishTransparentBG = new Color(0.5f, 0.75f, 1f, 0.5f);
        private static Color CyanishTransparent = new Color(0.5f, 1f, 1f, 0.8f);

        internal static void DrawPrototypeLabel(Vector2 topLeft, float width)
        {
            Text.Font = GameFont.Tiny;
            var protoLabel = "RR_PrototypeLabel".Translate();
            var height = Text.CalcHeight(protoLabel, width);
            var labelRect = new Rect(topLeft.x + 2f, topLeft.y + 13f, width - 4f, height);
            GUI.color = CyanishTransparentBG;
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.UpperCenter;
            GUI.color = CyanishTransparent;
            Widgets.Label(labelRect, protoLabel);
            Text.Anchor = TextAnchor.UpperLeft;

            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }
    }

    [HarmonyPatch(typeof(Designator_Dropdown), nameof(Designator_Dropdown.GizmoOnGUI))]
    public static class Designator_Dropdown_GizmoOnGUI_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(GizmoResult __result, Designator_Dropdown __instance, Designator ___activeDesignator, Vector2 topLeft, float maxWidth)
        {
            if (___activeDesignator != null && ___activeDesignator is Designator_Place placeDesignator)
            {
                if (placeDesignator.PlacingDef.IsAvailableOnlyForPrototyping())
                {
                    var width = __instance.GetWidth(maxWidth);
                    Designator_Build_GizmoOnGUI_Patches.DrawPrototypeLabel(topLeft, width);
                }
            }
        }
    }
}
