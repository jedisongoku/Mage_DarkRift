﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8 )
        {

            other.GetComponent<PlayerCombatManager>().BushEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            other.GetComponent<PlayerCombatManager>().BushExited();
        }
    }
}
