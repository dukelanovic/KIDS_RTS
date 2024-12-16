using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class GameSettingsCheckerWindow : EditorWindow
	{
		readonly List<string> warnings = new List<string>();
		Vector2 scrollPos;
		bool wasCheckRun;
		
		[MenuItem("RTS Starter Kit/Game Settings Checker")]
		public static void ShowWindow()
		{
			var window = GetWindow(typeof(GameSettingsCheckerWindow), false, "Game Settings Checker");
			window.minSize = new Vector2(400, 320);
		} 

		void OnGUI()
		{
			EditorGUILayout.HelpBox(@"The Game Settings Checker allows you to check if the game settings and units are set correctly. It reveals obvious problems and reports them.

It is important to understand that it cannot detect all possible problems.", MessageType.None);
			
			if (GUILayout.Button("Run game settings check"))
				RunCheck();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			
			if (warnings.Count > 0)
			{
				for (int i = 0; i < warnings.Count; i++)
					EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
			}
			else if (wasCheckRun)
			{
				EditorGUILayout.HelpBox("Problems were not found.", MessageType.Info);
			}
			
			EditorGUILayout.EndScrollView();
		}

		void RunCheck()
		{
			warnings.Clear();
			
			var storages = FindAssetsByType<Storage>();
			
			if (storages.Count > 2)
				AddWarning("You have more than 2 Storage assets! You can have only ONE Storage asset.");

			if (storages.Count == 1)
			{
				var storage = storages[0];
				
				if (storage.defaultWinCondition == null)
					AddWarning("No win condition is set in the Game Settings (Storage)! It will cause problems with game match finish.");
				
				if (storage.AiControllerPrefab == null)
					AddWarning("No AI Controller Prefab is set in the Game Settings (Storage)! It will cause problems with game match using AIs opponents.");
			}
			else if (storages.Count == 0)
			{
				AddWarning("You have No Storage asset in the project! You should create new Storage for your game.");
			}

			var units = FindAssetsByType<UnitData>();

			foreach (var unitData in units)
				CheckUnit(unitData);
			
			wasCheckRun = true;
		}

		void CheckUnit(UnitData data)
		{
			if (data.isBuilding)
			{
				if (!data.drawerObject)
					AddWarning($"{data.name} is building, but have no Drawer Object setup. It can cause building problems.");
			}
			
			if (!data.selfPrefab)
				AddWarning($"{data.name} has no Self Prefab set. It can cause spawn problems.");
			
			if (data.hasAttackModule && data.attackType == UnitData.AttackType.Shell && !data.attackShell)
				AddWarning($"{data.name} has Attack module and its attack type set to Shell. But Attack Shell prefab is not set. It can cause attack problems.");
			
			if (data.isProduction && (data.productionCategories == null || data.productionCategories.Count == 0))
				AddWarning($"{data.name} is Production, but its Production Categories is not set. It can cause units/buildings production problems.");

		}

		void AddWarning(string warning)
		{
			warnings.Add(warning);
		}
		
		public static List<T> FindAssetsByType<T>() where T : Object
		{
			var assets = new List<T>();
			var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
			
			for(int i = 0; i < guids.Length; i++)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				
				if(asset != null)
					assets.Add(asset);
			}
			
			return assets;
		}
	}
}