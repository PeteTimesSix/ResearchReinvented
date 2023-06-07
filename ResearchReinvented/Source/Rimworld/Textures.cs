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
		public static readonly Texture2D medicalInvasiveIcon = ContentFinder<Texture2D>.Get("UI/icons/medical", true);
        public static readonly Texture2D medicalNoninvasiveIcon = ContentFinder<Texture2D>.Get("UI/icons/pills", true);
        public static readonly Texture2D genericIcon = ContentFinder<Texture2D>.Get("UI/icons/question", true);
        public static readonly Texture2D genericPawnIcon = ContentFinder<Texture2D>.Get("UI/icons/questionPawn", true);
        public static readonly Texture2D corpseIconOverlay = ContentFinder<Texture2D>.Get("UI/icons/corpseOverlay", true);


        //set in defs
        /*public static readonly Texture2D booksHintIcon = ContentFinder<Texture2D>.Get("UI/hintIcons/books", true);
        public static readonly Texture2D cogHintIcon = ContentFinder<Texture2D>.Get("UI/hintIcons/cog", true);
        public static readonly Texture2D foodHintIcon = ContentFinder<Texture2D>.Get("UI/hintIcons/food", true);
        public static readonly Texture2D pillsHintIcon = ContentFinder<Texture2D>.Get("UI/hintIcons/pills", true);*/

        public static readonly Texture2D forbiddenOverlayTex = ContentFinder<Texture2D>.Get("Things/Special/ForbiddenOverlay");

    }
}
