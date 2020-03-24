using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisableObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnEnable()
    {
        Invoke("Disable", 5f);
    }

    void Disable()
    {
        //gameObject.transform.SetParent(null);
        this.gameObject.SetActive(false);
    }
}
