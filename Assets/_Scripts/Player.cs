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


    public bool IsDead
    {
        get
        {
            return IsDead;
        }
        set
        {
            IsDead = value;
            if(IsDead)
            {

            }
        }
    }


}
