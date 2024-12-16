using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class MapSettingsCheckerWindow : EditorWindow
	{
		readonly List<string> warnings = new List<string>();
		bool wasCheckRun;

		[MenuItem("RTS Starter Kit/Map Settings Checker")]
		public static void ShowWindow()
		{
			var window = GetWindow(typeof(MapSettingsCheckerWindow), false, "Map Checker");
			window.minSize = new Vector2(400, 320);
		}

		void OnGUI()
		{
			EditorGUILayout.HelpBox(@"Map Settings Checker allows you to check if the settings of your Scene (game Map) are set correctly It detects obvious problems and reports them. It checks the opened Scene. 

It is important to understand that it cannot detect all possible problems.", MessageType.None);

			if (GUILayout.Button("Run map check"))
				RunCheck();
			
			if (warnings.Count > 0)
			{
				for (int i = 0; i < warnings.Count; i++)
					EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
			}
			else if (wasCheckRun)
			{
				EditorGUILayout.HelpBox("Looks like your level setted up correctly.", MessageType.Info);
			}
		}
		
		void RunCheck()
		{
			warnings.Clear();
			
			var gameController = FindObjectOfType<GameController>();
			var playerStartPoints = FindObjectsOfType<PlayerStartPoint>();
			
			if (!gameController)
			{
				AddWarning("No GameController found on scene. Do you added SceneBase prefab?");
			}
			else
			{
				if (!gameController.MainStorage)
					AddWarning("GameController has no Storage added. Please, add Storage object from resources, otherwise game will not work correctly.");
				if (!gameController.MapSettings)
					AddWarning("GameController has no MapSettings added. Please, add this map MapSettings asset from resources, otherwise game can work wrong.");
			}

			if (playerStartPoints.Length == 0)
				AddWarning("No PlayerStartPoint object found on scene. Don't forget at least one, otherwise player will be not able to start the game on this map. Minimum points count, required for map to work fine - 2.");

			var sceneObjects = GetSceneObjects();
			var terrainObjects = GetObjectsInLayer(sceneObjects, LayerMask.NameToLayer("Terrain"));

			if (terrainObjects.Count == 0)
				AddWarning("Level has no objects in Terrain layer. Level ground object should have this layer to allow player create buildings.");

			wasCheckRun = true;
		}

		void AddWarning(string warning)
		{
			warnings.Add(warning);
		}

		static List<GameObject> GetObjectsInLayer(List<GameObject> inputObjects, int layer)
		{
			var objectsInLayer = new List<GameObject>();
			foreach (var obj in inputObjects)
				if (obj.gameObject.layer == layer)
					objectsInLayer.Add(obj.gameObject);

			return objectsInLayer;
		}

		static List<GameObject> GetSceneObjects()
		{
			return new List<GameObject>(FindObjectsOfType<GameObject>()).FindAll(go => go.hideFlags == HideFlags.None);
		}
	}
}