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
    NetworkPlayerManager networkPlayerManager;

    [SerializeField]
    [Tooltip("The player prefab.")]
    GameObject playerPrefab;


    [SerializeField]
    GameObject[] spawnLocations;

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
            if (message.Tag == Tags.SpawnPlayerTag)
                SpawnPlayer(sender, e);
            else if (message.Tag == Tags.DespawnPlayerTag)
                DespawnPlayer(sender, e);
        }
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        Debug.LogWarning("SPAWNINNG");
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (message.Tag == Tags.SpawnPlayerTag)
            {
                Debug.LogWarning("Reader length " + reader.Length);
                if (reader.Length % 31 != 0)
                {
                    Debug.LogWarning("Received malformed spawn packet.");
                    return;
                }

                while (reader.Position < reader.Length)
                {
                    Debug.LogWarning("Pos " + reader.Position + "-- Length " + reader.Length);
                    ushort id = reader.ReadUInt16();
                    Vector3 position = new Vector3(reader.ReadSingle(), 0, reader.ReadSingle());
                    string nickname = reader.ReadString();
                    byte skin = reader.ReadByte();

                    Debug.LogWarning("ID " + id);
                    Debug.LogWarning("Nickname " + nickname);
                    Debug.LogWarning("skin " + skin);
                    GameObject obj = Instantiate(playerPrefab, spawnLocations[id % 7].transform.position, Quaternion.identity) as GameObject;
                    GameObject skinObject = Instantiate(playerSkinPrefabs[skin], obj.transform);
                    Player player = obj.GetComponent<Player>();
                    //movement.RebindAnimator();
                    if (id == client.ID)
                    {
                        player.IsControllable = true;
                        player.Client = client;
                        
                    }
                    else
                    {
                        player.IsControllable = false;
                    }

                    player.Nickname = nickname;
                    player.Skin = skin;

                    networkPlayerManager.Add(id, player);
                }
            }
        }
    }

    void DespawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
            networkPlayerManager.DestroyPlayer(reader.ReadUInt16());
    }

}
