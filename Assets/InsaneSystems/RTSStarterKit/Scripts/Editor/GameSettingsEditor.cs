using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class GameSettingsEditor : ScriptableObjectWindow
	{
		const string EditorName = "RTS Game Settings";

		[MenuItem("RTS Starter Kit/Game Settings", priority = 0)]
		static void Init()
		{
			window = (GameSettingsEditor)GetWindow(typeof(GameSettingsEditor));
			window.titleContent = new GUIContent(EditorName);
			window.maxSize = new Vector2(768f, 600f);
			window.minSize = window.maxSize;
			window.Show();
		}
		
		[MenuItem("RTS Starter Kit/Help/Online Guide", priority = 101)]
		static void ShowGuide() => Process.Start("https://docs.google.com/document/d/1jM-qJoNewQ2HnpzaUbwVG6VSX94sXPAGT5YRK15mUDA/edit?usp=sharing");

		[MenuItem("RTS Starter Kit/Help/API Documentation", priority = 102)]
		static void ShowAPIDocumentation() => Process.Start("https://insanesystems.net/APIDocumentation/RtsStarterKit");

		[MenuItem("RTS Starter Kit/Help/Our Discord server", priority = 103)]
		static void ShowDiscord() => Process.Start("https://discord.gg/vguftKf");

		protected override void CustomStartOnGUI()
		{
			GUILayout.BeginVertical(InsaneEditorStyles.Headers["GameSettingsEditor"]);
			GUILayout.Label(EditorName, InsaneEditorStyles.EditorsHeaderTextStyle);
			GUILayout.EndVertical();
		}
		
		protected override void DrawCustomSO() => DrawScriptableObject(loadedStorage);
	}
}