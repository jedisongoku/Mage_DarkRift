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
        players = new Dictionary<int, Dictionary<int, int>>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8 )
        {
            if(!players.ContainsKey(bushGroupId))
            {
                var temp = new Dictionary<int, int>();
                temp.Add(other.gameObject.GetComponent<PhotonView>().ViewID, 0);
                players.Add(bushGroupId, temp);
                Debug.Log("Bush registered to dictionary");
            }
            else
            {
                if (!players[bushGroupId].ContainsKey(other.gameObject.GetComponent<PhotonView>().ViewID))
                {
                    players[bushGroupId].Add(other.gameObject.GetComponent<PhotonView>().ViewID, 0);
                }
                else
                {
                    players[bushGroupId][other.gameObject.GetComponent<PhotonView>().ViewID]++;
                }
                Debug.Log("Player Registered to bush");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 8 && players[bushGroupId].Count > 1)
        {   
            other.GetComponent<PlayerCombatManager>().canBeSeen = true;
            Debug.Log("Can be Seen");
        }
        else if(other.gameObject.layer == 8 && players[bushGroupId].Count <= 1)
        {
            other.GetComponent<PlayerCombatManager>().canBeSeen = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            if(players.ContainsKey(bushGroupId))
            {
                if (players[bushGroupId].ContainsKey(other.gameObject.GetComponent<PhotonView>().ViewID) && players[bushGroupId][other.gameObject.GetComponent<PhotonView>().ViewID] == 0)
                {
                    players[bushGroupId].Remove(other.gameObject.GetComponent<PhotonView>().ViewID);
                }
                else if (players[bushGroupId].ContainsKey(other.gameObject.GetComponent<PhotonView>().ViewID) && players[bushGroupId][other.gameObject.GetComponent<PhotonView>().ViewID] > 0)
                {
                    players[bushGroupId][other.gameObject.GetComponent<PhotonView>().ViewID]--;
                }
                Debug.Log("Player unRegistered to bush");
            }
            
            //other.GetComponent<PlayerCombatManager>().BushExited();
        }
    }
}
