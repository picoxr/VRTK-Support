// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using UnityEditor;
using Pvr_UnitySDKAPI;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;
using UnityEditor.Build;

[CustomEditor(typeof(Pvr_UnitySDKManager))]
public class Pvr_UnitySDKManagerEditor : Editor, IPreprocessBuild
{
    public delegate void HeadDofChanged(string dof);
    public static event HeadDofChanged HeadDofChangedEvent;

    static int QulityRtMass = 0;
    public delegate void Change(int Msaa);
    public static event Change MSAAChange;
    public const string PVRSinglePassDefine = "PVR_SINGLEPASS_ENABLED";
    
    public delegate void SetContentProtect(string enable_cpt);
    public static event SetContentProtect SetContentProtectXml;

    private bool resetNeckOffset = false;

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        var guiContent = new GUIContent();

        Pvr_UnitySDKManager manager = (Pvr_UnitySDKManager)target;
        Pvr_UnitySDKProjectSetting projectConfig = Pvr_UnitySDKProjectSetting.GetProjectConfig();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Current Build Platform", firstLevelStyle);
        EditorGUILayout.LabelField(EditorUserBuildSettings.activeBuildTarget.ToString());
        GUILayout.Space(10);

        EditorGUILayout.LabelField("RenderTexture Setting", firstLevelStyle);
        projectConfig.rtAntiAlising = (RenderTextureAntiAliasing)EditorGUILayout.EnumPopup("RenderTexture Anti-Aliasing", projectConfig.rtAntiAlising);
#if UNITY_2018_3_OR_NEWER
        GUI.enabled = false;
#endif
        projectConfig.rtBitDepth = (RenderTextureDepth)EditorGUILayout.EnumPopup("RenderTexture Bit Depth", projectConfig.rtBitDepth);
        projectConfig.rtFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup("RenderTexture Format", projectConfig.rtFormat);
#if UNITY_2018_3_OR_NEWER
        GUI.enabled = true;
#endif
        guiContent.text = "Use Default RenderTexture";
        guiContent.tooltip = "If false，you can define customed size of Render Texture, and effect for all scenes.";
        projectConfig.usedefaultRenderTexture = EditorGUILayout.Toggle(guiContent, projectConfig.usedefaultRenderTexture);
        if (!projectConfig.usedefaultRenderTexture)
        {
            projectConfig.customRTSize = EditorGUILayout.Vector2Field("    RT Size", projectConfig.customRTSize);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("1.width & height must be larger than 0;");
            EditorGUILayout.LabelField("2.the size of RT has a great influence on performance;");
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Pose Settings", firstLevelStyle);

        guiContent.text = "Tracking Origin";
        guiContent.tooltip = "Define the current tracking origin type.";
        manager.TrackingOrigin = (TrackingOrigin)EditorGUILayout.EnumPopup(guiContent, manager.TrackingOrigin);

        guiContent.text = "Reset Tracker OnLoad";
        guiContent.tooltip = "If true, each scene load will cause the head pose to reset.";
        manager.ResetTrackerOnLoad = EditorGUILayout.Toggle(guiContent, manager.ResetTrackerOnLoad);

        guiContent.text = "Only Rotation Tracking";
        guiContent.tooltip = "If true，tracking will not affect the position of specified target.";
        manager.Rotfoldout = EditorGUILayout.Foldout(manager.Rotfoldout, guiContent, true);
        if (manager.Rotfoldout)
        {
            guiContent.text = "  Only HMD Rotation Tracking";
            guiContent.tooltip = "If true, head tracking will not affect the position of each Pvr_UnitySDK's cameras, and just act in three DOF mode.";
            manager.HmdOnlyrot = EditorGUILayout.Toggle(guiContent, manager.HmdOnlyrot);
            if (manager.HmdOnlyrot)
            {
                manager.PVRNeck = EditorGUILayout.Toggle("    Enable Neck Model", manager.PVRNeck);
                if (manager.PVRNeck)
                {
                    manager.neckOffset = EditorGUILayout.Vector3Field("Neck Offset", manager.neckOffset);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(100);
                    resetNeckOffset = GUILayout.Button("Use Default Neck Offset");
                    GUILayout.EndHorizontal();
                    if (resetNeckOffset)
                    {
                        manager.neckOffset = new Vector3(0, 0.075f, 0.0805f);
                        manager.UseCustomNeckPara = false;
                    }
                }
            }
            else
            {
                manager.UseCustomNeckPara = false;
            }
            guiContent.text = "  Only Controller Rotation Tracking";
            guiContent.tooltip = "If true, hand tracking will not affect the position of controller(s).It will just act in three DOF mode.";
            manager.ControllerOnlyrot =
                EditorGUILayout.Toggle(guiContent, manager.ControllerOnlyrot);
        }
        else
        {
            manager.HmdOnlyrot = false;
            manager.ControllerOnlyrot = false;
            manager.UseCustomNeckPara = false;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Other Settings", firstLevelStyle);

        guiContent.text = "Show FPS";
        guiContent.tooltip = "If true，display FPS on Overlay.";
        manager.ShowFPS = EditorGUILayout.Toggle(guiContent, manager.ShowFPS);

        guiContent.text = "Use Default FPS";
        guiContent.tooltip = "If false, you can define upper limit of FPS.";
        projectConfig.usedefaultfps = EditorGUILayout.Toggle(guiContent, projectConfig.usedefaultfps);
        if (!projectConfig.usedefaultfps)
        {
            projectConfig.customfps = EditorGUILayout.IntField("    FPS", projectConfig.customfps);
        }

        EditorGUI.BeginDisabledGroup(projectConfig.usesinglepass);
        guiContent.text = "Use Monoscopic";
        guiContent.tooltip = "If true, both eyes will be rendered with same image  from the center eye pose, which will reduce GPU load.";
        manager.Monoscopic = EditorGUILayout.Toggle(guiContent, manager.Monoscopic);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(manager.Monoscopic);
        guiContent.text = "Use SinglePass";
        guiContent.tooltip = "If true, objects are rendered once to the left eye buffer, then duplicated to the right buffer automatically with appropriate modifications. It primarily reduces CPU usage.";
        projectConfig.usesinglepass = EditorGUILayout.Toggle(guiContent, projectConfig.usesinglepass);
        if (projectConfig.usesinglepass != IsSinglePassEnable())
        {
            SetSinglePass(projectConfig.usesinglepass);
        }
        EditorGUI.EndDisabledGroup();

        guiContent.text = "Use Content Protect";
        guiContent.tooltip = "If true, content will not be recorded or casted illegally.";
        projectConfig.usecontentprotect = EditorGUILayout.Toggle(guiContent, projectConfig.usecontentprotect);
        if (projectConfig.usecontentprotect)
        {

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("This is Content Protect,if checked:");
            EditorGUILayout.LabelField("Screen Shot & Screen Recording & Screen Cast CANNOT work");
            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            QulityRtMass = (int)projectConfig.rtAntiAlising;
            if (QulityRtMass == 1)
            {
                QulityRtMass = 0;
            }
            if (MSAAChange != null)
            {
                MSAAChange(QulityRtMass);
            }
            var headDof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
            if (HeadDofChangedEvent != null)
            {
                if (headDof == 0)
                {
                    HeadDofChangedEvent("3dof");
                }
                else
                {
                    HeadDofChangedEvent("6dof");
                }

            }
            if (SetContentProtectXml != null)
            {
                if (projectConfig.usecontentprotect)
                    SetContentProtectXml("1");
                else
                    SetContentProtectXml("0");


            }
            if (manager.Rotfoldout && manager.HmdOnlyrot)
            {
                if (manager.neckOffset != new Vector3(0, 0.075f, 0.0805f))
                {
                    manager.UseCustomNeckPara = true;
                }
            }
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(projectConfig);
#if !UNITY_5_2
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
#endif
        }

        serializedObject.ApplyModifiedProperties();
    }

    public static bool IsSinglePassEnable()
    {
        bool isSinglePass;
#if UNITY_2017_2
        isSinglePass = PlayerSettings.virtualRealitySupported;
#else
        isSinglePass = PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android);
#endif
        //List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
        //isSinglePass &= allDefines.Contains(PVRSinglePassDefine);
        isSinglePass &= PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
        GraphicsDeviceType[] graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        isSinglePass &= graphics[0] == GraphicsDeviceType.OpenGLES3;
        return isSinglePass;
    }

    public static void SetSinglePass(bool enable)
    {
        if (enable)
        {
            SetVRSupported(BuildTargetGroup.Android, true);
            PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
            //List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
            //SetSinglePassDefine(BuildTargetGroup.Android, true, allDefines);
            SetGraphicsAPI();
        }
        else
        {
            SetVRSupported(BuildTargetGroup.Android, false);
            PlayerSettings.stereoRenderingPath = StereoRenderingPath.MultiPass;
            //List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
            //SetSinglePassDefine(BuildTargetGroup.Android, false, allDefines);
        }
    }

    public static void SetGraphicsAPI()
    {
        GraphicsDeviceType[] graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        List<GraphicsDeviceType> listgraphic = graphics.ToList();
        if (listgraphic.Contains(GraphicsDeviceType.OpenGLES3))
        {
            int index = listgraphic.IndexOf(GraphicsDeviceType.OpenGLES3);
            GraphicsDeviceType temp = listgraphic[0];
            listgraphic[0] = GraphicsDeviceType.OpenGLES3;
            listgraphic[index] = temp;
        }
        else
        {
            listgraphic.Insert(0, GraphicsDeviceType.OpenGLES3);
        }
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, listgraphic.ToArray());
    }

    public static void SetVRSupported(BuildTargetGroup group, bool set)
    {
#if UNITY_2017_2
        PlayerSettings.virtualRealitySupported = set; 
#else
        PlayerSettings.SetVirtualRealitySupported(group, set);
#endif
    }

    public static List<string> GetDefineSymbols(BuildTargetGroup group)
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        return symbols.Split(';').ToList();
    }

    public static void SetSinglePassDefine(BuildTargetGroup group, bool set, List<string> allDefines)
    {
        var hasDefine = allDefines.Contains(PVRSinglePassDefine);

        if (set)
        {
            if (hasDefine)
                return;
            allDefines.Add(PVRSinglePassDefine);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
            Debug.Log("Add \"" + PVRSinglePassDefine + "\" to define symbols");
        }
        else
        {
            if (hasDefine)
            {
                allDefines.Remove(PVRSinglePassDefine);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
                Debug.Log("Remove \"" + PVRSinglePassDefine + "\" from define symbols");
            }
        }
    }


    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        Debug.Log(string.Format("[Build Check] useSinglePass = {0}， isSinglePassEnable = {1}", Pvr_UnitySDKProjectSetting.GetProjectConfig().usesinglepass, IsSinglePassEnable()));
        if (Pvr_UnitySDKProjectSetting.GetProjectConfig().usesinglepass != IsSinglePassEnable())
        {
            Debug.Log(string.Format("[Build Check] SetSinglePass({0})", Pvr_UnitySDKProjectSetting.GetProjectConfig().usesinglepass));
            SetSinglePass(Pvr_UnitySDKProjectSetting.GetProjectConfig().usesinglepass);
        }
    }
}
