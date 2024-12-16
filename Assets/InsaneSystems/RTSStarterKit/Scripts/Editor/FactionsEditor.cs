using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class FactionsEditor : DataListEditor
	{
		[MenuItem("RTS Starter Kit/Factions Editor", priority = 2)]
		static void Init()
		{
			var window = (FactionsEditor)GetWindow(typeof(FactionsEditor));
			window.InitLoad();
			
			window.titleContent = new GUIContent(window.editorName);
			window.minSize = new Vector2(1024f, 600f);
			window.maxSize = new Vector2(1024f, 2048f);
			
			window.Show();
		}

		public override void InitLoad()
		{
			InitialSetup("RTS Factions Editor", "Factions", "factiondata", "Faction");
			StylesSetup(InsaneEditorStyles.Headers["FactionsEditor"]);
		}

		protected override void DrawCreateButton()
		{
			if (GUILayout.Button("Create new faction"))
				CreateNewData<FactionData>();
		}
	}
}