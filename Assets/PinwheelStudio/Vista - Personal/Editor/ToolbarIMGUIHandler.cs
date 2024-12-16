#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.VistaEditor;
using Pinwheel.Vista.Graphics;
using UnityEditor;
using System;

namespace Pinwheel.VistaEditor.Graph
{
    public static class ToolbarIMGUIHandler
    {
        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            GraphEditorToolbar.leftImguiCallback += OnGraphEditorToolbarLeftIMGUI;
        }

        private static void OnGraphEditorToolbarLeftIMGUI(GraphEditorToolbar sender)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Vista - Personal Edition", EditorCommon.Styles.italicLabel, GUILayout.Width(145));

            var featuredAssets = EditorSettings.Get().marketingSettings.GetFeaturedAssets();
            foreach (var a in featuredAssets)
            {
                if (a.name.StartsWith("Vista") && !string.IsNullOrEmpty(a.promotionText))
                {
                    string text = $"<color=orange>{a.promotionText}</color>";
                    if (GUILayout.Button(text, EditorCommon.Styles.richTextLabel))
                    {
                        string linkSuffix = "?utm_source=vista-personal&utm_medium=graph-editor&utm_campaign=none";
                        Application.OpenURL(a.link + linkSuffix);
                    }
                    Rect r = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
                    break;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
