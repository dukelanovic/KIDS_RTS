using System;
using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class WelcomeWindow : EditorWindow
	{
		//[MenuItem("RTS Starter Kit/Show Overview Page", priority = -100)]
		static void Init()
		{
			var window = (WelcomeWindow)EditorWindow.GetWindow(typeof(WelcomeWindow));

			window.titleContent = new GUIContent("Overview - RTS Starter Kit");
			window.minSize = new Vector2(1024f, 600f);
			window.maxSize = new Vector2(1024f, 2048f);
			
			window.Show();
		}

		void OnGUI()
		{
			
			// todo RTS Kit - logo
			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal();

			DrawBox("Test",
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
			
			DrawBox("Test",
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");

			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();

			DrawBox("Test",
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
			
			DrawBox("Contacts",
				"You can contact us using:<br>" +
				"E-Mail: <a href='mailto:godlikeaurora@gmail.com'>godlikeaurora@gmail.com</a><br>" +
				"Discord Server: <a href='https://discord.com/invite/7UUqQhU'>Open Discord</a>");

			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
		}

		void DrawBox(string name, Action innerDraw)
		{
			GUILayout.BeginVertical(InsaneEditorStyles.RoundedCornersBoxSimple);
			GUILayout.Label(name, InsaneEditorStyles.HeaderBoldTextStyle);

			innerDraw?.Invoke();

			GUILayout.EndVertical();
		}

		void DrawContacts()
		{
			GUILayout.Label("You can contact us using:");
			//EditorGUILayout.url("You can contact us using:", EditorStyles.);
		}
		
		void DrawBox(string name, string text)
		{
			DrawBox(name, DrawText(text)); // todo change it
		}

		Action DrawText(string text)
		{
			GUILayout.Label(text, InsaneEditorStyles.WrappedText);

			return null;
		}
	}
}