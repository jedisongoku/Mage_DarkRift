using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift;
using Cinemachine;
using TMPro;

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
    [SerializeField] GameObject playerUI;
    [SerializeField] MeshRenderer playerBaseRenderer;
    [SerializeField] Material playerBaseColor;
    [SerializeField] TextMeshProUGUI playerName;


    private bool isDead = false;
    private bool isServer = false;
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
        //in the meantime, disable skin here
        playerSkin.SetActive(false);
        playerUI.SetActive(false);
        playerBaseRenderer.enabled = false;
        cinemachineCamera.Follow = null;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(true);

            using (Message message = Message.Create(NetworkTags.RespawnPlayerTag, writer))
                Client.SendMessage(message, SendMode.Reliable);
        }
    }

    #region Server Only Calls
    public void RespawnPlayer()
    {
        Debug.Log("Server only RespawnPlayer Start");
        int spawnLocationIndex = Random.Range(0, GameManager.Instance.spawnLocations.Length);
        transform.position = GameManager.Instance.spawnLocations[spawnLocationIndex].transform.position;
        isDead = false;
        playerHealthManager.RespawnPlayer();
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
        transform.position = GameManager.Instance.spawnLocations[_spawnLocation].transform.position;
        isDead = false;
        playerHealthManager.RespawnPlayer();
        GetComponent<Animator>().SetTrigger("Respawn");
        playerSkin.SetActive(true);
        playerUI.SetActive(true);
        playerBaseRenderer.enabled = true;
        if(IsControllable)
        {
            Invoke("ReFollowCamera", 1f);
        }
        
    }

    void ReFollowCamera()
    {
        cinemachineCamera.Follow = transform;
    }
}
