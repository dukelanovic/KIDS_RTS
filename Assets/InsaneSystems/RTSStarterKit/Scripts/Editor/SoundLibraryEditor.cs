using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CustomEditor(typeof(SoundLibrary))]
	public sealed class SoundLibraryEditor : Editor
	{
		static string categoryName = "";
		static int removeCategoryId = -1;
	
		public override void OnInspectorGUI()
		{
			var soundLibrary = target as SoundLibrary;
			
			var categories = serializedObject.FindProperty("soundsCategories");
			var minVolume = serializedObject.FindProperty("UnitMinSoundDistance"); 
			var maxVolume = serializedObject.FindProperty("UnitMaxSoundDistance");

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(minVolume);
			EditorGUILayout.PropertyField(maxVolume);

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}
			
			DrawCategories(categories);
			DrawCreateCategory(categories, soundLibrary); 
		}
		
		// todo RTS Kit - we can unify it, because actually this code knows nothing about category type and still works. So for Text Editor can be used the same.
		void DrawCategories(SerializedProperty categories)
		{
			for (int i = 0; i < categories.arraySize; i++)
			{
				var arrayElement = categories.GetArrayElementAtIndex(i);

				GUILayout.BeginVertical(InsaneEditorStyles.RoundedCornersBoxSimple);
				EditorGUILayout.PropertyField(arrayElement, true);

				EditorGUILayout.Space();
				
				if (arrayElement.isExpanded)
				{
					if (i == removeCategoryId)
					{
						GUILayout.Label("Are you sure you want to remove it?");
						
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Yes"))
						{
							removeCategoryId = -1;
							
							categories.DeleteArrayElementAtIndex(i);
							serializedObject.ApplyModifiedProperties();
							
							GUILayout.EndHorizontal();
							GUILayout.EndVertical();
							return;
						}

						if (GUILayout.Button("No"))
							removeCategoryId = -1;
						
						GUILayout.EndHorizontal();
					}
					else if (GUILayout.Button("Remove category"))
					{
						removeCategoryId = i;
					}
				}

				GUILayout.EndVertical();
			}
		}
		
		void DrawCreateCategory(SerializedProperty categories, SoundLibrary soundLibrary)
		{
			GUILayout.BeginHorizontal(InsaneEditorStyles.RoundedCornersBoxSimple);

			categoryName = EditorGUILayout.TextField("Create new category", categoryName);
			
			var prevEnabled = GUI.enabled;
			
			GUI.enabled = categoryName.Length > 0 && soundLibrary.soundsCategories.Find(cat => cat.categoryName == categoryName) == null;
			
			if (GUILayout.Button("Create"))
			{
				categories.InsertArrayElementAtIndex(0);
				categories.GetArrayElementAtIndex(0).FindPropertyRelative("categoryName").stringValue = categoryName;
				serializedObject.ApplyModifiedProperties();
			}

			GUI.enabled = prevEnabled;
			
			GUILayout.EndHorizontal();
		}
	}
}