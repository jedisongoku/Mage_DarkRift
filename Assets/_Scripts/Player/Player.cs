using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift;
using Cinemachine;
using TMPro;
using System.Collections;

public class Player : MonoBehaviour
{
    public UnityClient Client { get; set; }
    public IClient ServerClient { get; set; }

    public GameObject playerSkin { get; set; }
    public int ID { get; set; }
    public bool IsControllable { get; set; }

    public bool IsServer { get; set; }
    public string Nickname { get; set; }
    public byte Skin { get; set; }

    [SerializeField] PlayerCombatManager playerCombatManager;
    [SerializeField] PlayerHealthManager playerHealthManager;
    [SerializeField] PlayerMovementManager playerMovementManager;
    [SerializeField] PlayerRuneManager playerRuneManager;
    [SerializeField] GameObject playerUI;
    [SerializeField] MeshRenderer playerBaseRenderer;
    [SerializeField] Material playerBaseColor;
    [SerializeField] TextMeshProUGUI playerName;


    private bool isDead = false;
    private bool isServer = false;
    private int respawnTimerTick;
    private bool IsReadyForRespawn { get; set; }
    private bool CanRespawn { get; set; }
    private CinemachineVirtualCamera cinemachineCamera;


    public bool IsDead
    {
        get
        {
            return isDead;
        }
        set
        {
            isDead = value;
        }
    }

    private void Start()
    {
        if (IsControllable)
        {
            cinemachineCamera = GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
            cinemachineCamera.Follow = transform;
            playerBaseRenderer.material = playerBaseColor;
            

        }
        if (!IsServer) playerName.text = Nickname;

    }
    public void SendRespawnMessage()
    {
        cinemachineCamera.Follow = null;

        /*
        playerSkin.SetActive(false);
        playerUI.SetActive(false);
        playerBaseRenderer.enabled = false;
        
        */

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(true);

            using (Message message = Message.Create(NetworkTags.RespawnPlayerTag, writer))
                Client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void ReadyForRespawn()
    {
        IsReadyForRespawn = true;
        if(CanRespawn)
        {
            RespawnPlayer();
        }
    }

    #region Server Only Calls
    public void RespawnPlayer()
    {
        

        Debug.Log("Server only RespawnPlayer Start");
        int spawnLocationIndex = Random.Range(0, GameManager.Instance.spawnLocations.Length);
        transform.position = GameManager.Instance.spawnLocations[spawnLocationIndex].transform.position;
        isDead = false;
        IsReadyForRespawn = false;

        playerHealthManager.RespawnPlayer();
        playerCombatManager.RespawnPlayer();
        playerMovementManager.RespawnPlayer();
        playerRuneManager.RestartPlayerRunes();
        GetComponent<Animator>().SetTrigger("Respawn");
        playerSkin.SetActive(true);

        Debug.Log("Server only RespawnPlayer Notify everyone");
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            RespawnPlayerMessageModel newMessage = new RespawnPlayerMessageModel()
            {
                NetworkID = (ushort)ID,
                SpawnLocation = spawnLocationIndex
            };

            using (Message message = Message.Create(NetworkTags.RespawnPlayerTag, newMessage))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Reliable);
        }
    }

    #endregion

    public void RespawnPlayer(int _spawnLocation)
    {
        playerSkin.SetActive(false);
        playerUI.SetActive(false);
        playerBaseRenderer.enabled = false;

        if (IsControllable)
        {
            HUDManager.Instance.LoadingOnRespawn();
            Invoke("ReFollowCamera", 0.25f);
        }

        IsReadyForRespawn = true;

        transform.position = GameManager.Instance.spawnLocations[_spawnLocation].transform.position;
        isDead = false;
        playerRuneManager.RestartPlayerRunes();
        playerHealthManager.RespawnPlayer();
        playerCombatManager.RespawnPlayer();
        playerMovementManager.RespawnPlayer();
        GetComponent<Animator>().SetTrigger("Respawn");
        playerSkin.SetActive(true);
        playerUI.SetActive(true);
        playerBaseRenderer.enabled = true;
        
        
    }

    public void StartRespawnTimer()
    {
        CanRespawn = false;
        respawnTimerTick = 0;
        if (IsControllable) HUDManager.Instance.SetRespawnTimer(PlayerBaseStats.Instance.RespawnTime - respawnTimerTick);
        
        StartCoroutine(RespawnTimer());

    }

    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(1f);
        respawnTimerTick++;

        if(IsControllable)
        {
            HUDManager.Instance.SetRespawnTimer(PlayerBaseStats.Instance.RespawnTime - respawnTimerTick);
        }

        if(respawnTimerTick >= PlayerBaseStats.Instance.RespawnTime)
        {
            CanRespawn = true;
            if(IsReadyForRespawn && IsServer)
            {
                RespawnPlayer();
            }
        }
        else
        {
            StartCoroutine(RespawnTimer());
        }

    }

    void ReFollowCamera()
    {
        cinemachineCamera.Follow = transform;
    }
}
