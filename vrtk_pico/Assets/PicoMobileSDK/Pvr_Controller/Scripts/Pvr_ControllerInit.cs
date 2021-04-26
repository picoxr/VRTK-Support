// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class Pvr_ControllerInit : MonoBehaviour
{
    private ControllerVariety Variety;
    private bool isCustomModel = false;
    [SerializeField]
    private GameObject goblin;
    [SerializeField]
    private GameObject g2;
    [SerializeField]
    private GameObject neo2L;
    [SerializeField]
    private GameObject neo2R;
    [SerializeField]
    private GameObject neo3L;
    [SerializeField]
    private GameObject neo3R;
    [SerializeField]
    private Material standardMat;
    [SerializeField]
    private Material unlitMat;

    private int controllerType = -1;
    [HideInInspector]
    public bool loadModelSuccess = false;

    private int systemOrUnity = 0;
    private JsonData curControllerData;
    private string modelName = "";
    private string texFormat = "";
    private string prePath = "";
    private string modelFilePath = "/system/media/PvrRes/controller/";
    private const string goblinTexbasePath = "Controller/controller1/controller1";
    private const string g2TexbasePath = "Controller/controller3/controller3";
    private const string neo2TexbasePath = "Controller/controller4/controller4";
    private const string neo3TexbasePath = "Controller/controller5/controller5";

    void Awake()
    {
#if ANDROID_DEVICE
        int enumindex = (int)GlobalIntConfigs.iCtrlModelLoadingPri;
        Render.UPvr_GetIntConfig(enumindex, ref systemOrUnity);
#endif
        Variety = transform.GetComponentInParent<Pvr_ControllerModuleInit>().Variety;
        isCustomModel = transform.GetComponentInParent<Pvr_ControllerModuleInit>().IsCustomModel;
        if (!isCustomModel)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerAbilityEvent += CheckControllerStateOfAbility;
            Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForGoblin;
        }

#if UNITY_EDITOR
        var neo2Go = Instantiate(Variety == ControllerVariety.Controller0 ? neo2L : neo2R, transform, false);
        var neo2Comp = neo2Go.AddComponent<Pvr_ControllerVisual>();
        neo2Comp.currentDevice = ControllerDevice.Neo2;
        LoadTexture(neo2Comp, neo2TexbasePath, true);
#endif
    }
    void OnDestroy()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
        Pvr_ControllerManager.SetControllerAbilityEvent -= CheckControllerStateOfAbility;
        Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForGoblin;
    }

    private void OnApplicationPause(bool pause)
    {
#if ANDROID_DEVICE
        if (pause)
        {
            DestroyLocalController();
        }
#endif
    }

    private void ServiceStartSuccess()
    {
        var type = Controller.UPvr_GetDeviceType();
        if (controllerType != type && type != 0)
        {
            controllerType = type;
        }

        LoadResFromJson();

        if (Pvr_ControllerManager.controllerlink.neoserviceStarted)
        {
            if (Variety == ControllerVariety.Controller0)
            {
                if (Pvr_ControllerManager.controllerlink.controller0Connected)
                {
                    StartCoroutine(RefreshController(0));
                }
                else
                {
                    DestroyLocalController();
                }
            }
            if (Variety == ControllerVariety.Controller1)
            {
                if (Pvr_ControllerManager.controllerlink.controller1Connected)
                {
                    StartCoroutine(RefreshController(1));
                }
                else
                {
                    DestroyLocalController();
                }
            }
        }
        if (Pvr_ControllerManager.controllerlink.goblinserviceStarted)
        {
            if (Variety == ControllerVariety.Controller0)
            {
                if (Pvr_ControllerManager.controllerlink.controller0Connected)
                {
                    StartCoroutine(RefreshController(0));
                }
                else
                {
                    DestroyLocalController();
                }
            }
        }
    }

    private void LoadResFromJson()
    {
        string json = Pvr_UnitySDKAPI.System.UPvr_GetObjectOrArray("config.controller",
            (int)Pvr_UnitySDKAPI.ResUtilsType.TYPR_OBJECTARRAY);
        if (json != null)
        {
            JsonData jdata = JsonMapper.ToObject(json);
            if (controllerType >= 0)
            {
                if (jdata.Count >= controllerType )
                {
                    curControllerData = jdata[controllerType - 1];
                    modelFilePath = (string)curControllerData["base_path"];
                    modelName = (string)curControllerData["model_name"] + "_sys";
                }
            }
        }
        else
        {
            PLOG.E("PvrLog LoadJsonFromSystem Error");
        }

    }

    private void CheckControllerStateForGoblin(string state)
    {
        var type = Controller.UPvr_GetDeviceType();
        if (Variety == ControllerVariety.Controller0)
        {
            if (Convert.ToInt16(state) == 1)
            {
                if (controllerType != type)
                {
                    DestroySysController();
                    controllerType = type;
                }
                StartCoroutine(RefreshController(0));
            }
            else
            {
                DestroyLocalController();
            }
        }
    }

    private void CheckControllerStateOfAbility(string data)
    {
        var state = Convert.ToBoolean(Convert.ToInt16(data.Substring(4, 1)));
        var id = Convert.ToInt16(data.Substring(0, 1));
        var type = Controller.UPvr_GetDeviceType();
        if (state)
        {
            controllerType = type;
            switch (id)
            {
                case 0:
                    if (Variety == ControllerVariety.Controller0)
                    {
                        StartCoroutine(RefreshController(0));
                    }
                    break;
                case 1:
                    if (Variety == ControllerVariety.Controller1)
                    {
                        StartCoroutine(RefreshController(1));
                    }
                    break;
            }
        }
        else
        {
            switch (id)
            {
                case 0:
                    if (Variety == ControllerVariety.Controller0)
                    {
                        DestroyLocalController();
                    }
                    break;
                case 1:
                    if (Variety == ControllerVariety.Controller1)
                    {
                        DestroyLocalController();
                    }
                    break;
            }
        }
    }

    private void DestroyLocalController()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
            loadModelSuccess = false;
        }
    }

    private void DestroySysController()
    {
        foreach (Transform t in transform)
        {
            if (t.name == modelName)
            {
                Destroy(t.gameObject);
                loadModelSuccess = false;
            }
        }
    }

    private IEnumerator RefreshController(int id)
    {
        yield return null;
        yield return null;

        if (Controller.UPvr_GetControllerState(id) == ControllerState.Connected)
        {
            if (systemOrUnity == 0)
            {
                LoadControllerFromPrefab();
                if (!loadModelSuccess)
                {
                    LoadControllerFromSystem(id);
                }
            }
            else
            {
                var isControllerExist = false;
                foreach (Transform t in transform)
                {
                    if (t.name == modelName)
                    {
                        isControllerExist = true;
                    }
                }
                if (!isControllerExist)
                {
                    LoadControllerFromSystem(id);
                    if (!loadModelSuccess)
                    {
                        LoadControllerFromPrefab();
                    }
                }
                else
                {
                    var currentController = transform.Find(modelName);
                    currentController.gameObject.SetActive(true);
                }
            }
            Pvr_ToolTips.RefreshTips();
            PLOG.I("PvrLog Controller Refresh Success");
        }
    }

    private void LoadControllerFromPrefab()
    {
        switch (controllerType)
        {
            case 1:
                var goblinGo = Instantiate(goblin, transform, false);
                var goblinComp = goblinGo.AddComponent<Pvr_ControllerVisual>();
                goblinComp.currentDevice = ControllerDevice.Goblin;
                LoadTexture(goblinComp, goblinTexbasePath, true);
                loadModelSuccess = true;
                break;
            case 3:
                var g2Go = Instantiate(g2, transform, false);
                var g2Comp = g2Go.AddComponent<Pvr_ControllerVisual>();
                g2Comp.currentDevice = ControllerDevice.G2;
                LoadTexture(g2Comp, g2TexbasePath, true);
                loadModelSuccess = true;
                break;
            case 4:
                var neo2Go = Instantiate(Variety == ControllerVariety.Controller0 ? neo2L : neo2R, transform, false);
                var neo2Comp = neo2Go.AddComponent<Pvr_ControllerVisual>();
                neo2Comp.currentDevice = ControllerDevice.Neo2;
                LoadTexture(neo2Comp,neo2TexbasePath,true);
                loadModelSuccess = true;
                break;
            case 5:
                var neo3Go = Instantiate(Variety == ControllerVariety.Controller0 ? neo3L : neo3R, transform, false);
                var neo3Comp = neo3Go.AddComponent<Pvr_ControllerVisual>();
                neo3Comp.currentDevice = ControllerDevice.Neo3;
                LoadTexture(neo3Comp, neo3TexbasePath, true);
                loadModelSuccess = true;
                break;
            default:
                loadModelSuccess = false;
                break;
        }
    }

    private void LoadControllerFromSystem(int id)
    {
        var syscontrollername = controllerType.ToString() + id.ToString() + ".obj";
        var fullFilePath = modelFilePath + syscontrollername;

        if (!File.Exists(fullFilePath))
        {
            PLOG.I("PvrLog Obj File does not exist==" + fullFilePath);
        }
        else
        {
            GameObject go = new GameObject();
            go.name = modelName;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = Pvr_ObjImporter.Instance.ImportFile(fullFilePath);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            int matID = (int)curControllerData["material_type"];
            meshRenderer.material = matID == 0 ? standardMat : unlitMat;

            loadModelSuccess = true;
            Pvr_ControllerVisual controllerVisual = go.AddComponent<Pvr_ControllerVisual>();
            switch (controllerType)
            {
                case 1:
                    {
                        controllerVisual.currentDevice = ControllerDevice.Goblin;
                    }
                    break;
                case 3:
                    {
                        controllerVisual.currentDevice = ControllerDevice.G2;
                    }
                    break;
                case 4:
                    {
                        controllerVisual.currentDevice = ControllerDevice.Neo2;
                    }
                    break;
                case 5:
                    {
                        controllerVisual.currentDevice = ControllerDevice.Neo3;
                    }
                    break;
                default:
                    controllerVisual.currentDevice = ControllerDevice.NewController;
                    break;
            }

            controllerVisual.variety = Variety;
            LoadTexture(controllerVisual, controllerType.ToString() + id.ToString(),false);
            go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            go.transform.localScale = new Vector3(-0.01f, 0.01f, 0.01f);
        }
    }


    private void LoadTexture(Pvr_ControllerVisual visual, string controllerName,bool fromRes)
    {
        if (fromRes)
        {
            texFormat = "";
            prePath = controllerName;
        }
        else
        {
            texFormat = "." + (string)curControllerData["tex_format"];
            prePath = modelFilePath + controllerName;
        }
            
        var texturepath = prePath + "_idle" + texFormat;
        visual.m_idle = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_app" + texFormat;
        visual.m_app = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_home" + texFormat;
        visual.m_home = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_touch" + texFormat;
        visual.m_touchpad = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_volume_down" + texFormat;
        visual.m_volDn = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_volume_up" + texFormat;
        visual.m_volUp = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_trigger" + texFormat;
        visual.m_trigger = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_a" + texFormat;
        visual.m_a = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_b" + texFormat;
        visual.m_b = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_x" + texFormat;
        visual.m_x = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_y" + texFormat;
        visual.m_y = LoadOneTexture(texturepath, fromRes);
        texturepath = prePath + "_grip" + texFormat;
        visual.m_grip = LoadOneTexture(texturepath, fromRes);
    }

    private Texture2D LoadOneTexture(string filepath,bool fromRes)
    {
        if (fromRes)
        {
            return Resources.Load<Texture2D>(filepath);
        }
        else
        {
            int t_w = (int)curControllerData["tex_width"];
            int t_h = (int)curControllerData["tex_height"];
            var m_tex = new Texture2D(t_w, t_h);
            m_tex.LoadImage(ReadPNG(filepath));
            return m_tex;
        }
    }

    private byte[] ReadPNG(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] binary = new byte[fileStream.Length];
        fileStream.Read(binary, 0, (int)fileStream.Length);
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        return binary;
    }
}
