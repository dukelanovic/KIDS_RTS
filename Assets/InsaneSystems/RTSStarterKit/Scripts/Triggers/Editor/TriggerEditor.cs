using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public sealed class TriggerEditor : EditorWindow
	{
		const string editorName = "RTS Trigger Editor";
		
		static TriggerController triggerController;
		static TriggerController[] foundTriggerControllers;

		Vector2 scrollPosition;

		[MenuItem("RTS Starter Kit/Trigger Editor", priority = 1)]
		static void Init()
		{
			var window = (TriggerEditor)GetWindow(typeof(TriggerEditor));
			window.titleContent = new GUIContent(editorName);
			window.minSize = new Vector2(512, 600);
			window.Show();
		}

		void OnGUI()
		{
			GUILayout.BeginVertical(InsaneEditorStyles.Headers["TriggersEditor"]);
			GUILayout.Label(editorName, InsaneEditorStyles.EditorsHeaderTextStyle);
			GUILayout.EndVertical();
			
			if (!triggerController)
			{
				foundTriggerControllers = FindObjectsOfType<TriggerController>();

				if (foundTriggerControllers.Length == 0)
				{
					var triggerControllerObject = new GameObject("TriggerController");
					foundTriggerControllers = new TriggerController[1];
					foundTriggerControllers[0] = triggerControllerObject.AddComponent<TriggerController>();
				}

				triggerController = foundTriggerControllers[0];
			}

			if (foundTriggerControllers.Length > 1)
				EditorGUILayout.HelpBox("Several Trigger Controllers found on scene. Your scene should have only one TriggerController.", MessageType.Warning);

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, InsaneEditorStyles.PaddedBoxStyle);

			var editor = Editor.CreateEditor(triggerController);
			editor.OnInspectorGUI();

			EditorGUILayout.EndScrollView();
		}
	}
}