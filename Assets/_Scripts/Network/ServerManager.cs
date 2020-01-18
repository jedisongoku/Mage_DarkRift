using System.Collections.Generic;
using System.Linq;
using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;

    public XmlUnityServer gameServer;
    public GameObject playerPrefab;
    [SerializeField]
    public GameObject[] playerSkinPrefabs;

    Dictionary<IClient, ServerPlayer> players = new Dictionary<IClient, ServerPlayer>();
    public Dictionary<IClient, GameObject> serverPlayersInScene = new Dictionary<IClient, GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        gameServer = GetComponent<XmlUnityServer>();

        gameServer.Server.ClientManager.ClientConnected += ClientConnected;
        gameServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;

        SceneManager.LoadScene("PoisonShop");
        DontDestroyOnLoad(this);
    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += MessageReceived;

        
        string nickname = "Mage#" + UnityEngine.Random.Range(1000, 9999);
        //Create a new player for the new client
        ServerPlayer playerServer = new ServerPlayer
        {
            ID = e.Client.ID,
            X = 0f,
            Z = 0f,
            Horizontal = 0f,
            Vertical = 0f,
            Nickname = nickname,
            Skin = 0,
        };

        //Create and send message for new connected player to all other players
        using (DarkRiftWriter playerMessage = DarkRiftWriter.Create())
        {
            using (Message myMessage = Message.Create(NetworkTags.SpawnPlayerTag, playerServer))
            {
                foreach (IClient client in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                    client.SendMessage(myMessage, SendMode.Reliable);
            }
        }

        GameObject playerObject = Instantiate(playerPrefab, GameManager.Instance.spawnLocations[playerServer.ID % 7].transform.position, Quaternion.identity) as GameObject;
        GameObject skinObject = Instantiate(playerSkinPrefabs[playerServer.Skin], playerObject.transform);
        playerObject.GetComponent<Player>().IsServer = true;
        playerObject.GetComponent<Player>().ID = e.Client.ID;
        playerObject.GetComponent<Player>().ServerClient = e.Client;
        playerObject.GetComponent<Player>().playerSkin = skinObject;


        //Add playerObject to the list
        serverPlayersInScene.Add(e.Client, playerObject);
        ScoreManager.Instance.AddPlayer(e.Client.ID, new ScorePlayer(e.Client.ID, playerServer.Nickname, 0), true);
        //ScoreManager.Instance.AddPlayer(e.Client);

        //Add the new client to the list
        players.Add(e.Client, playerServer);

        //get all other player data and send it to new connected player
        using (DarkRiftWriter playerMessageWriter = DarkRiftWriter.Create())
        {
            foreach (ServerPlayer player in players.Values)
            {
                playerMessageWriter.Write(player);
            }

            using (Message playerMessage = Message.Create(NetworkTags.SpawnPlayerTag, playerMessageWriter))
                e.Client.SendMessage(playerMessage, SendMode.Reliable);
        }

        ObjectPooler.Instance.GenerateParticlesForServer();
    }

    private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        players.Remove(e.Client);
        Destroy(serverPlayersInScene[e.Client]);
        serverPlayersInScene.Remove(e.Client);
        ScoreManager.Instance.RemovePlayer(e.Client.ID, true);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(e.Client.ID);

            using (Message message = Message.Create(NetworkTags.DespawnPlayerTag, writer))
            {
                foreach (IClient client in gameServer.Server.ClientManager.GetAllClients())
                    client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == NetworkTags.MovePlayerTag)
                Movement(sender, e);
            else if (message.Tag == NetworkTags.PrimarySkillTag)
                PrimarySkill(sender, e);
            else if (message.Tag == NetworkTags.SecondarySkillTag)
                SecondarySkill(sender, e);
            else if (message.Tag == NetworkTags.RespawnPlayerTag)
                Respawn(sender, e);
        }
    }

    private void Respawn(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                bool respawn = reader.ReadBoolean();

                serverPlayersInScene[e.Client].GetComponent<Player>().RespawnPlayer();

            }
        }
    }

    #region Movement Manager
    private void Movement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                MovementMessageModel newMessage = reader.ReadSerializable<MovementMessageModel>();
                //float newX = reader.ReadSingle();
                //float newZ = reader.ReadSingle();
                float horizontal = newMessage.Horizontal;
                float vertical = newMessage.Vertical;

                ServerPlayer player = players[e.Client];

                //move the copy of the character on the server
                serverPlayersInScene[e.Client].GetComponent<PlayerMovementManager>().SetMovement(horizontal, vertical);

            }
        }
    }

    #endregion 

    #region Combat Manager

    private void PrimarySkill(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();

                ServerPlayer player = players[e.Client];

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(x);
                    writer.Write(y);
                    writer.Write(z);
                    message.Serialize(writer);
                }

                serverPlayersInScene[e.Client].GetComponent<PlayerCombatManager>().PrimarySkillMessageReceived(x, y, z);

                //foreach (IClient c in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                //c.SendMessage(message, e.SendMode);
                foreach (IClient c in gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, e.SendMode);
                
            }
        }
    }
    private void SecondarySkill(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                bool state = reader.ReadBoolean();

                ServerPlayer player = players[e.Client];

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(state);
                    message.Serialize(writer);
                }

                serverPlayersInScene[e.Client].GetComponent<PlayerCombatManager>().SecondarySkillMessageReceived();

                //foreach (IClient c in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                //c.SendMessage(message, e.SendMode);

                foreach (IClient c in gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, e.SendMode);
            }
        }
    }

    #endregion

    #region Health Manager
        
    #endregion
}
