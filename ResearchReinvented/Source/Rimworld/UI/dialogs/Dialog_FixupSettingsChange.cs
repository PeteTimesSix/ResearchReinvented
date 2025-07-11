using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.UI.Dialogs
{
    public class Dialog_FixupSettingsChange: Dialog_Confirm
    {
        public Dialog_FixupSettingsChange(string title, Action onConfirm) : base(title, onConfirm)
        {
        }
    }
}
