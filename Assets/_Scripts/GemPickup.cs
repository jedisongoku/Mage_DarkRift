﻿using UnityEngine;

public class GemPickup : MonoBehaviour
{
    //public static Dictionary<GemPickup, Vector3> gemList = new Dictionary<GemPickup, Vector3>();
    bool isPickedUp = false;
    // Start is called before the first frame update


    // Update is called once per frame
    void OnEnable()
    {
        isPickedUp = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 8 && !isPickedUp)
        {
            isPickedUp = true;
            if(other.GetComponent<PlayerCombatManager>().isPlayer) other.gameObject.GetComponent<PlayerLevelManager>().AddXP(4);
            //if(gemList.ContainsKey(this)) gemList.Remove(this);
            gameObject.SetActive(false);
        }
    }

    
}
