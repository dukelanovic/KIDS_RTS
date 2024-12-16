using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class GlobalEditor : EditorWindow
	{
		static readonly List<ExtendedEditorWindow> editors = new List<ExtendedEditorWindow>();

		int selectedEditor = 0;
		
		[MenuItem("RTS Starter Kit/Global Editor", priority = -10000)]
		static void Init()
		{
			var window = (GlobalEditor)GetWindow(typeof(GlobalEditor));
			window.titleContent = new GUIContent("RTS Global Editor");
			window.maxSize = new Vector2(1024f, 700f);
			window.minSize = window.maxSize;
			window.Show();
			
			editors.Clear();
			
			editors.Add(CreateInstance<GameSettingsEditor>());
			editors.Add(CreateInstance<UnitViewer>());
			editors.Add(CreateInstance<ProductionCategoriesEditor>());
			editors.Add(CreateInstance<AbilitiesEditor>());
			editors.Add(CreateInstance<FactionsEditor>());
			editors.Add(CreateInstance<AIEditor>());
			editors.Add(CreateInstance<SoundEditor>());
			editors.Add(CreateInstance<TextsEditor>());
			
			foreach (var editor in editors)
				if (editor is DataListEditor dataListEditor)
					dataListEditor.InitLoad();
		}

		void OnGUI()
		{
			GUILayout.BeginHorizontal();

			var prevColor = GUI.color;
			
			for (var i = 0; i < editors.Count; i++)
			{
				if (selectedEditor == i)
					GUI.color = InsaneEditorStyles.GetSelectedButtonColor();
				
				if (GUILayout.Button(editors[i].GetType().Name))
					selectedEditor = i;

				GUI.color = prevColor;
			}

			GUILayout.EndHorizontal();
			
			if (editors.Count > 0)
				editors[selectedEditor].Draw();
		}
	}
}