using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;

public class PoisonShopManager : MonoBehaviour
{
    public static PoisonShopManager Instance;
    [Header("Shop Attributes")]
    [SerializeField] int poisonTime = 150;
    [SerializeField] GameObject shrine;
    [SerializeField] Material shrineDefaultMaterial;
    [SerializeField] Material[] shrineMaterials;
    [SerializeField] GameObject[] captureAreas;
    [SerializeField] SphereCollider pickupCollider;


    int poisonTimer;
    float poisonPickupTimer;
    bool isCheckingForPlayers = false;
    bool IsServer { get; set; }
    List<Collider> players = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        if(ServerManager.Instance != null)
        {
            IsServer = true;
            InitializePoisonTimer();
        }
    }

    void InitializePoisonTimer()
    {
        poisonTimer = poisonTime;
        StartCoroutine(PoisonTimerServer());
    }
    
    IEnumerator PoisonTimerServer()
    {

        yield return new WaitForSeconds(1f);
        poisonTimer -= 1;

        if (poisonTimer <= 0)
        {
            //Enable the next poison pickup
            //Disable the timer
            SendPoisonShopMessage();

        }
        else
        {
            StartCoroutine(PoisonTimerServer());
        }

        
    }

    public void SendPoisonShopMessage()
    {

    }



    IEnumerator PoisonTimerClient()
    {

        HUDManager.Instance.UpdatePoisonTimer(poisonTimer);
        yield return new WaitForSeconds(1f);
        poisonTimer -= 1;

        if (poisonTimer <= 0)
        {
            //Enable the next poison pickup
            EnablePoisonArea(1);//make it random
            //Disable the timer
            HUDManager.Instance.DisablePoisonTimer();
        }
        else
        {
            StartCoroutine(PoisonTimerClient());
        }
    }

    public void EnablePoisonArea(int _index)
    {
        for (int i = 0; i < Areas.Length; i++)
        {
            if (i == _index)
            {
                captureAreas[i].SetActive(true);
                shrine.GetComponent<MeshRenderer>().material = shrineMaterials[i];
                pickupCollider.enabled = true;
            }
            else
            {
                captureAreas[i].SetActive(false);
            }
        }
    }

    void DisablePoisonArea()
    {
        for (int i = 0; i < captureAreas.Length; i++)
        {
            captureAreas[i].SetActive(false);

        }

        shrine.GetComponent<MeshRenderer>().material = shrineDefaultMaterial;
        pickupCollider.enabled = false;
    }

    public void ActivatePoison(int _playerID)
    {
        DisablePoisonArea();
        SendPoisonShopMessage();
        ApplyPoison();
        InitializePoisonTimer();

        
    }

    void ApplyPoison()
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
                if (players[i].enabled == false)
                {
                    players.Remove(players[i]);
                }

            }
            else
            {
                players.Remove(players[i]);
            }
        }
        if (players.Count > 1)
        {
            StartCoroutine(CheckPlayersInPortal());
        }
        else
        {
            isCheckingForPlayers = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        players.Add(other);
        if (players.Count > 1 && !isCheckingForPlayers)
        {
            isCheckingForPlayers = true;
            StartCoroutine(CheckPlayersInPortal());
        }
        poisonPickupTimer = 0f;
        if (other.GetComponent<Player>().IsControllable)
        {
            HUDManager.Instance.EnablePoisonPickupLoader();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (players.Count == 1)
        {
            poisonPickupTimer += Time.deltaTime;
            if (other.GetComponent<Player>().IsControllable)
            {
                HUDManager.Instance.SetPoisonPickupProgress(poisonPickupTimer / PlayerBaseStats.Instance.PoisonPickupTime);
            }

            if (poisonPickupTimer >= PlayerBaseStats.Instance.PoisonPickupTime)
            {
                //Activate poison for everyone other than who activated
                if (IsServer)
                {
                    ActivatePoison(other.GetComponent<Player>().ID);
                }
                
                
                if (other.GetComponent<Player>().IsControllable)
                {
                    HUDManager.Instance.DisablePoisonPickupLoader();
                }

            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            players.Remove(other);
            if (isCheckingForPlayers)
            {
                StopCoroutine(CheckPlayersInPortal());
                isCheckingForPlayers = false;
            }
        }

        if (other.GetComponent<Player>().IsControllable)
        {
            poisonPickupTimer = 0f;
            HUDManager.Instance.SetPoisonPickupProgress(0);
            HUDManager.Instance.DisablePoisonPickupLoader();
        }
    }

    public Material[] ShrineMaterials
    {
        get
        {
            return shrineMaterials;
        }
    }

    public Material ShrineDefaultMaterials
    {
        get
        {
            return shrineDefaultMaterial;
        }
    }

    public GameObject[] Areas
    {
        get
        {
            return captureAreas;
        }
    }

    public GameObject Shrine
    {
        get
        {
            return shrine;
        }
    }

    public int PoisonTime
    {
        get
        {
            return poisonTime;
        }
    }

    public SphereCollider Collider
    {
        get
        {
            return pickupCollider;
        }
    }
}
