using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[CustomPropertyDrawer(typeof(CommentAttribute))]
	public sealed class CommentDrawer : PropertyDrawer
	{
		float height = 30f;
		float space = 5f;
		float yPadding = 10f;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var comment = attribute as CommentAttribute;

			height = EditorStyles.helpBox.CalcHeight(new GUIContent(comment.Comment), position.width) + yPadding;
			var helpRect = new Rect(position.x, position.y, position.width, height);
			EditorGUI.HelpBox(helpRect, comment.Comment, MessageType.Info);
			
			var offseted = new Rect(position.x, position.y + height + space, position.width, position.height);
			EditorGUI.PropertyField(offseted, property, new GUIContent(property.displayName), true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true) + height + space;
		}
	}
}