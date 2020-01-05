using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;
    public UnityClient client;

    Dictionary<ushort, Player> networkPlayers = new Dictionary<ushort, Player>();

    void Awake()
    {
        
        Instance = this;
        client = GetComponent<UnityClient>();
        client.MessageReceived += MessageReceived;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        //SceneManager.LoadScene("PoisonShop", LoadSceneMode.Additive);
    }

    public void Add(ushort id, Player player)
    {
        networkPlayers.Add(id, player);
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == NetworkTags.MovePlayerTag)
                Movement(sender, e);
            else if (message.Tag == NetworkTags.SecondarySkillTag)
                SecondarySkill(sender, e);
        }
    }
    void Movement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                Vector3 position = new Vector3(reader.ReadSingle(), 0f, reader.ReadSingle());
                float horizontal = reader.ReadSingle();
                float vertical = reader.ReadSingle();

                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerMovementController>().SetMovement(position, horizontal, vertical);
            }
        }
    }

    void SecondarySkill(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                bool secondary = reader.ReadBoolean();


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerCombatManager>().SetCombat(false, secondary);
            }
        }
    }

    public void DestroyPlayer(ushort id)
    {
        Player o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
    }

    public int TotalPlayer
    {
        get
        {
            return networkPlayers.Count;
        }
    }

    
}
