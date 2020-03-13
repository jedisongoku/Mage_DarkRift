using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FrostNova : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HIT : " + LayerMask.LayerToName(other.gameObject.layer));
        if (other.gameObject.layer == 8 && other.gameObject.GetComponent<PhotonView>() != null)
        {
            other.gameObject.GetComponent<PlayerMovementController>().StartChill(PlayerBaseStats.Instance.ChillDuration);
        }


    }
}
