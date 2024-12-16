#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{ 
	public static class EditorExtensions
	{
		public static void LoadAssetsToList<T>(List<T> listToAddIn, string searchFilter) where T : Object
		{
			listToAddIn.Clear();
            
			var assets = AssetDatabase.FindAssets(searchFilter);

			for (int i = 0; i < assets.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(T)) as T;
				listToAddIn.Add(asset);
			}
		}
		
		public static Texture2D MakeTex(int width, int height, Color col)
		{
			var pixels = new Color[width * height];
 
			for(int i = 0; i < pixels.Length; i++)
				pixels[i] = col;
 
			var result = new Texture2D(width, height);
			result.SetPixels(pixels);
			result.Apply();
 
			return result;
		}
		
		public static Texture2D MakeGradientTex(List<Color> colors, int widthResolution = 64)
		{
			var pixels = new Color[widthResolution];
			
			var length = colors.Count;
			
			var colorKeys = new GradientColorKey[length];
			var alphaKeys = new GradientAlphaKey[length];
			
			var steps = length - 1f;
			for (int i = 0; i < length; i++)
			{
				var step = i / steps;
				colorKeys[i].color = colors[i];
				colorKeys[i].time = step;
				alphaKeys[i].alpha = colors[i].a;
				alphaKeys[i].time = step;
			}
			
			var gradient = new Gradient();
			gradient.SetKeys(colorKeys, alphaKeys);
			
			for (int i = 0; i < widthResolution; i++)
				pixels[i] =  gradient.Evaluate((float)i / widthResolution);

			var result = new Texture2D(widthResolution, 1)
			{
				filterMode = FilterMode.Point
			};
			
			result.SetPixels(pixels);
			result.Apply();
 
			return result;
		}
	}
}
#endif