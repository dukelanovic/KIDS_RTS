using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{ 
	public static class InsaneEditorStyles
	{
		public static Color SelectedButtonColor { get; } = new Color(0.7f, 1, 0.7f, 1);
		public static Color SelectedButtonProColor { get; } = new Color(0.3f, 0.6f, 0.3f, 1);

		public static GUIStyle HeaderTextStyle { get; private set; }
		public static GUIStyle HeaderBoldTextStyle { get; private set; }
		public static GUIStyle EditorsHeaderTextStyle { get; private set; }
		public static GUIStyle SmallHeaderTextStyle { get; private set; }
		public static GUIStyle PopupWindowTextStyle { get; private set; }
		
		public static GUIStyle PaddedBoxStyle { get; private set; }
		public static GUIStyle DatasListStyle { get; private set; }
		public static GUIStyle RichTextStyle { get; private set; }
		public static GUIStyle RoundedCornersBoxSimple { get; private set; }
		public static GUIStyle EditorHeader { get; private set; }

		public static Dictionary<string, GUIStyle> Headers { get; } = new Dictionary<string, GUIStyle>();
		
		public static GUIStyle WrappedText { get; private set; }

		static readonly Dictionary<Color, Texture2D> lightGradientsCached = new Dictionary<Color, Texture2D>();

		static InsaneEditorStyles() => Reload();

		public static Color GetSelectedButtonColor() => EditorGUIUtility.isProSkin ? SelectedButtonProColor : SelectedButtonColor;

		public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
		{
			var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			rect.height = thickness;
			rect.y += padding / 2;
			rect.x -= 2;
			rect.width += 6;
			EditorGUI.DrawRect(rect, color); 
		}

		public static void Reload()
		{
			var isDark = EditorGUIUtility.isProSkin;
			
			WrappedText = new GUIStyle();
			WrappedText.wordWrap = true;
			WrappedText.richText = true;
			
			EditorHeader = new GUIStyle();
			EditorHeader.padding = new RectOffset(25, 25, 25, 25);
			
			Headers.Add("SoundEditor", MakeStyleLightGradientBackground(new Color(0.4f, 0.75f, 0.5f, 1f)));
			Headers.Add("TextsEditor", MakeStyleLightGradientBackground(new Color(0.4f, 0.4f, 0.75f, 1f)));
			Headers.Add("UnitEditor", MakeStyleLightGradientBackground(new Color(0.4f, 0.4f, 0.75f, 1f)));
			Headers.Add("AbilitiesEditor", MakeStyleLightGradientBackground(new Color(0.75f, 0.4f, 0.4f, 1f)));
			Headers.Add("AiEditor", MakeStyleLightGradientBackground(new Color(0.75f, 0.3f, 0.5f, 1f)));
			Headers.Add("GameSettingsEditor", MakeStyleLightGradientBackground(new Color32(200, 96, 64, 255)));
			Headers.Add("ProductionCatEditor", MakeStyleLightGradientBackground(new Color32(201, 156, 50, 255)));
			Headers.Add("TriggersEditor", MakeStyleLightGradientBackground(new Color32(149, 155, 159, 255)));
			Headers.Add("FactionsEditor", MakeStyleLightGradientBackground(new Color32(119, 155, 254, 255)));

			RichTextStyle = new GUIStyle
			{
				richText = true
			};

			RoundedCornersBoxSimple = new GUIStyle(EditorStyles.helpBox)
			{
				fontSize = 12,
				wordWrap = true
			};

			var bgColor = isDark ? new Color(0.18f, 0.18f, 0.18f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
			PaddedBoxStyle = new GUIStyle
			{
				padding = new RectOffset(5, 5, 5, 5),
				normal =
				{
					background = EditorExtensions.MakeTex(2, 2, bgColor)
				}
			};

			var bgDataColor = isDark ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);
			
			DatasListStyle = new GUIStyle(PaddedBoxStyle)
			{
				normal =
				{
					background = EditorExtensions.MakeTex(2, 2, bgDataColor)
				}
			};

			HeaderTextStyle = new GUIStyle
			{
				fontSize = 16,
				normal =
				{
					textColor = isDark ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f)
				},
				padding = new RectOffset(3, 3, 5, 5)
			};

			HeaderBoldTextStyle = new GUIStyle(HeaderTextStyle)
			{
				fontStyle = FontStyle.Bold
			};

			EditorsHeaderTextStyle = new GUIStyle
			{
				fontSize = 24,
				normal =
				{
					textColor = isDark ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f)
				}
			};

			SmallHeaderTextStyle = new GUIStyle
			{
				fontSize = 14,
				fontStyle = FontStyle.Bold,
				padding = new RectOffset(3, 3, 5, 5),

				normal =
				{
					textColor = isDark ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f)
				}
			};
			
			PopupWindowTextStyle = new GUIStyle
			{
				fontSize = 14,
				richText = true,
				padding = new RectOffset(3, 3, 10, 10),

				normal =
				{
					textColor = isDark ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f)
				}
			};
		}

		static GUIStyle MakeStyleLightGradientBackground(Color baseColor)
		{
			var style = new GUIStyle(EditorHeader);

			Texture2D lightGradient = null;

			if (lightGradientsCached.ContainsKey(baseColor))
				lightGradientsCached.TryGetValue(baseColor, out lightGradient);

			if (!lightGradient)
			{
				lightGradient = MakeLighterGradient(baseColor);
				lightGradientsCached.Add(baseColor, lightGradient); 
			}

			style.normal.background = lightGradient;

			return style;
		}

		static Texture2D MakeLighterGradient(Color color)
		{
			var colorB = color;
			colorB.r += 0.15f;
			colorB.g += 0.15f;
			colorB.b += 0.15f;
			
			var colors = new List<Color>
			{
				color,
				colorB
			};
			
			return EditorExtensions.MakeGradientTex(colors);
		}
	}
}