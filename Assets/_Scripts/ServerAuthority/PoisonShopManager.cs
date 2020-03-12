using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using System.Linq;

public class PoisonShopManager : MonoBehaviour
{
    public static readonly ushort DashLock_ID = 101;
    public static readonly ushort Poison_ID = 102;
    public static readonly ushort WintersChill_ID = 103;
    public static readonly ushort DistortAim_ID = 104;

    public static PoisonShopManager Instance;
    [Header("Shop Attributes")]
    [SerializeField] int poisonTime = 150;
    [SerializeField] GameObject shrine;
    [SerializeField] Material shrineDefaultMaterial;
    [SerializeField] Material[] shrineMaterials;
    [SerializeField] GameObject[] captureAreas;
    [SerializeField] SphereCollider pickupCollider;
    [SerializeField] string[] poisonNames;

    public int PoisonID { get; set; }
    public int PoisonTimer { get; set; }
    bool IsServer { get; set; }
    IClient Player { get; set; }


    float poisonPickupTimer;
    bool isTimerOn = false;
    bool isPoisonAreaEnabled = false;
    bool isPoisonActivated = false;
    bool isCheckingForPlayers = false;
    int previousPoisonID;


    List<Collider> players = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        if(ServerManager.Instance != null)
        {
            PoisonID = -1;
            previousPoisonID = PoisonID;
            IsServer = true;
            InitializePoisonTimer();
        }
    }

    public void InitializePoisonTimer()
    {
        PoisonTimer = poisonTime;
        ClearPlayerList();

        if (IsServer)
        {
            isTimerOn = true;
            isPoisonAreaEnabled = false;
            StartCoroutine(PoisonTimerServer());
            SendPoisonShopMessage();
            isPoisonActivated = false;
        }
        else
        {
            //HUDManager.Instance.EnablePoisonTimer();
            //HUDManager.Instance.DisablePoisonPickupProgress();
            StartCoroutine(PoisonTimerClient());
        }

    }
    
    IEnumerator PoisonTimerServer()
    {
        yield return new WaitForSeconds(1f);
        PoisonTimer -= 1;

        if (PoisonTimer <= 0)
        {
            //Enable the next poison pickup
            //Disable the timer
            isPoisonAreaEnabled = true;
            isTimerOn = false;
            while(PoisonID == previousPoisonID)
            {
                PoisonID = Random.Range(0, captureAreas.Length);
            }
            previousPoisonID = PoisonID;
            EnablePoisonArea(PoisonID);
            SendPoisonShopMessage();

        }
        else
        {
            StartCoroutine(PoisonTimerServer());
        }

        
    }

    public void SendPoisonShopMessage()
    {
        PoisonShopMessageModel newMessage = new PoisonShopMessageModel()
        {
            IsTimerOn = isTimerOn,
            Time = (ushort)PoisonTimer,
            PoisonID = (ushort)PoisonID,
            IsPoisonAreaEnabled = isPoisonAreaEnabled,
            IsPoisonActivated = isPoisonActivated

        };

        using (Message message = Message.Create(NetworkTags.PoisonShopTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    public void SendPoisonShopMessageOnClientConnected(IClient _player)
    {
        PoisonShopMessageModel newMessage = new PoisonShopMessageModel()
        {
            IsTimerOn = isTimerOn,
            Time = (ushort)PoisonTimer,
            PoisonID = (ushort)PoisonID,
            IsPoisonAreaEnabled = isPoisonAreaEnabled,
            IsPoisonActivated = isPoisonActivated

        };

        using (Message message = Message.Create(NetworkTags.PoisonShopTag, newMessage))
                _player.SendMessage(message, SendMode.Reliable);
    }

    IEnumerator PoisonTimerClient()
    {
        //HUDManager.Instance.UpdatePoisonTimer(PoisonTimer);

        yield return new WaitForSeconds(1f);
        PoisonTimer -= 1;
        
        if (PoisonTimer > 0)
        {
            StartCoroutine(PoisonTimerClient());
        }
    }

    public void EnablePoisonArea(int _index)
    {
        if(!IsServer)
        {
            //HUDManager.Instance.DisablePoisonTimer();
        }
        for (int i = 0; i < captureAreas.Length; i++)
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

    public void DisablePoisonArea()
    {
        for (int i = 0; i < captureAreas.Length; i++)
        {
            captureAreas[i].SetActive(false);

        }

        shrine.GetComponent<MeshRenderer>().material = shrineDefaultMaterial;
        pickupCollider.enabled = false;
    }

    public void ActivatePoison()
    {
        isPoisonActivated = true;
        DisablePoisonArea();
        ApplyPoison();
        InitializePoisonTimer();

        
    }

    void ApplyPoison()
    {
        Invoke(poisonNames[PoisonID], 0f);
    }


    public void ClearPlayerList()
    {
        players.Clear();
        ResetPickupTimer();
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
                    if(i == 0) ResetPickupTimer();
                }

            }
            else
            {
                players.Remove(players[i]);
                if (i == 0) ResetPickupTimer();
            }
        }
        if (players.Count > 1)
        {
            StartCoroutine(CheckPlayersInPortal());
        }
        else
        {
            ResetPickupTimer();
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
        //poisonPickupTimer = 0f;
        if (other.GetComponent<Player>().IsControllable)
        {
            //HUDManager.Instance.EnablePoisonPickupProgress();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (players.Count == 1)
        {
            poisonPickupTimer += Time.deltaTime;
            if (other.GetComponent<Player>().IsControllable)
            {
                //HUDManager.Instance.SetPoisonPickupProgress(poisonPickupTimer / PlayerBaseStats.Instance.PoisonPickupTime);
            }

            if (poisonPickupTimer >= PlayerBaseStats.Instance.PoisonPickupTime)
            {
                //Activate poison for everyone other than who activated
                if (IsServer)
                {
                    Player = other.GetComponent<Player>().ServerClient;
                    ActivatePoison();
                    
                }
                
                
                if (other.GetComponent<Player>().IsControllable)
                {
                   // HUDManager.Instance.DisablePoisonPickupProgress();

                }

                ClearPlayerList();

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
            ResetPickupTimer();
            //HUDManager.Instance.DisablePoisonPickupProgress();
        }

        if (players.Count == 0) ResetPickupTimer();
    }

    void ResetPickupTimer()
    {
        poisonPickupTimer = 0;
    }

    void Poison()
    {
        foreach (IClient player in ServerManager.Instance.serverPlayersInScene.Keys.Where(x => x != Player))
            ServerManager.Instance.serverPlayersInScene[player].GetComponent<PlayerHealthManager>().ApplyPoison(Player, Poison_ID);

    }

    void WintersChill()
    {
        foreach (IClient player in ServerManager.Instance.serverPlayersInScene.Keys.Where(x => x != Player))
            ServerManager.Instance.serverPlayersInScene[player].GetComponent<PlayerMovementManager>().ApplyWintersChill(Player, WintersChill_ID);
    }

    void DashLock()
    {
        foreach (IClient player in ServerManager.Instance.serverPlayersInScene.Keys.Where(x => x != Player))
            ServerManager.Instance.serverPlayersInScene[player].GetComponent<PlayerCombatManager>().ApplyDashLock(DashLock_ID);
    }

    void DistortAim()
    {
        foreach (IClient player in ServerManager.Instance.serverPlayersInScene.Keys.Where(x => x != Player))
            ServerManager.Instance.serverPlayersInScene[player].GetComponent<PlayerCombatManager>().ApplyDistortAim(DistortAim_ID);
    }
}
