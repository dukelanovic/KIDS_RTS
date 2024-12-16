using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> Special EditorWindow class, simplifies creation of Data-files editors (UnitDatas, ProductionCategories etc in same way. Should be used as template - derive from it. </summary>
	public abstract class DataListEditor : ExtendedEditorWindow
	{
		protected const string DatasFolderPath = "Assets/Resources/Data";
		
		protected string editorName = "RTS Unit Editor";
		protected string datasFolderName = "SO";
		protected string dataSearchFilter = "scriptableobject";
		protected string defaultDataFileName = "SOPreset";
		
		protected readonly List<ScriptableObject> datasList = new List<ScriptableObject>();

		protected int selectedDataId, loadedDatasCount;

		Vector2 datasListScrollPos, dataEditorScrollPos, actionsScrollPos;
	
		string searchString = "", dataName;

		GUIStyle headerStyle;

		public abstract void InitLoad();
		
		public void InitialSetup(string editorName, string datasFolderName, string dataSearchFilter, string defaultDataFileName)
		{
			this.editorName = editorName;
			this.datasFolderName = datasFolderName;
			this.dataSearchFilter = dataSearchFilter;
			this.defaultDataFileName = defaultDataFileName;
			
			ReloadDatas();
			SelectData(0);
		}
		
		public void StylesSetup(GUIStyle headerStyle)
		{
			this.headerStyle = headerStyle;
		}

		public override void Draw()
		{
			if (loadedDatasCount == 0)
			{
				ReloadDatas();
				return;
			}

			DefaultDraw();
			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(InsaneEditorStyles.DatasListStyle, GUILayout.MaxWidth(240));

			datasListScrollPos = EditorGUILayout.BeginScrollView(datasListScrollPos);
			GUILayout.Label("Datas list", InsaneEditorStyles.HeaderTextStyle);

			DrawList();

			InsaneEditorStyles.DrawUILine(Color.gray, 1);

			DrawCreateButton();

			EditorGUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(InsaneEditorStyles.PaddedBoxStyle);

			GUILayout.Label("Editor of the " + GetEditorTitle(), InsaneEditorStyles.HeaderTextStyle);
			DrawEditor(datasList);
			GUILayout.EndVertical();

			GUILayout.BeginVertical(InsaneEditorStyles.PaddedBoxStyle, GUILayout.MaxWidth(200));
			actionsScrollPos = EditorGUILayout.BeginScrollView(actionsScrollPos);
			GUILayout.Label("Actions", InsaneEditorStyles.HeaderTextStyle);
			DrawActions();
			EditorGUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		public virtual void OnGUI()
		{
			Draw();
		}

		protected virtual void DefaultDraw()
		{
			GUILayout.BeginVertical(headerStyle);
			GUILayout.Label(editorName, InsaneEditorStyles.EditorsHeaderTextStyle);
			GUILayout.EndVertical();
		}
		
		protected abstract void DrawCreateButton();
		protected virtual void DrawCustomActions() { }

		protected virtual Sprite GetButtonIcon<T>(T data) where T : ScriptableObject 
		{ return null;}
		
		protected string GetEditorTitle()
		{
			if (selectedDataId < datasList.Count)
				return datasList[selectedDataId].name;

			return "Empty";
		}

		public void ReloadDatas()
		{
			datasList.Clear();

			EditorExtensions.LoadAssetsToList(datasList, "t:" + dataSearchFilter);

			loadedDatasCount = datasList.Count;
		}
		
		protected void DrawEditor<T>(List<T> dataList) where T : ScriptableObject
		{
			if (selectedDataId >= dataList.Count)
				return;

			dataEditorScrollPos = EditorGUILayout.BeginScrollView(dataEditorScrollPos);

			var data = dataList[selectedDataId];
			
			EditorGUILayout.BeginHorizontal();
			
			dataName = EditorGUILayout.TextField("Change name", dataName);
			if (GUILayout.Button("Apply"))
				data.name = dataName;
			
			EditorGUILayout.EndHorizontal();
			
			var unitDataEditor = Editor.CreateEditor(data);
			unitDataEditor.OnInspectorGUI();
			EditorGUILayout.EndScrollView();
		}
		
		protected void SelectData<T>(T data) where T : ScriptableObject
		{
			for (int i = 0; i < datasList.Count; i++)
				if (data == datasList[i])
				{
					selectedDataId = i;
					dataName = data.name;
				}
		}
		
		protected void SelectData(int id)
		{
			if (id < 0 || id >= datasList.Count)
				return;
			
			selectedDataId = id;
			dataName = datasList[id].name;
		}
		
		protected void CreateNewData<T>(T dataToClone = null) where T : ScriptableObject
		{
			CheckFolders();
			
			var data = ScriptableObject.CreateInstance(typeof(T)) as T;

			if (dataToClone)
				data = Instantiate(dataToClone);

			AssetDatabase.CreateAsset(data, $"{DatasFolderPath}/{datasFolderName}/{defaultDataFileName}({datasList.Count + 1})_{data.name}.asset");

			ReloadDatas();
			SelectData(data);
		}
		
		protected void CheckFolders()
		{		
			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
				AssetDatabase.CreateFolder("Assets/Resources", "Data");
			
			if (!AssetDatabase.IsValidFolder(DatasFolderPath + "/" + datasFolderName))
				AssetDatabase.CreateFolder(DatasFolderPath, datasFolderName);
		}

		protected void DrawList()
		{
			GUILayout.Label("Search");
			searchString = EditorGUILayout.TextField(searchString);

			int foundAmount = 0;
			
			for (int i = 0; i < datasList.Count; i++)
			{
				if (!datasList[i])
					continue;
				
				if (searchString != "" && !datasList[i].name.ToLower().Contains(searchString))
					continue;
				
				int cachedIndex = i;

				GUILayout.BeginHorizontal();

				if (selectedDataId == i)
					GUI.color = EditorGUIUtility.isProSkin ? InsaneEditorStyles.SelectedButtonProColor : InsaneEditorStyles.SelectedButtonColor;
				
				var buttonContent = new GUIContent(datasList[i].name);

				var icon = GetButtonIcon(datasList[i]);
				
				if (icon)
					buttonContent.image = icon.texture;

				if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(24)))
					SelectData(cachedIndex);

				GUI.color = Color.white;
				if (GUILayout.Button("X", GUILayout.MaxHeight(24), GUILayout.MaxWidth(24)))
					RemoveDataWindow.Init(AssetDatabase.GetAssetPath(datasList[i]), datasList[i], this);

				GUILayout.EndHorizontal();

				foundAmount++;
			}
			
			if (foundAmount == 0)
				GUILayout.Label("Nothing found");
		}
		
		protected void DrawActions()
		{
			if (selectedDataId >= datasList.Count)
				return;

			if (GUILayout.Button("Clone " + defaultDataFileName))
				CreateNewData(datasList[selectedDataId]);

			if (GUILayout.Button("Delete " + defaultDataFileName))
				RemoveDataWindow.Init(AssetDatabase.GetAssetPath(datasList[selectedDataId]), datasList[selectedDataId], this);

			DrawCustomActions();
		}
	}
}