using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using System;

public class PlayerSpawnManager : MonoBehaviour
{

    const byte SPAWN_TAG = 0;

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The network player manager.")]
    ClientManager clientManager;

    [SerializeField]
    [Tooltip("The player prefab.")]
    GameObject playerPrefab;

    [SerializeField]
    public GameObject[] spawnLocations;

    [SerializeField]
    public GameObject[] playerSkinPrefabs;


    // Start is called before the first frame update
    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner");
            Application.Quit();
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }
    
        client.MessageReceived += MessageReceived;
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == NetworkTags.SpawnPlayerTag)
                SpawnPlayer(sender, e);
            else if (message.Tag == NetworkTags.DespawnPlayerTag)
                DespawnPlayer(sender, e);
        }
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        Debug.LogWarning("SPAWNINNG");
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 39 != 0)
            {
                Debug.LogWarning("Received malformed spawn packet.");
                return;
            }
            while (reader.Position < reader.Length)
            {
                Debug.LogWarning("Pos " + reader.Position + "-- Length " + reader.Length);
                ServerPlayer content = reader.ReadSerializable<ServerPlayer>();

                ushort id = content.ID;
                Vector3 position = new Vector3(content.X, 0, content.Z);
                float h = content.Horizontal;
                float v = content.Vertical;
                string nickname = content.Nickname;
                byte skin = content.Skin;

                Debug.Log("spawning on this position - " + GameManager.Instance.spawnLocations[id % 7].transform.position);
                GameObject obj = Instantiate(playerPrefab, GameManager.Instance.spawnLocations[id % 7].transform.position, Quaternion.identity) as GameObject;
                Debug.Log("Player position " + obj.transform.position);
                GameObject skinObject = Instantiate(playerSkinPrefabs[skin], obj.transform);
                Player player = obj.GetComponent<Player>();

                player.playerSkin = skinObject;
                player.IsServer = false;
                player.Nickname = nickname;
                player.Skin = skin;
                if (id == client.ID)
                {
                    clientManager.localPlayer = obj;
                    player.IsControllable = true;
                    player.Client = client;
                    player.ID = client.ID;

                }
                else
                {
                    player.ID = id;
                    player.IsControllable = false;
                }
                

                clientManager.Add(id, player);
            }
        }

        ObjectPooler.Instance.GenerateParticlesForClient();
    }

    void DespawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
            clientManager.DestroyPlayer(reader.ReadUInt16());
    }

}
