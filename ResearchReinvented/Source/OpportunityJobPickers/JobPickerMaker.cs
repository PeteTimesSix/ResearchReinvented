using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.OpportunityJobPickers
{
    public static class JobPickerMaker
    {
        private static Dictionary<Type, OpportunityJobPickerBase> builtPickers = new Dictionary<Type, OpportunityJobPickerBase>();

        public static OpportunityJobPickerBase MakePicker(Type pickerType) 
        {
            if(!builtPickers.ContainsKey(pickerType))
            {
                OpportunityJobPickerBase picker = (OpportunityJobPickerBase)Activator.CreateInstance(pickerType);
                builtPickers[pickerType] = picker;
            }
            return builtPickers[pickerType];
        }
    }
}
