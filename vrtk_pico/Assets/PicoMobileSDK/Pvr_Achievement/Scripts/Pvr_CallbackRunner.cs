
using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
  public class Pvr_CallbackRunner : MonoBehaviour
  {

    public bool IsPersistantBetweenSceneLoads = true;

    void Awake()
    {
      var existingCallbackRunner = FindObjectOfType<Pvr_CallbackRunner>();
      if (existingCallbackRunner != this)
      {
        Debug.LogWarning("You only need one instance of CallbackRunner");
      }
      if (IsPersistantBetweenSceneLoads)
      {
        DontDestroyOnLoad(gameObject);
      }
    }

    void Update()
    {
      Request.RunCallbacks();
    }

    void OnApplicationQuit()
    {
      Pvr_Callback.OnApplicationQuit();
    }
  }
}
