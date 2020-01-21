using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostNova : MonoBehaviour
{
    public int PlayerOrigin { get; set; }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8 && other.gameObject.GetComponent<Player>().ID != PlayerOrigin)
        {
            if (other.gameObject.GetComponent<Player>().IsServer)
            {
                other.gameObject.GetComponent<PlayerMovementManager>().ApplyChill(PlayerBaseStats.Instance.ChillDuration, PlayerRuneManager.Chill_ID);
            }
        }

    }
}
