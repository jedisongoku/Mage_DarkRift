using DarkRift;
using UnityEngine;
using Cinemachine;
using DarkRift.Server;

public class PlayerMovementManager : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;
    public float clientWalkSpeed = 20f;
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
    private float movementTimeFrame;
    private bool correctPosition;


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
        if(!player.IsServer)
        {
            //m_Rigidbody.isKinematic = true;
        }
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
            }

            m_Movement.Set(horizontal, 0f, vertical);
            m_Movement.Normalize();
            
            m_Animator.SetFloat("Horizontal", horizontal);
            m_Animator.SetFloat("Vertical", vertical);
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);

        }
        
    }


    void OnAnimatorMove()
    {
        
        if (player.IsServer)
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude * walkSpeed);
        }
        else
        {

            Debug.Log("Magnitude " + (m_Rigidbody.position - m_NetworkPosition).magnitude);
            if((m_Rigidbody.position - m_NetworkPosition).magnitude <= 0.02f)
            {
                m_NetworkPosition = m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude * walkSpeed;
            }
            m_Rigidbody.position = Vector3.MoveTowards(m_Rigidbody.position, m_NetworkPosition, Time.fixedDeltaTime * clientWalkSpeed);
        }
        
        SendMovementMessage();
        
        if (fireTimer > 0.2f)
        {
            m_Rigidbody.MoveRotation(m_Rotation);
        }
        
    }

    public void SetMovement(Vector3 _networkPosition, float _horizontal, float _vertical, bool _isServer)
    {
        m_NetworkPosition = _networkPosition;
        horizontal = _horizontal;
        vertical = _vertical;
    }


    public void TurnForAttack(Vector3 _mousePosition)
    {
        fireTimer = 0;
        transform.LookAt(_mousePosition);
    }

    void SendMovementMessage()
    {
        if(player.IsControllable)
        {
            MovementMessageModel newMessage = new MovementMessageModel()
            {
                Horizontal = horizontal,
                Vertical = vertical,
                X = m_Rigidbody.position.x,
                Z = m_Rigidbody.position.z
            };
            using (Message message = Message.Create(NetworkTags.MovePlayerTag, newMessage))
                player.Client.SendMessage(message, SendMode.Unreliable);
        }
        else if(player.IsServer)
        {

            MovementMessageModel newMessage = new MovementMessageModel()
            {
                NetworkID = (ushort)player.ID,
                Horizontal = horizontal,
                Vertical = vertical,
                X = m_Rigidbody.position.x,
                Z = m_Rigidbody.position.z
            };

            using (Message message = Message.Create(NetworkTags.MovePlayerTag, newMessage))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Unreliable);
        } 
    }

}
