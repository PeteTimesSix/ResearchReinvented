using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.UI
{
    public static class Widgets_Extra
	{

		private static float TEXT_EPSILON = 1f;
		public static void LabelFitHeightAware(Rect rect, string label, bool withBacking = true)
		{
			GameFont storedFont = Text.Font;

			var words = Words(label);
			int wordCount = words.Count();

			var maxLinesSmall = MaxLinesIn(rect, GameFont.Small);
			var maxLinesTiny = MaxLinesIn(rect, GameFont.Tiny);

			//check if any words cant fit on one line in small font
			if (words.Any(word => CalcSizeInFont(word, GameFont.Small).x + TEXT_EPSILON > rect.width))
			{
				//check if any words cant fit on one line in tiny font
				if (words.Any(word => CalcSizeInFont(word, GameFont.Tiny).x + TEXT_EPSILON > rect.width))
				{
					//check if we can fit on available lines in small font (ignoring word count)
					if (NewLinesNeededFor(rect.width, GameFont.Small, label) <= maxLinesSmall)
					{
						Text.Font = GameFont.Small;
						Widgets.Label(rect, label);
					}
					else
					{
						Text.Font = GameFont.Tiny;
						//check if we can fit on available lines in tiny font (ignoring word count)
						if (NewLinesNeededFor(rect.width, GameFont.Tiny, label) <= maxLinesTiny)
							Widgets.Label(rect, label);
						else
							Widgets.Label(rect, label.Truncate(rect.width * maxLinesTiny));
					}
				}
				else
				{
					Text.Font = GameFont.Tiny;
					//check if we can fit on available lines in tiny font
					if (NewLinesNeededFor(rect.width, GameFont.Tiny, label) <= maxLinesTiny)
						Widgets.Label(rect, label);
					else
						Widgets.Label(rect, label.Truncate(rect.width * maxLinesTiny));
				}
			}
			else
			{
				//check if we can fit on available lines in small font
				if (NewLinesNeededFor(rect.width, GameFont.Small, label) <= maxLinesSmall)
				{
					Text.Font = GameFont.Small;
					Widgets.Label(rect, label);
				}
				else
				{
					Text.Font = GameFont.Tiny;
					//check if we can fit on available lines in tiny font
					if (NewLinesNeededFor(rect.width, GameFont.Tiny, label) <= maxLinesTiny)
						Widgets.Label(rect, label);
					else
						Widgets.Label(rect, label.Truncate(rect.width * maxLinesTiny));
				}
			}

			Text.Font = storedFont;
		}

		private static GUIContent tmpTextGUIContent = new GUIContent();

        private static Vector2 CalcSizeInFont(string text, GameFont font) 
		{
			GUIStyle guiStyle;
			switch (font)
			{
				case GameFont.Tiny:
					guiStyle = Text.fontStyles[0];
					break;
				case GameFont.Small:
					guiStyle = Text.fontStyles[1];
					break;
				case GameFont.Medium:
					guiStyle = Text.fontStyles[2];
					break;
				default:
					throw new NotImplementedException();
			}
			tmpTextGUIContent.text = text.StripTags();
			return guiStyle.CalcSize(tmpTextGUIContent);
		}

		private const float SIDE_OF_CAUTION = 2f;
		public static int NewLinesNeededFor(float rectWidth, GameFont font, string text, bool includeFirst = true) 
		{
			int newLines = 0;
			float widthAccum = 0f;
			float widthAccumSinceLastWhitespace = 0f;
			int lastWhitespaceIndex = -1;
			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];
				var width = CalcSizeInFont(ch.ToString(), GameFont.Small).x;
				widthAccum += width;
				widthAccumSinceLastWhitespace += width;
				if (char.IsWhiteSpace(ch))
				{
					lastWhitespaceIndex = i;
					widthAccumSinceLastWhitespace = 0;
				}
				if (widthAccum + SIDE_OF_CAUTION > rectWidth)
				{
					if (lastWhitespaceIndex != -1 && widthAccumSinceLastWhitespace <= rectWidth)
						widthAccum = widthAccumSinceLastWhitespace;
					else
						widthAccum = 0;
					newLines++;
					lastWhitespaceIndex = -1;
					widthAccumSinceLastWhitespace = 0;
				}
			}
			if (includeFirst)
				newLines++;
			return newLines;
		}

		private static int MaxLinesIn(Rect rect, GameFont font)
		{
			//Log.Message($"mas lines for (gameFont {font.ToString()}): {Math.Max(1, (int)Math.Floor(rect.height / Text.LineHeightOf(font)))}");
			return Math.Max(1, (int)Math.Floor(rect.height / Text.LineHeightOf(font)));
		}

		/*private static int WordCount(string text) 
		{
			int wordCount = 0, index = 0;

			// skip whitespace until first word
			while (index < text.Length && char.IsWhiteSpace(text[index]))
				index++;

			while (index < text.Length)
			{
				// check if current char is part of a word
				while (index < text.Length && !char.IsWhiteSpace(text[index]))
					index++;

				wordCount++;

				// skip whitespace until next word
				while (index < text.Length && char.IsWhiteSpace(text[index]))
					index++;
			}
			return wordCount;
		}*/

		private static IEnumerable<string> Words(string text)
		{
			List<string> words = new List<string>();
			int wordCount = 0;

			int wordStartIndex = -1;
			int index = 0;

			// skip whitespace until first word
			while (index < text.Length && char.IsWhiteSpace(text[index]))
			{
				index++;
			}

			while (index < text.Length)
			{
				wordStartIndex = index;

				// check if current char is part of a word
				while (index < text.Length && !char.IsWhiteSpace(text[index]))
					index++;

				words.Add(text.Substring(wordStartIndex, index - wordStartIndex));
				wordCount++;

				// skip whitespace until next word
				while (index < text.Length && char.IsWhiteSpace(text[index]))
					index++;
			}

			return words;
		}
    }
}
