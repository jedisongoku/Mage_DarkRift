using DarkRift;
using UnityEngine;
using Cinemachine;
using DarkRift.Server;
using System.Collections;

public class PlayerMovementManager : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;
    public float clientWalkSpeed = 15f;
    private Player player;
    private PlayerParticleManager playerParticleManager;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;

    private Quaternion m_Rotation;
    private Vector3 m_NetworkPosition = Vector3.zero;
    private float m_networkHorizontal = 0;
    private float m_networkVertical = 0;
    private Vector3 m_Movement = Vector3.zero;
    private Vector3 m_networkMovement = Vector3.zero;
    private float animatorSpeed;

    private float walkSpeed;
    private float turnSpeed;
    private float horizontal;
    private float vertical;
    private float fireTimer;
    private float movementTimeFrame;
    private bool correctPosition;
    private bool IsChilled { get; set; }

    void Awake()
    {
        player = GetComponent<Player>();
        playerParticleManager = GetComponent<PlayerParticleManager>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_NetworkPosition = m_Rigidbody.position;

        SetPlayerBaseStats(); 
    }

    public void RespawnPlayer()
    {
        SetPlayerBaseStats();
    }

    void SetPlayerBaseStats()
    {
        walkSpeed = PlayerBaseStats.Instance.WalkSpeed;
        turnSpeed = PlayerBaseStats.Instance.TurnSpeed;
        IsChilled = false;
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
                horizontal = (float)System.Math.Round(Input.GetAxis("Horizontal"), 1);
                vertical = (float)System.Math.Round(Input.GetAxis("Vertical"), 1);
                SendMovementMessage();
            }

            m_Movement.Set(m_networkHorizontal, 0f, m_networkVertical);
            m_Movement.Normalize();

            //Debug.Log(m_networkHorizontal + " - " + m_networkVertical);
            m_Animator.SetFloat("Horizontal", m_networkHorizontal);
            m_Animator.SetFloat("Vertical", m_networkVertical);
            animatorSpeed = m_Animator.deltaPosition.magnitude > 0.2 ? 0.3f : m_Animator.deltaPosition.magnitude > 0.025 ? 0.1f : 0;
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);

        }
        
    }


    void OnAnimatorMove()
    {       
        if(m_Movement == Vector3.zero) m_Rigidbody.velocity = Vector3.zero;

        if(!player.IsDead)
        {
            if (!player.IsServer)
            {
                m_Rigidbody.MovePosition(m_Rigidbody.position + m_networkMovement * animatorSpeed * walkSpeed);
                m_Rigidbody.position = Vector3.MoveTowards(m_Rigidbody.position, m_NetworkPosition, Time.fixedDeltaTime);
                if ((m_Rigidbody.position - m_NetworkPosition).magnitude > 2f)
                {
                    m_Rigidbody.position = m_NetworkPosition;
                }
            }
            else
            {
                m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * animatorSpeed * walkSpeed);
                SendMovementMessage();
            }
        }
        

        

        if (fireTimer > 0.2f)
        {
            m_Rigidbody.MoveRotation(m_Rotation);
        }
        
    }

    //Client-Side
    public void SetMovement(Vector3 _networkPosition, Vector3 _movement, float _horizontal, float _vertical, float _walkSpeed)
    {
        m_NetworkPosition = _networkPosition;
        m_networkMovement = _movement;
        m_networkHorizontal = _horizontal;
        m_networkVertical = _vertical;
        walkSpeed = _walkSpeed;
        
    }

    //Server-Side
    public void SetMovement(float _horizontal, float _vertical)
    {
        m_networkHorizontal = _horizontal;
        m_networkVertical = _vertical;
    }


    public void TurnForAttack(Vector3 _mousePosition)
    {
        fireTimer = 0;
        transform.LookAt(_mousePosition);
    }

    void SendMovementMessage()
    {
        
        if (player.IsControllable)
        {
            MovementMessageModel newMessage = new MovementMessageModel()
            {
                Horizontal = horizontal,
                Vertical = vertical
            };
            using (Message message = Message.Create(NetworkTags.MovePlayerTag, newMessage))
                player.Client.SendMessage(message, SendMode.Unreliable);
        }
        else if(player.IsServer)
        {

            MovementMessageModel newMessage = new MovementMessageModel()
            {
                NetworkID = (ushort)player.ID,
                Horizontal = m_networkHorizontal,
                Vertical = m_networkVertical,
                Pos_X = m_Rigidbody.position.x,
                Pos_Z = m_Rigidbody.position.z,
                Move_X = m_Movement.x,
                Move_Z = m_Movement.z,
                WalkSpeed = walkSpeed
                
            };

            using (Message message = Message.Create(NetworkTags.MovePlayerTag, newMessage))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Unreliable);
        } 
    }

    public void ApplyChill(float _duration, ushort _runeID)
    {
        if (!IsChilled)
        {
            IsChilled = true;
            playerParticleManager.Chill(true);
            StartCoroutine(Chill(_duration, _runeID));
            SendChillParticleMessage(true, _runeID);
        }
        
    }

    void SendChillParticleMessage(bool _value, ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID,
            Chill = _value
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    IEnumerator Chill(float _duration, ushort _runeID)
    {
        walkSpeed -= walkSpeed * PlayerBaseStats.Instance.ChillSlowRate;

        yield return new WaitForSeconds(_duration);

        IsChilled = false;
        SendChillParticleMessage(false, _runeID);
        playerParticleManager.Chill(false);
        walkSpeed = PlayerBaseStats.Instance.WalkSpeed;

    }

}
