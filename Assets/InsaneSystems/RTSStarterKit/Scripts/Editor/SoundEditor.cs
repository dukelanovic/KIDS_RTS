using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class SoundEditor : ScriptableObjectWindow
	{
		const string EditorName = "RTS Sound Editor";
		
		static SoundLibrary loadedSoundLibrary;

		[MenuItem("RTS Starter Kit/Sound Editor", priority = 0)]
		static void Init()
		{
			var window = (SoundEditor)GetWindow(typeof(SoundEditor));
			window.titleContent = new GUIContent(EditorName);
			window.minSize = new Vector2(768f, 512f);
			window.maxSize = new Vector2(768f, 2048f);
			
			window.Show();
			
			onStorageLoad += window.AdditionalLoadFromStorage;
		}
		
		protected override void CustomStartOnGUI()
		{
			GUILayout.BeginVertical(InsaneEditorStyles.Headers["SoundEditor"]);
			GUILayout.Label(EditorName, InsaneEditorStyles.EditorsHeaderTextStyle);
			GUILayout.EndVertical();
		}

		protected override void CustomInlineOnGUI()
		{
			EditorGUILayout.HelpBox("This is preview version of the " + EditorName + ". Our team is working on improving visual representation of " + EditorName + " parameters. In next updates we hope to make it look better.", MessageType.Info);
			
			if (!loadedSoundLibrary && loadedStorage) // todo rts kit check whe AdditionalLoad doesnt work
				loadedSoundLibrary = loadedStorage.soundLibrary;
			
			if (!loadedSoundLibrary)
				EditorGUILayout.HelpBox("No Sound Library setted up in Storage parameters. You should create at least one and add it to the Storage settings.", MessageType.Warning);
		}

		protected override void DrawCustomSO()
		{
			DrawScriptableObject(loadedSoundLibrary);
		}

		void AdditionalLoadFromStorage(Storage storage)
		{
			loadedSoundLibrary = loadedStorage.soundLibrary;
		}
	}
}