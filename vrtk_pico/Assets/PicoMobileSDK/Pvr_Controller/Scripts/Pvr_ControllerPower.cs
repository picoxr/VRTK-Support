// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_ControllerPower : MonoBehaviour
{
    [SerializeField]
    private Sprite power1;
    [SerializeField]
    private Sprite power2;
    [SerializeField]
    private Sprite power3;
    [SerializeField]
    private Sprite power4;
    [SerializeField]
    private Sprite power5;

    
    [HideInInspector]
    public ControllerVariety variety;
    [HideInInspector]
    public ControllerDevice currentDevice;

    private Image powerImage;
    private int powerValue;
    private float power;

    void Start()
    {
        powerImage = transform.GetComponent<Image>();
        powerValue = -1;
        variety = transform.GetComponentInParent<Pvr_ControllerModuleInit>().Variety;
        currentDevice = transform.GetComponentInParent<Pvr_ControllerVisual>().currentDevice;
    }

    void Update()
    {
        RefreshPower(variety == ControllerVariety.Controller0
            ? 0
            : 1);
    }

    private void RefreshPower(int hand)
    {
        if (powerValue != Controller.UPvr_GetControllerPower(hand))
        {
            switch (Controller.UPvr_GetControllerPower(hand))
            {
                case 1:
                    powerImage.sprite = power1;
                    powerImage.color = Color.red;
                    break;
                case 2:
                    powerImage.sprite = power2;
                    powerImage.color = Color.white;
                    break;
                case 3:
                    powerImage.sprite = power3;
                    powerImage.color = Color.white;
                    break;
                case 4:
                    powerImage.sprite = power4;
                    powerImage.color = Color.white;
                    break;
                case 5:
                    powerImage.sprite = power5;
                    powerImage.color = Color.white;
                    break;
                default:
                    powerImage.sprite = power1;
                    powerImage.color = Color.white;
                    break;
            }
            powerValue = Controller.UPvr_GetControllerPower(hand);
        }
    }
}
