using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDisableObject : MonoBehaviour
{
    [SerializeField] private float disableTimer = 1f;
    // Start is called before the first frame update
    void OnEnable()
    {
        Invoke("DisableObject", disableTimer);
    }


    void DisableObject()
    {
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
