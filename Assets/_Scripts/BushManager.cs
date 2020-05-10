using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BushManager : MonoBehaviour
{
    public static Dictionary<int, Dictionary<int,bool>> players = new Dictionary<int, Dictionary<int,bool>>();
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
            other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isInBush = true;
            if (other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isPlayer) isLocalInTheBush[bushGroupId] = true;

            if (!players.ContainsKey(bushGroupId))
            {
                var temp = new Dictionary<int, bool>();
                temp.Add(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID, true);
                players.Add(bushGroupId, temp);
            }
            else
            {
                if (!players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID))
                {
                    players[bushGroupId].Add(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID, true);
                }
                else
                {
                    players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] = true;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] == false)
        {
            players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] = true;
        }

        if (other.gameObject.layer == 17 && players[bushGroupId].Count > 1 && isLocalInTheBush[bushGroupId] && !other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
        {
            if (!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().IsDead)
                other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().PlayerCanBeSeen();
            //other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen = true;
            //other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().isSearchable = true;
        }
        else if (other.gameObject.layer == 17 && players[bushGroupId].Count <= 1 && !isLocalInTheBush[bushGroupId] && other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
        {
            if (!other.transform.parent.gameObject.GetComponent<PlayerCombatManager>().IsDead)
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

                if (players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID) && players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] == true)
                {
                    players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] = false;
                    StartCoroutine(RemovePlayerFromBush(other));
                }
            }

            //other.GetComponent<PlayerCombatManager>().BushExited();
        }
    }

    IEnumerator RemovePlayerFromBush(Collider other)
    {
        yield return new WaitForSeconds(0.2f);

        if (players[bushGroupId].ContainsKey(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID))
        {
            if (players[bushGroupId][other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID] == false)
            {
                players[bushGroupId].Remove(other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
            }
        }

    }
    #endregion

}
