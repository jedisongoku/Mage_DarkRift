using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;
    public UnityClient client;

    Dictionary<ushort, Player> networkPlayers = new Dictionary<ushort, Player>();
    public GameObject localPlayer;

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
            else if (message.Tag == NetworkTags.PrimarySkillTag)
                PrimarySkill(sender, e);
            else if (message.Tag == NetworkTags.SecondarySkillTag)
                SecondarySkill(sender, e);
            else if (message.Tag == NetworkTags.HealthPlayerTag)
                Health(sender, e);
            else if (message.Tag == NetworkTags.RespawnPlayerTag)
                Respawn(sender, e);
            else if (message.Tag == NetworkTags.ScoreboardTag)
                Scoreboard(sender, e);
        }
    }

    void Scoreboard(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Scoreboard message " + e.GetMessage());
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ScoreboardMessageModel newMessage = reader.ReadSerializable<ScoreboardMessageModel>();
                ushort killerId = newMessage.KillerID;
                Debug.Log("score returned in the message " + newMessage.Score);
                ushort score = (ushort)newMessage.Score;
                ushort victimId = newMessage.VictimID;

                ScoreManager.Instance.UpdateScoreboard(killerId, score, victimId);
            }
        }
    }

    private void Respawn(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                RespawnPlayerMessageModel newMessage = reader.ReadSerializable<RespawnPlayerMessageModel>();
                ushort id = newMessage.NetworkID;
                ushort spawnLocation = (ushort)newMessage.SpawnLocation;


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<Player>().RespawnPlayer(spawnLocation);
            }
        }
    }

    void Health(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Health Message " + e.GetMessage());
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                HealthMessageModel newMessage = reader.ReadSerializable<HealthMessageModel>();
                ushort id = newMessage.NetworkID;
                Debug.Log("Health returned in the message " + newMessage.Health);
                ushort health = (ushort)newMessage.Health;

                //ushort id = reader.ReadUInt16();
                //ushort health = reader.ReadUInt16();
                

                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerHealthManager>().HealthMessageReceived(health);
            }
        }
    }

    void Movement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                MovementMessageModel newMessage = reader.ReadSerializable<MovementMessageModel>();
                ushort id = newMessage.NetworkID;
                float horizontal = newMessage.Horizontal;
                float vertical = newMessage.Vertical;
                float pos_x = newMessage.Pos_X;
                float pos_z = newMessage.Pos_Z;
                float move_x = newMessage.Move_X;
                float move_z = newMessage.Move_Z;


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerMovementManager>().SetMovement(new Vector3(pos_x,0,pos_z), new Vector3(move_x,0,move_z), horizontal, vertical);
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
                    networkPlayers[id].GetComponent<PlayerCombatManager>().SecondarySkillMessageReceived();
            }
        }
    }

    void PrimarySkill(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort id = reader.ReadUInt16();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();


                if (networkPlayers.ContainsKey(id))
                    networkPlayers[id].GetComponent<PlayerCombatManager>().PrimarySkillMessageReceived(x, y, z);
            }
        }
    }

    public void DestroyPlayer(ushort id)
    {
        Player o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
        ScoreManager.Instance.RemovePlayer(id, false);
    }

    public int TotalPlayer
    {
        get
        {
            return networkPlayers.Count;
        }
    }

    
}
