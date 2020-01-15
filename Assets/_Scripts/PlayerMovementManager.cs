using DarkRift;
using UnityEngine;
using Cinemachine;
using DarkRift.Server;

public class PlayerMovementManager : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;
    
    private Player player;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;

    private Quaternion m_Rotation;
    private Vector3 m_NetworkPosition = Vector3.zero;
    private float m_networkHorizontal = 0;
    private float m_networkVertical = 0;
    private Vector3 m_Movement = Vector3.zero;
    private Vector3 m_networkMovement = Vector3.zero;

    private float walkSpeed;
    private float turnSpeed;
    private float horizontal;
    private float vertical;
    private float fireTimer;
    

    void Awake()
    {
        player = GetComponent<Player>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_NetworkPosition = m_Rigidbody.position;

        SetPlayerBaseStats(); 
    }

    void SetPlayerBaseStats()
    {
        walkSpeed = PlayerBaseStats.Instance.WalkSpeed;
        turnSpeed = PlayerBaseStats.Instance.TurnSpeed;
    }

    private void Start()
    {
        
        m_Animator.Rebind();
    }

    

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!player.IsDead)
        {
            fireTimer += Time.deltaTime;

            if (player.IsControllable)
            {
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    //send input to the server
                    MovementMessageModel newMessage = new MovementMessageModel()
                    {

                        //NetworkID = (ushort)player.ID,
                        Horizontal = horizontal,
                        Vertical = vertical
                    };

                    using (Message message = Message.Create(NetworkTags.MovePlayerTag, newMessage))
                        player.Client.SendMessage(message, SendMode.Unreliable);
                }

            }

            if (!player.IsServer)
            {
                if ((m_Rigidbody.position - m_NetworkPosition).magnitude > 3)
                {
                    m_Rigidbody.position = m_NetworkPosition;
                }
                m_Rigidbody.position = Vector3.MoveTowards(m_Rigidbody.position, m_NetworkPosition, Time.fixedDeltaTime);

                m_networkMovement.Set(m_networkHorizontal, 0f, m_networkVertical);
                m_networkMovement.Normalize();
                m_Animator.SetFloat("Horizontal", m_networkHorizontal);
                m_Animator.SetFloat("Vertical", m_networkVertical);
                Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_networkMovement, turnSpeed * Time.deltaTime, 0f);
                m_Rotation = Quaternion.LookRotation(desiredForward);

            }
            else
            {
                m_Movement.Set(horizontal, 0f, vertical);
                m_Movement.Normalize();
                m_Animator.SetFloat("Horizontal", horizontal);
                m_Animator.SetFloat("Vertical", vertical);
                Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
                m_Rotation = Quaternion.LookRotation(desiredForward);
            }
 
        }
        
    }

    void OnAnimatorMove()
    {
        
        if (player.IsServer)
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude * walkSpeed);
            SendMovementMessage();
        }
        else
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_networkMovement * m_Animator.deltaPosition.magnitude * walkSpeed);
        }
        
        if (fireTimer > 0.2f)
        {
            m_Rigidbody.MoveRotation(m_Rotation);
        }
        
    }

    //Client-Side Method
    public void SetMovement(Vector3 _networkPosition, float _horizontal, float _vertical)
    {
        m_NetworkPosition = _networkPosition;
        m_networkHorizontal = _horizontal;
        m_networkVertical = _vertical;
    }

    //Server-Side Method
    public void SetMovement(float _horizontal, float _vertical)
    {
        horizontal = _horizontal;
        vertical = _vertical;
        
    }

    public void TurnForAttack(Vector3 _mousePosition)
    {
        fireTimer = 0;
        transform.LookAt(_mousePosition);
    }

    //Server-Side Method
    void SendMovementMessage()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            MovementMessageModel newMessage = new MovementMessageModel()
            {
                NetworkID = (ushort)player.ID,
                Horizontal = horizontal,
                Vertical = vertical,
                X = m_Rigidbody.position.x,
                Z = m_Rigidbody.position.z
            };
            //writer.Write(player.ID);
            //writer.Write(playerhealth);
            //probably add the rune applications for particles

            using (Message message = Message.Create(NetworkTags.MovePlayerTag, newMessage))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Unreliable);
        }
    }

}
