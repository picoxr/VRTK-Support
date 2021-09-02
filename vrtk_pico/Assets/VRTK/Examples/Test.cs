using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private GameObject testGameObject;
    public Transform parentGameObject;
    // Start is called before the first frame update
    void Start()
    {
        testGameObject = this.gameObject;
        Invoke("TestParent", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("test");
    }
    private void TestParent()
    {
        testGameObject.transform.SetParent(parentGameObject);
        testGameObject.transform.localPosition = new Vector3(0,0,0);
    }

}
