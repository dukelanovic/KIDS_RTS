using System;
using System.Collections.Generic;
using System.Linq;
using InsaneSystems.RTSStarterKit.Abilities;
using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class AbilitiesEditor : DataListEditor
	{
		int selectedNewAbilityTypeId;
		Type[] listOfAbilitiesTypes = Array.Empty<Type>();
		
		[MenuItem("RTS Starter Kit/Abilities Editor", priority = 2)]
		static void Init()
		{
			var window = (AbilitiesEditor)GetWindow(typeof(AbilitiesEditor));
			window.InitLoad();
			
			window.titleContent = new GUIContent(window.editorName);
			window.minSize = new Vector2(1024f, 600f);
			window.maxSize = new Vector2(1024f, 2048f);
			
			window.Show();
		}
		
		public override void InitLoad()
		{
			InitialSetup( "RTS Abilities Editor", "Abilities", "ability", "Ability");
			StylesSetup(InsaneEditorStyles.Headers["AbilitiesEditor"]);
			
			LoadAllAbilitiesTypes();
		}
		
		protected override Sprite GetButtonIcon<T>(T data) => data == null ? null : (data as Ability).icon;

		Type[] LoadAllAbilitiesTypes()
		{
			listOfAbilitiesTypes = (
				from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetTypes()
				where typeof(Ability).IsAssignableFrom(assemblyType)
				      && !assemblyType.IsAbstract
				select assemblyType).ToArray();

			return listOfAbilitiesTypes;
		}

		protected override void DrawCreateButton()
		{
			var selectedAbilityType = DrawAbilitySelectEnum();
			
			EditorGUILayout.HelpBox("If you need more abilities types, you can code your own. Check documentation for more info.", MessageType.Info);

			if (GUILayout.Button("Create new ability"))
				CreateNewAbility(selectedAbilityType);
		}

		Type DrawAbilitySelectEnum()
		{
			var strings = new List<string>();
			
			for (var i = 0; i < listOfAbilitiesTypes.Length; i++)
				strings.Add(listOfAbilitiesTypes[i].Name);
			
			selectedNewAbilityTypeId = EditorGUILayout.Popup("Select ability type", selectedNewAbilityTypeId, strings.ToArray());

			return listOfAbilitiesTypes[selectedNewAbilityTypeId];
		}
		
		void CreateNewAbility(Type type)
		{
			CheckFolders();
			
			var ability = ScriptableObject.CreateInstance(type) as Ability;
			ability.name = type.Name;
			
			AssetDatabase.CreateAsset(ability, DatasFolderPath + "/" + datasFolderName + "/Ability" + (datasList.Count + 1) + "_" + ability.name + ".asset");

			ReloadDatas();
			SelectData(ability);
		}
	}
}