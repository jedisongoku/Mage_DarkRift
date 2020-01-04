using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    private Player player;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;

    private bool isPrimary;
    private bool isSecondary;

    void Awake()
    {
        player = GetComponent<Player>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        SetBaseStats();


    }
    void SetBaseStats()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsControllable)
        {
            if (Input.GetButtonDown("Dash"))
            {
                isSecondary = true;
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(isPrimary);
                    writer.Write(isSecondary);

                    using (Message message = Message.Create(Tags.CombatPlayerTag, writer))
                        player.Client.SendMessage(message, SendMode.Unreliable);
                }
            }
        }
        
        if (isPrimary)
        {

        }
        if(isSecondary)
        {
            isSecondary = false;
            m_Animator.SetTrigger("isDashing");
        }
    }

    public void SetCombat(bool _isPrimary, bool _isSecondary)
    {
        isPrimary = _isPrimary;
        isSecondary = _isSecondary;
    }
}
