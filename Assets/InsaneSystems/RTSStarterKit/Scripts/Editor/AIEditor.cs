using UnityEditor;
using UnityEngine;
using InsaneSystems.RTSStarterKit.AI;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class AIEditor : DataListEditor
	{
		[MenuItem("RTS Starter Kit/AI Editor", priority = 2)]
		static void Init()
		{
			var window = (AIEditor)GetWindow(typeof(AIEditor));
			window.InitLoad();

			window.titleContent = new GUIContent(window.editorName);
			window.minSize = new Vector2(1024f, 600f);
			window.maxSize = new Vector2(1024f, 2048f);
			
			window.Show();
		}

		public override void InitLoad()
		{
			InitialSetup("RTS AI Editor", "AI", "aisettings", "AIPreset");
			StylesSetup(InsaneEditorStyles.Headers["AiEditor"]);
		}

		protected override void DrawCreateButton()
		{
			if (GUILayout.Button("Create new AI Preset"))
				CreateNewData<AISettings>();
		}
	}
}