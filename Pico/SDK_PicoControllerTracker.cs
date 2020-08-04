// Unity SDK Controller Tracker|SDK_Unity|005
namespace VRTK
{
    using UnityEngine;
#if VRTK_DEFINE_SDK_PICO
    using Pvr_UnitySDKAPI;
#endif
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
    using UnityEngine.VR;
    using XRNode = UnityEngine.VR.VRNode;
#endif

    /// <summary>
    /// The Controller Tracker enables the GameObject to track it's position/rotation to the available connected VR Controller via the `UnityEngine.VR` library.
    /// </summary>
    /// <remarks>
    /// The Unity Controller Tracker is attached to the `[UnityBase_CameraRig]` prefab on the child `LeftHandAnchor` and `RightHandAnchor` to enable controller tracking.
    /// </remarks>
    public class SDK_PicoControllerTracker : MonoBehaviour
    {
        [Tooltip("The Unity VRNode to track.")]
        public XRNode nodeType;
        [Tooltip("The unique index to assign to the controller.")]
        public uint index;
        [HideInInspector, Tooltip("The Unity Input name for the trigger axis.")]
        public string triggerAxisName = "";
        [HideInInspector, Tooltip("The Unity Input name for the grip axis.")]
        public string gripAxisName = "";
        [HideInInspector, Tooltip("The Unity Input name for the touchpad horizontal axis.")]
        public string touchpadHorizontalAxisName = "";
        [HideInInspector, Tooltip("The Unity Input name for the touchpad vertical axis.")]
        public string touchpadVerticalAxisName = "";

        protected virtual void OnEnable()
        {
            CheckAxisIsValid(triggerAxisName, "triggerAxisName");
            CheckAxisIsValid(gripAxisName, "gripAxisName");
            CheckAxisIsValid(touchpadHorizontalAxisName, "touchpadHorizontalAxisName");
            CheckAxisIsValid(touchpadVerticalAxisName, "touchpadVerticalAxisName");
        }

        protected virtual string GetVarName<T>(T item) where T : class
        {
            return VRTK_SharedMethods.GetPropertyFirstName<T>();
        }

        protected virtual void CheckAxisIsValid(string axisName, string varName)
        {
            //try
            //{
            //    Input.GetAxis(axisName);
            //}
            //catch (System.ArgumentException ae)
            //{
            //    VRTK_Logger.Warn(ae.Message + " on index [" + index + "] variable [" + varName + "]");
            //}
        }

        protected virtual void FixedUpdate()
        {
            //transform.localPosition = transform.InverseTransformDirection(Controller.UPvr_GetControllerPOS((int)index));
            //transform.localRotation = Controller.UPvr_GetControllerQUA((int)index);
            Debug.Log($"Controller {index} position is {transform.localPosition} and rotation is {transform.localRotation}");
        }
    }
}