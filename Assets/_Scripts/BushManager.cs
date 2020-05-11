using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BushManager : MonoBehaviour
{
    public static Dictionary<int, Dictionary<int,int>> players = new Dictionary<int, Dictionary<int,int>>();
    public static Dictionary<int, bool> isLocalInTheBush = new Dictionary<int, bool>();

    public int bushGroupId = 0;
    bool canBeSeen = false;
    bool isInvisible = false;

    private void Start()
    {
        if (!isLocalInTheBush.ContainsKey(bushGroupId)) isLocalInTheBush.Add(bushGroupId, false);
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 17 )
        {
            other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isInBush = true;
            if (other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isPlayer) isLocalInTheBush[bushGroupId] = true;

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
        if (other.gameObject.layer == 17 && players[bushGroupId].Count > 1 && isLocalInTheBush[bushGroupId] && !other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
        {
            if (!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().IsDead)
                other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().PlayerCanBeSeen();
            //other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen = true;
            //other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isSearchable = true;
        }
        else if(other.gameObject.layer == 17 && players[bushGroupId].Count <= 1 && !isLocalInTheBush[bushGroupId] && other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
        {
            if(!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().IsDead)
                other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().PlayerIsInvisible();
            //other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen = false;
            //other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isSearchable = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 17)
        {
            if (players.ContainsKey(bushGroupId))
            {
                if (other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isPlayer) isLocalInTheBush[bushGroupId] = false;

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
    }*/

    #region Updated Bush Triggers
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 17)
        {
            if(!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isInBush)
            {
                other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().BushEntered();
                if (other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isPlayer) isLocalInTheBush[bushGroupId] = true;
            }
                

            if (!players.ContainsKey(bushGroupId))
            {
                var temp = new Dictionary<int, int>();
                temp.Add(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID, 1);
                players.Add(bushGroupId, temp);
            }
            else
            {
                if (!players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID))
                {
                    players[bushGroupId].Add(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID, 1);
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
        if(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID != GameManager.Instance.GetCurrentPlayerViewID)
        {
            if (other.gameObject.layer == 17 && isLocalInTheBush[bushGroupId] && players[bushGroupId].Count > 1 && !other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
            {
                if (!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().IsDead)
                    other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().PlayerCanBeSeen();
            }
            else if (other.gameObject.layer == 17 && !isLocalInTheBush[bushGroupId] && other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
            {
                if (!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().IsDead)
                    other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().PlayerIsInvisible();
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 17)
        {
            if (players.ContainsKey(bushGroupId))
            {
                if (players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID))
                {
                    players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID]--;
                    if(players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] == 0)
                    {
                        other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().BushExited();
                        players[bushGroupId].Remove(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
                        if (other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isPlayer) isLocalInTheBush[bushGroupId] = false;
                    }
                        
                }
            }
        }
    }
    /*
    IEnumerator RemovePlayerFromBush(Collider other)
    {
        yield return new WaitForSeconds(0);

        if (players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID))
        {
            if (players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] == false)
            {
                players[bushGroupId].Remove(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
                other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().BushExited();
            }

            if (other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isPlayer) isLocalInTheBush[bushGroupId] = false;
        }

    }*/
    #endregion

}
