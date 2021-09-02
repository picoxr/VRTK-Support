// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using Pvr_UnitySDKAPI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pvr_UnitySDKEyeManager))]
public class Pvr_UnitySDKEyeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.changed = false;

        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        var guiContent = new GUIContent();
        Pvr_UnitySDKEyeManager sdkEyeManager = (Pvr_UnitySDKEyeManager)target;

        guiContent.text = "Eye Tracking";
        guiContent.tooltip = "Before calling EyeTracking API, enable this option first（For Neo 2 Eye device ONLY). ";
        sdkEyeManager.EyeTracking = EditorGUILayout.Toggle(guiContent, sdkEyeManager.EyeTracking);
        if(sdkEyeManager.EyeTracking)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("EyeTracking is supported only on the Neo2 Eye");
            EditorGUILayout.EndVertical();
        }

        guiContent.text = "Foveated Rendering";
        guiContent.tooltip = "Helps reducing the power usage and slightly increases performance by sacrificing the quality of the peripheral region. In addition, enable both Eye-Tracking and Foveated Rendering will switch to Dynamic Foveated Rendering automatically.";
        sdkEyeManager.FoveatedRendering = EditorGUILayout.Toggle(guiContent, sdkEyeManager.FoveatedRendering);
        if (sdkEyeManager.FoveatedRendering)
        {
            EditorGUI.indentLevel = 1;
            sdkEyeManager.FoveationLevel = (EFoveationLevel)EditorGUILayout.EnumPopup("Foveation Level", sdkEyeManager.FoveationLevel);
            EditorGUI.indentLevel = 0;
        }

        EditorUtility.SetDirty(sdkEyeManager);
        if (GUI.changed)
        {
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager
                .GetActiveScene());
#endif
        }
    }

}
