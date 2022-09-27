using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class UIComponents
    {
        private static readonly Color BadValueOutlineColor = new Color(.9f, .1f, .1f, 1f);


        public static void DrawBadTextValueOutline(Rect rect)
        {
            var prevColor = GUI.color;
            GUI.color = BadValueOutlineColor;
            Widgets.DrawBox(rect);
            GUI.color = prevColor;
        }
    }
}
