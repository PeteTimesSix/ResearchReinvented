using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PeteTimesSix.ResearchReinvented.Utilities.CustomWidgets
{
	enum RangeEnd : byte
	{
		None,
		Min,
		Max
	}

	[StaticConstructorOnStartup]
	public static class CustomFloatRange
	{
		private static readonly Color RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);
		private static readonly Texture2D FloatRangeSliderTex = ContentFinder<Texture2D>.Get("UI/Widgets/RangeSlider", true);
		private static RangeEnd curDragEnd = RangeEnd.None;
		private static int draggingId = 0;
		private static float lastDragSliderSoundTime = -1f;

		public static void FloatRange(Rect fullRect, int id, ref FloatRange range, float min = 0f, float max = 1f, float roundTo = -1f)
		{
			var rect = fullRect;
			rect.x += 8f;
			rect.width -= 16f;
			var barRect = rect.BottomHalf().TopPartPixels(2f);
			barRect.y -= 1f;
			barRect.width += 2f;
			GUI.color = RangeControlTextColor;
			GUI.DrawTexture(barRect, BaseContent.WhiteTex);
			GUI.color = Color.white;
			float num = rect.x + rect.width * Mathf.InverseLerp(min, max, range.min);
			float num2 = rect.x + rect.width * Mathf.InverseLerp(min, max, range.max);
			Rect rect5 = new Rect(num - 16f, rect.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(rect5, FloatRangeSliderTex);
			Rect rect6 = new Rect(num2 + 16f, rect.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(rect6, FloatRangeSliderTex);
			if (curDragEnd != RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
			{
				draggingId = 0;
				curDragEnd = RangeEnd.None;
				SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
			}
			bool flag = false;
			if (Mouse.IsOver(fullRect) || draggingId == id)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != draggingId)
				{
					draggingId = id;
					float x = Event.current.mousePosition.x;
					if (x < rect5.xMax)
					{
						curDragEnd = RangeEnd.Min;
					}
					else if (x > rect6.xMin)
					{
						curDragEnd = RangeEnd.Max;
					}
					else
					{
						float num3 = Mathf.Abs(x - rect5.xMax);
						float num4 = Mathf.Abs(x - (rect6.x - 16f));
						curDragEnd = ((num3 < num4) ? RangeEnd.Min : RangeEnd.Max);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
				}
				if (flag || (curDragEnd != RangeEnd.None && Event.current.type == EventType.MouseDrag))
				{
					float num5 = (Event.current.mousePosition.x - rect.x) / rect.width * (max - min) + min;

					if (roundTo > 0f)
					{
						num5 = (float)Mathf.RoundToInt(num5 / roundTo) * roundTo;
					}
					num5 = Mathf.Clamp(num5, min, max);
					if (curDragEnd == RangeEnd.Min)
					{
						if (num5 != range.min)
						{
							range.min = num5;
							if (range.max < range.min)
							{
								range.max = range.min;
							}
							CheckPlayDragSliderSound();
						}
					}
					else if (curDragEnd == RangeEnd.Max && num5 != range.max)
					{
						range.max = num5;
						if (range.min > range.max)
						{
							range.min = range.max;
						}
						CheckPlayDragSliderSound();
					}
					Event.current.Use();
				}
			}
		}
		private static void CheckPlayDragSliderSound()
		{
			if (Time.realtimeSinceStartup > lastDragSliderSoundTime + 0.075f)
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
				lastDragSliderSoundTime = Time.realtimeSinceStartup;
			}
		}
	}
}
