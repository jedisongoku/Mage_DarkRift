using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift.Server;

public class Player : MonoBehaviour
{
    public UnityClient Client { get; set; }
    public IClient ServerClient { get; set; }
    public int ID { get; set; }
    public bool IsControllable { get; set; }

    public bool IsServer { get; set; }
    public string Nickname { get; set; }
    public byte Skin { get; set; }

    [SerializeField]
    PlayerCombatManager playerCombatManager;
    [SerializeField]
    PlayerHealthManager playerHealthManager;
    [SerializeField]
    PlayerMovementManager playerMovementManager;
    private bool isDead = false;
    private bool isServer = false;


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


}
