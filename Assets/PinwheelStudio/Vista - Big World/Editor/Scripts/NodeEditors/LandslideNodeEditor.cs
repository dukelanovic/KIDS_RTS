#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(LandslideNode))]
    public class LandslideNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent INTENSITY = new GUIContent("Intensity", "Intensity of the effect.");
        private static readonly GUIContent INTENSITY_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the intensity without modifying its base value");

        private static readonly GUIContent FLOW_RATE = new GUIContent("Flow Rate", "Soil 'flow' speed, how fast the soil move to lower position. Default value is fine, too high may cause numerical error");
        private static readonly GUIContent FLOW_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the flow speed without modifying its base value");

        private static readonly GUIContent RESTING_ANGLE = new GUIContent("Resting Angle", "The angle in degree where soil stop sliding");
        private static readonly GUIContent RESTING_ANGLE_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the resting angle without modifying its base value");

        private static readonly GUIContent HIGH_QUALITY = new GUIContent("High Quality", "If true, it will simulate soil movement in 8 directions, otherwise 4");
        private static readonly GUIContent DETAIL_LEVEL = new GUIContent("Detail Level", "Smaller value runs faster and produces larger features, while larger value is more expensive but produces more micro details");
        private static readonly GUIContent ITERATION_COUNT = new GUIContent("Iteration", "The number of simulation step to perform");
        private static readonly GUIContent ITERATION_PER_FRAME = new GUIContent("Iteration Per Frame", "The number of step to perform in a single frame");

        private static readonly Rect RECT01 = new Rect(0, 0, 1, 1);

        public override void OnGUI(INode node)
        {
            LandslideNode n = node as LandslideNode;

            EditorGUI.BeginChangeCheck();
            float intensity = EditorGUILayout.FloatField(INTENSITY, n.intensity);
            float intensityMultiplier = EditorGUILayout.Slider(INTENSITY_MULTIPLIER, n.intensityMultiplier, 0f, 2f);

            float flowRate = EditorGUILayout.FloatField(FLOW_RATE, n.flowRate);
            float flowMultiplier = EditorGUILayout.Slider(FLOW_MULTIPLIER, n.flowMultiplier, 0f, 2f);

            float restingAngle = EditorGUILayout.FloatField(RESTING_ANGLE, n.restingAngle);
            float restingAngleMultiplier = EditorGUILayout.Slider(RESTING_ANGLE_MULTIPLIER, n.restingAngleMultiplier, 0f, 2f);

            bool highQuality = EditorGUILayout.Toggle(HIGH_QUALITY, n.highQualityMode);
            float detailLevel = EditorGUILayout.Slider(DETAIL_LEVEL, n.detailLevel, 0f, 1f);
            int iterationCount = EditorGUILayout.IntField(ITERATION_COUNT, n.iterationCount);
            int iterationPerFrame = EditorGUILayout.IntField(ITERATION_PER_FRAME, n.iterationPerFrame);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.intensity = intensity;
                n.intensityMultiplier = intensityMultiplier;

                n.flowRate = flowRate;
                n.flowMultiplier = flowMultiplier;

                n.restingAngle = restingAngle;
                n.restingAngleMultiplier = restingAngleMultiplier;

                n.highQualityMode = highQuality;
                n.detailLevel = detailLevel;
                n.iterationCount = iterationCount;
                n.iterationPerFrame = iterationPerFrame;
            }
        }
    }
}
#endif
