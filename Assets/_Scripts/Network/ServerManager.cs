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

    Dictionary<IClient, PlayerServer> players = new Dictionary<IClient, PlayerServer>();

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        gameServer = GetComponent<XmlUnityServer>();

        gameServer.Server.ClientManager.ClientConnected += ClientConnected;
        gameServer.Server.ClientManager.ClientDisconnected += ClientDisconnected;

        SceneManager.LoadScene("PoisonShop", LoadSceneMode.Additive);
        DontDestroyOnLoad(this);
    }

    private void ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += MessageReceived;

        //Create a new player for the new client
        string nickname = "Mage" + Random.Range(1000, 9999);

        //Create the Message for new connected player
        PlayerServer playerServer = new PlayerServer
        {
            ID = e.Client.ID,
            X = 0f,
            Z = 0f,
            Horizontal = 0f,
            Vertical = 0f,
            Nickname = "Mage" + Random.Range(1000, 9999),
            Skin = 0
        };



        //Send the connected player's information to the rest of the players
        using (Message m = Message.Create(NetworkTags.SpawnPlayerTag, playerServer))
        {
            using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, playerServer))
            {
                foreach (IClient client in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                    client.SendMessage(newPlayerMessage, SendMode.Reliable);
            }
        }

        //Add the new client to the list
        players.Add(e.Client, playerServer);

        //get other clients' information to the newly connected client
        using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
        {
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

            using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                e.Client.SendMessage(playerMessage, SendMode.Reliable);
        }
    }

    private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        players.Remove(e.Client);

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(e.Client.ID);

            using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
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
            if (message.Tag == Tags.MovePlayerTag)
                Movement(sender, e);
            else if (message.Tag == Tags.CombatPlayerTag)
                Combat(sender, e);
        }
    }

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

    private void Combat(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                bool primary = reader.ReadBoolean();
                bool secondary = reader.ReadBoolean();

                PlayerServer player = players[e.Client];


                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(primary);
                    writer.Write(secondary);
                    message.Serialize(writer);
                }

                foreach (IClient c in gameServer.Server.ClientManager.GetAllClients().Where(x => x != e.Client))
                    c.SendMessage(message, e.SendMode);
            }
        }
    }


}
