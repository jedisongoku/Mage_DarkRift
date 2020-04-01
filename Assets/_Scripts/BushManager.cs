using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BushManager : MonoBehaviour
{
    public static Dictionary<int, Dictionary<int,int>> players = new Dictionary<int, Dictionary<int,int>>();

    public int bushGroupId = 0;

    public static void OnPlayerDeath()
    {
        //players = new Dictionary<int, Dictionary<int, int>>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 17 )
        {
            other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isInBush = true;

            if(!players.ContainsKey(bushGroupId))
            {
                var temp = new Dictionary<int, int>();
                temp.Add(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID, 0);
                players.Add(bushGroupId, temp);
            }
            else
            {
                if (!players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID))
                {
                    players[bushGroupId].Add(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID, 0);
                }
                else
                {
                    players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID]++;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 17 && players[bushGroupId].Count > 1)
        {   
            other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen = true;
        }
        else if(other.gameObject.layer == 17 && players[bushGroupId].Count <= 1)
        {
            other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 17)
        {
            if (players.ContainsKey(bushGroupId))
            {
                if (players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID) && players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] == 0)
                {
                    players[bushGroupId].Remove(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
                }
                else if (players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID) && players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] > 0)
                {
                    players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID]--;
                }
            }
            
            //other.GetComponent<PlayerCombatManager>().BushExited();
        }
    }
}
