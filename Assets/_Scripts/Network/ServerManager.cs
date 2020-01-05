using System.Collections.Generic;
using System.Linq;
using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;

    public XmlUnityServer gameServer;
    public GameObject playerPrefab;
    [SerializeField]
    public GameObject[] playerSkinPrefabs;

    Dictionary<IClient, PlayerServer> players = new Dictionary<IClient, PlayerServer>();
    Dictionary<IClient, GameObject> serverPlayers = new Dictionary<IClient, GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
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

        //Create a new player for the new client
        string nickname = "Mage" + Random.Range(1000, 9999);

        //Create the Message for new connected player
        PlayerServer playerServer = new PlayerServer(
            e.Client.ID,
            0f,
            0f,
            0f,
            0f,
            nickname,
            0
            );

        //Send the connected player's information to the rest of the players
        using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
        {
            playerWriter.Write(playerServer.ID);
            playerWriter.Write(playerServer.X);
            playerWriter.Write(playerServer.Z);
            playerWriter.Write(playerServer.Horizontal);
            playerWriter.Write(playerServer.Vertical);
            playerWriter.Write(playerServer.Nickname);
            playerWriter.Write(playerServer.Skin);
            
            using (Message playerMessage = Message.Create(NetworkTags.SpawnPlayerTag, playerWriter))
                foreach (IClient client in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                    client.SendMessage(playerMessage, SendMode.Reliable);
        }

        //Add the new client to the list
        players.Add(e.Client, playerServer);

        //GameObject obj = Instantiate(playerPrefab, PoisonShopManager.Instance.spawnLocations[e.Client.ID % 7].transform.position, Quaternion.identity) as GameObject;
        //GameObject skinObject = Instantiate(playerSkinPrefabs[playerServer.Skin], obj.transform);
        //serverPlayers.Add(e.Client, obj);

        //get other clients' information to the newly connected client including the new client
        //this is where new client is being instantiated with other players
        using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
        {
            Debug.Log("Players " + players.Count);
            foreach (PlayerServer player in players.Values)
            {
                playerWriter.Write(player.ID);
                playerWriter.Write(player.X);
                playerWriter.Write(player.Z);
                playerWriter.Write(player.Horizontal);
                playerWriter.Write(player.Vertical);
                playerWriter.Write(player.Nickname);
                playerWriter.Write(player.Skin);
            }
            
            using (Message playerMessage = Message.Create(NetworkTags.SpawnPlayerTag, playerWriter))
                e.Client.SendMessage(playerMessage, SendMode.Reliable);
        }
    }

    private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        players.Remove(e.Client);
        Destroy(serverPlayers[e.Client]);
        serverPlayers.Remove(e.Client);

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
        }
    }

    #region Movement Manager
    private void Movement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                float newX = reader.ReadSingle();
                float newZ = reader.ReadSingle();
                float horizontal = reader.ReadSingle();
                float vertical = reader.ReadSingle();

                PlayerServer player = players[e.Client];

                player.Horizontal = horizontal;
                player.Vertical = vertical;
                player.X = newX;
                player.Z = newZ;

                //move the copy of the character on the server
                //serverPlayers[e.Client].GetComponent<PlayerMovementController>().SetMovement(new Vector3(player.X, 0f, player.Z), player.Horizontal, player.Vertical);

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(player.X);
                    writer.Write(player.Z);
                    writer.Write(player.Horizontal);
                    writer.Write(player.Vertical);
                    message.Serialize(writer);
                }
                
                foreach (IClient c in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                    c.SendMessage(message, e.SendMode);
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
                bool state = reader.ReadBoolean();

                PlayerServer player = players[e.Client];

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(state);
                    message.Serialize(writer);
                }

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

                PlayerServer player = players[e.Client];

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(state);
                    message.Serialize(writer);
                }

                serverPlayers[e.Client].GetComponent<PlayerCombatManager>().SetCombat(false, true);
                //foreach (IClient c in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                //c.SendMessage(message, e.SendMode);
                foreach (IClient c in gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, e.SendMode);
            }
        }
    }

    #endregion
}
