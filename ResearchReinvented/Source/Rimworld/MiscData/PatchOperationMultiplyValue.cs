using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.MiscData
{
	public class PatchOperationMultiplyNumericValue : PatchOperationPathed
	{
		public float? multiplier;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			if (!multiplier.HasValue)
				return false;
			bool result = false;
			var nodes = xml.SelectNodes(xpath);
			if (nodes.Count > 0)
				result = true;
			foreach (object obj in nodes)
			{
				result = true;
				XmlNode xmlNode = obj as XmlNode;
				if (xmlNode.Value == null)
					result = false;
				string sValue = xmlNode.Value;
				bool parseableAsInt = int.TryParse(sValue, out int iValue);
				if (parseableAsInt)
				{
					iValue = (int)(iValue * multiplier.Value);
					xmlNode.Value = iValue.ToString();
				}
				else
				{
					bool parseableAsFloat = float.TryParse(sValue, out float fValue);
					if (parseableAsFloat)
					{
						fValue *= multiplier.Value;
						xmlNode.Value = fValue.ToString();
					}
					else
						result = false;
				}
			}
			return result;
		}
	}
}
