using UnityEngine;
using UnityEditor;
using System;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[CustomEditor(typeof(TriggerController))]
	public sealed class TriggerControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var triggerController = target as TriggerController;
			var triggerDatasProperty = serializedObject.FindProperty("triggerDatas");
			
			for (int i = 0; i < triggerDatasProperty.arraySize; i++)
			{
				GUILayout.BeginVertical(InsaneEditorStyles.RoundedCornersBoxSimple);
				
				var triggerDataProperty = triggerDatasProperty.GetArrayElementAtIndex(i);
				var triggerTypeProperty = triggerDataProperty.FindPropertyRelative("TriggerType");
				var triggerTextIdProperty = triggerDataProperty.FindPropertyRelative("TriggerTextId");
				var triggerProperty = triggerDataProperty.FindPropertyRelative("Trigger");

				GUILayout.Label(triggerTextIdProperty.stringValue != "" ? triggerTextIdProperty.stringValue : "New trigger", InsaneEditorStyles.SmallHeaderTextStyle);

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(triggerTypeProperty, true);

				var triggerName = triggerTypeProperty.enumNames[triggerTypeProperty.enumValueIndex];

				if (EditorGUI.EndChangeCheck())
				{
					if (triggerProperty.objectReferenceValue)
						DestroyImmediate((triggerProperty.objectReferenceValue as TriggerBase).gameObject);

					if (triggerName != "None")
					{
						var type = GetAssemblyType("InsaneSystems.RTSStarterKit.Triggers." + triggerName + "Trigger");

						var targetGO = new GameObject(triggerName);
						targetGO.transform.SetParent(triggerController.gameObject.transform);

						var addedComponent = targetGO.AddComponent(type);
						triggerProperty.objectReferenceValue = addedComponent;
					}
				}

				if (triggerName != "None" && triggerName != "")
					EditorGUILayout.PropertyField(triggerTextIdProperty, true);

				if (triggerProperty.objectReferenceValue)
				{
					GUILayout.Label("Trigger parameters", EditorStyles.boldLabel);
					var editor = Editor.CreateEditor(triggerProperty.objectReferenceValue);
					editor.DrawDefaultInspectorWithoutScriptField();
				}

				GUI.color = new Color(1f, 0.8f, 0.8f, 1f);
				if (GUILayout.Button("Delete trigger"))
				{
					if (triggerProperty.objectReferenceValue)
						DestroyImmediate((triggerProperty.objectReferenceValue as TriggerBase).gameObject);

					triggerDatasProperty.DeleteArrayElementAtIndex(i);
					serializedObject.ApplyModifiedProperties();
					GUILayout.EndVertical();
					
					return;
				}
				
				GUI.color = Color.white;

				//EditorGUILayout.PropertyField(triggerProperty, true);

				GUILayout.EndVertical();
			}
			
			if (GUILayout.Button("Add trigger"))
			{
				triggerDatasProperty.InsertArrayElementAtIndex(triggerDatasProperty.arraySize);

				var triggerDataProperty = triggerDatasProperty.GetArrayElementAtIndex(triggerDatasProperty.arraySize - 1);
				var triggerTypeProperty = triggerDataProperty.FindPropertyRelative("TriggerType");
				var triggerProperty = triggerDataProperty.FindPropertyRelative("Trigger");

				triggerTypeProperty.enumValueIndex = 0;
				triggerProperty.objectReferenceValue = null;
			}

			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			EditorUtility.SetDirty(target);
			Repaint();
		}

		public static Type GetAssemblyType(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type != null)
				return type;

			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = a.GetType(typeName);
				if (type != null)
					return type;
			}

			return null;
		}
	}

	public static class InsaneEditorExtension
	{
		public static bool DrawDefaultInspectorWithoutScriptField(this Editor inspector)
		{
			EditorGUI.BeginChangeCheck();

			inspector.serializedObject.Update();

			var iterator = inspector.serializedObject.GetIterator();
			iterator.NextVisible(true);

			while (iterator.NextVisible(false))
				EditorGUILayout.PropertyField(iterator, true);

			inspector.serializedObject.ApplyModifiedProperties();

			return EditorGUI.EndChangeCheck();
		}
	}
}