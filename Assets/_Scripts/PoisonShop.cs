using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PoisonShop : MonoBehaviourPun
{
    public static PoisonShop Instance;
    [SerializeField] private GameObject poisonShopShrine;
    [SerializeField] private Material poisonShopShrineDefaultMaterial;
    [SerializeField] private Material[] poisonShopShrineMaterials;
    [SerializeField] private GameObject[] poisonShopPortals;
    [SerializeField] private SphereCollider pickupCollider;
    [SerializeField] private int nextPoisonTime;

    float poisonPickupTimer;
    bool isCheckingForPlayers = false;
    List<Collider> players = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        
    }

    void Update()
    {
    }

    public void ClearPlayerList()
    {
        players.Clear();
    }
    IEnumerator CheckPlayersInPortal()
    {
        yield return new WaitForSeconds(0);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                if(players[i].enabled == false)
                {
                    players.Remove(players[i]);
                }
                
            }
            else
            {
                players.Remove(players[i]);
            }
        }
        if(players.Count > 1)
        {
            StartCoroutine(CheckPlayersInPortal());
        }
        else
        {
            isCheckingForPlayers = false;
        }
        
    }

    public Material[] ShrineMaterials
    {
        get
        {
            return poisonShopShrineMaterials;
        }
    }

    public Material ShrineDefaultMaterials
    {
        get
        {
            return poisonShopShrineDefaultMaterial;
        }
    }

    public GameObject[] Portals
    {
        get
        {
            return poisonShopPortals;
        }
    }

    public GameObject Shrine
    {
        get
        {
            return poisonShopShrine;
        }
    }

    public int NextPoisonTime
    {
        get
        {
            return nextPoisonTime;
        }
    }

    public SphereCollider Collider
    {
        get
        {
            return pickupCollider;
        }
    }

    void OnRoomPropertiesChanged(Hashtable updatedProperties)
    {
        //when room props changed
    }

    private void OnTriggerEnter(Collider other)
    {
        players.Add(other);
        if (players.Count > 1 && !isCheckingForPlayers)
        {
            isCheckingForPlayers = true;
            StartCoroutine(CheckPlayersInPortal());
        }
        if (other.GetComponent<PhotonView>().IsMine)
        {
            poisonPickupTimer = 0f;
            //HUDManager.Instance.EnablePoisonPickupLoader();
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PhotonView>().IsMine && players.Count == 1)
        {
            poisonPickupTimer += Time.deltaTime;
            //HUDManager.Instance.SetPoisonPickupProgress(poisonPickupTimer / PlayFabDataStore.playerBaseStats.PoisonPickupTime);
           /* if(poisonPickupTimer >= PlayFabDataStore.playerBaseStats.PoisonPickupTime)
            {
                //other.GetComponent<PlayerNetworkManager>().ActivatePoison(other.GetComponent<PhotonView>().ViewID);
                //HUDManager.Instance.DisablePoisonPickupLoader();
            }*/

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other != null)
        {
            players.Remove(other);
            if(isCheckingForPlayers)
            {
                StopCoroutine(CheckPlayersInPortal());
                isCheckingForPlayers = false;
            }
        }
        
        if (other.GetComponent<PhotonView>().IsMine)
        {
            poisonPickupTimer = 0f;
            //HUDManager.Instance.SetPoisonPickupProgress(0);
            //HUDManager.Instance.DisablePoisonPickupLoader();
        }
    }

}
