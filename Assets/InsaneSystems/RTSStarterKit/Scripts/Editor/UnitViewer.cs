#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class UnitViewer : DataListEditor
	{
		[MenuItem("RTS Starter Kit/Unit Editor", priority = 1)]
		static void Init()
		{
			var window = (UnitViewer)GetWindow(typeof(UnitViewer));
			window.InitLoad();

			window.minSize = new Vector2(1024f, 700f);
			window.maxSize = new Vector2(1024f, 2048f);
			window.titleContent = new GUIContent(window.editorName);
			
			window.Show();
		}
		
		public override void InitLoad()
		{
			InitialSetup("RTS Unit Editor", "UnitsDatas", "unitdata", "Unit");
			StylesSetup(InsaneEditorStyles.Headers["UnitEditor"]);
		}
		
		protected override Sprite GetButtonIcon<T>(T data)
		{
			return data == null ? null : (data as UnitData).icon;
		}

		protected override void DrawCreateButton()
		{
			if (GUILayout.Button("Create new unit"))
				CreateNewData<UnitData>();
		}

		protected override void DrawCustomActions()
		{
			if (selectedDataId >= datasList.Count)
				return;

			var unitData = datasList[selectedDataId] as UnitData;
			
			bool isUnitDataSuitableForPrefab = unitData.unitModel != null;

			if (!isUnitDataSuitableForPrefab)
				EditorGUILayout.HelpBox("Add unit model to Unit Data field to generate prefab.", MessageType.Info);
			else
				EditorGUILayout.HelpBox("Unit prefab will be generated and created on opened scene. Also it will be saved to assets.", MessageType.Info);

			GUI.enabled = isUnitDataSuitableForPrefab;

			if (GUILayout.Button("Generate unit prefab"))
				CreateUnitPrefab(unitData);

			GUI.enabled = true; 
		}

		void CreateUnitPrefab(UnitData unitData)
		{
			var unitGameObject = new GameObject("Unit" + unitData.textId);
			unitGameObject.layer = LayerMask.NameToLayer("Unit");

			unitData.SetupPrefab(unitGameObject);

			if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
				AssetDatabase.CreateFolder("Assets", "Prefabs");

			if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Units"))
				AssetDatabase.CreateFolder("Assets/Prefabs", "Units");
			
			var path = "Assets/Prefabs/Units/Unit" + unitData.textId + ".prefab";
			var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(unitGameObject, path, InteractionMode.AutomatedAction);
			//AssetDatabase.Refresh(); // todo Insane Systems - is it needed?

			unitData.selfPrefab = prefab;
			EditorUtility.SetDirty(unitData);
		}
	}
}

#endif