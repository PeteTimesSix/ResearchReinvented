using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        public static readonly Texture2D translucentWhite = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.5f));
        public static readonly Texture2D mostlyTransparentWhite = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));

        public static readonly Texture2D scienceIcon = ContentFinder<Texture2D>.Get("scienceIcon", true);
        public static readonly Texture2D scienceIconDark = ContentFinder<Texture2D>.Get("scienceIconDark", true);

        public static readonly Texture2D forbiddenOverlayTex = ContentFinder<Texture2D>.Get("Things/Special/ForbiddenOverlay");

        public static readonly Texture2D handlerMode_theory = ContentFinder<Texture2D>.Get("UI/handlerModeIcons/theory");
        public static readonly Texture2D handlerMode_analysis = ContentFinder<Texture2D>.Get("UI/handlerModeIcons/analysis");
        public static readonly Texture2D handlerMode_ingestible = ContentFinder<Texture2D>.Get("UI/handlerModeIcons/food");
        public static readonly Texture2D handlerMode_ingestibleObserved = ContentFinder<Texture2D>.Get("UI/handlerModeIcons/med_watched");
        public static readonly Texture2D handlerMode_prototype = ContentFinder<Texture2D>.Get("UI/handlerModeIcons/cog");

    }
}
