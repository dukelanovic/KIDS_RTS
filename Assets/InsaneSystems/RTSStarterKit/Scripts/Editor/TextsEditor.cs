using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class TextsEditor : ScriptableObjectWindow
	{
		const string EditorName = "RTS Texts Editor";

		static TextsLibrary loadedTextsLibrary;

		[MenuItem("RTS Starter Kit/Texts Editor", priority = 0)]
		static void Init()
		{
			var window = (TextsEditor)GetWindow(typeof(TextsEditor));
			window.titleContent = new GUIContent(EditorName);
			window.maxSize = new Vector2(768f, 512f);
			window.minSize = window.maxSize;
			
			window.Show();
			
			onStorageLoad += window.AdditionalLoadFromStorage;
		}

		protected override void CustomStartOnGUI()
		{
			GUILayout.BeginVertical(InsaneEditorStyles.Headers["TextsEditor"]);
			GUILayout.Label(EditorName, InsaneEditorStyles.EditorsHeaderTextStyle);
			GUILayout.EndVertical();
		}

		protected override void CustomInlineOnGUI()
		{
			EditorGUILayout.HelpBox("This is preview version of the " + EditorName + ". Our team is working on improving visual representation of " + EditorName + " parameters. In next updates we hope to make it look better.", MessageType.Info);

			if (!loadedTextsLibrary && loadedStorage) // todo rts kit check whe AdditionalLoad doesnt work
				loadedTextsLibrary = loadedStorage.textsLibrary;
			
			if (!loadedTextsLibrary)
				EditorGUILayout.HelpBox("No Texts Library setted up in Storage parameters. You should create at least one and add it to the Storage settings.", MessageType.Warning);
		}

		protected override void DrawCustomSO() => DrawScriptableObject(loadedTextsLibrary);

		void AdditionalLoadFromStorage(Storage storage) => loadedTextsLibrary = loadedStorage.textsLibrary;
	}
}