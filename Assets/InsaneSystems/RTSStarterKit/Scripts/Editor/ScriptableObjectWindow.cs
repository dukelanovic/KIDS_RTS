using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class ScriptableObjectWindow : ExtendedEditorWindow
	{
		const string StoragePrefsId = "EditorSelectedStorage";
		
		public static ScriptableObjectWindow window;
		protected static Action<Storage> onStorageLoad;
		
		protected static int storageDatasFoundCount;
		protected static Storage loadedStorage;
		
		protected Vector2 dataEditorScrollPos;

		void OnGUI()
		{
			Draw();
		}

		public override void Draw()
		{
			CustomStartOnGUI();
			
			GUILayout.BeginVertical(InsaneEditorStyles.PaddedBoxStyle);
			CustomInlineOnGUI();
			
			CheckStorage();
			
			GUILayout.EndVertical();
		}


		protected virtual void CustomStartOnGUI() { }
		protected virtual void CustomInlineOnGUI() { }

		void CheckStorage()
		{
			if (storageDatasFoundCount > 1)
			{
				EditorGUILayout.HelpBox("Found several Storages, it can cause showing of wrong settings in this window. It is recommended to have only one game Storage.", MessageType.Warning);

				SetStorage(EditorGUILayout.ObjectField("Select correct storage", loadedStorage, typeof(Storage), false) as Storage);

				if (loadedStorage != null)
					GUILayout.Label("Using storage <b>" + loadedStorage.name + "</b>", InsaneEditorStyles.RichTextStyle);
			}

			if (storageDatasFoundCount > 0 && loadedStorage)
			{
				DrawCustomSO();
			}
			else if (storageDatasFoundCount == 0)
			{
				EditorGUILayout.HelpBox("No Storage found. You should create at least one, otherwise game will not work.", MessageType.Warning);
				
				if (GUILayout.Button("Refresh"))
					Load();
			}
		}

		protected virtual void DrawCustomSO() { }

		protected void DrawScriptableObject(ScriptableObject so)
		{
			if (!so)
				return;
			
			dataEditorScrollPos = EditorGUILayout.BeginScrollView(dataEditorScrollPos);
			var soEditor = Editor.CreateEditor(so);
			soEditor.OnInspectorGUI();
			EditorGUILayout.EndScrollView();
		}

		static void SetStorage(Storage storage)
		{
			loadedStorage = storage;
			
			PlayerPrefs.SetInt(StoragePrefsId, storage.GetInstanceID());
			
			if (loadedStorage && onStorageLoad != null)
				onStorageLoad.Invoke(loadedStorage);
		}
		
		/// <summary>Loads Storage from game resources. This method is can be used only in the Unity Editor, not in the build.</summary>
		public static Storage GetStorage()
		{
			if (!loadedStorage)
				Load();
			
			return loadedStorage;
		}

		public static void Load()
		{
			var storages = new List<Storage>();

			EditorExtensions.LoadAssetsToList(storages, "t:storage");

			if (storages.Count == 1)
			{
				SetStorage(storages[0]);
			}
			else if (PlayerPrefs.HasKey(StoragePrefsId))
			{
				var id = PlayerPrefs.GetInt(StoragePrefsId);

				for (var i = 0; i < storages.Count; i++)
				{
					if (storages[i].GetInstanceID() == id)
					{
						SetStorage(storages[i]);
						break;
					}
				}
			}
			
			storageDatasFoundCount = storages.Count;
		}
	}
}