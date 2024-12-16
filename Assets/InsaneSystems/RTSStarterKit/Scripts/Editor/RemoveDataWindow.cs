using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class RemoveDataWindow : EditorWindow
	{
		static string fileToRemoveName;
		static Object selectedData;
		static DataListEditor dataListEditorWindowCaller;
		
		public static void Init(string fileToRemove, Object dataFile, DataListEditor dataListEditorWindow)
		{
			var window = (RemoveDataWindow)GetWindow(typeof(RemoveDataWindow));
			window.titleContent = new GUIContent("Remove data");
			window.Show();

			fileToRemoveName = fileToRemove;
			selectedData = dataFile;
			dataListEditorWindowCaller = dataListEditorWindow;
		}

		void OnGUI()
		{
			GUILayout.Label("Are you sure you want to delete data file <b>" + selectedData.name + "</b>?", InsaneEditorStyles.PopupWindowTextStyle);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Yes"))
			{
				AssetDatabase.DeleteAsset(fileToRemoveName);
				dataListEditorWindowCaller.ReloadDatas();
				Close();
			}
			else if (GUILayout.Button("Cancel"))
			{
				Close();
			}
			GUILayout.EndHorizontal();
		}
	}
}