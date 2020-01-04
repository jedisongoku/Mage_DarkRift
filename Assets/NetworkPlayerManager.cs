using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    Dictionary<ushort, Player> networkPlayers = new Dictionary<ushort, Player>();

    public void Add(ushort id, Player player)
    {
        networkPlayers.Add(id, player);
    }

    public void Awake()
    {
        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.MovePlayerTag)
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
    }

    public void DestroyPlayer(ushort id)
    {
        Player o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
    }
}
