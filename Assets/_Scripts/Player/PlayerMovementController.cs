using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerMovementController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private float walkSpeed = 1f;
    private DynamicJoystick joystick;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    //AudioSource m_AudioSource;
    Vector3 m_Movement;
    
    Vector3 aimLocation;
    Vector3 lookPosition;
    Vector3 attackDirection;
    Quaternion m_Rotation = Quaternion.identity;
    Plane plane = new Plane(Vector3.up, Vector3.zero);

    Vector3 networkPosition;
    Quaternion networkRotation;
    Vector2 movement;
    Vector2 newMovement;
    float fireTimer = 1f;
    float horizontal;
    float vertical;
    bool attack;

    [Header("Runes")]
    [SerializeField] private GameObject chillParticle;
    bool isChill = false;

    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        SetPlayerBaseStats();
        joystick = HUDManager.Instance.MovementJoystick;
        //PhotonNetwork.SendRate = 30;
        //PhotonNetwork.SerializationRate = 20;


        //m_AudioSource = GetComponent<AudioSource>();

    }

    void OnDisable()
    {
        chillParticle.SetActive(false);
    }

    void SetPlayerBaseStats()
    {
        walkSpeed = PlayFabDataStore.playerBaseStats.WalkSpeed;
        isChill = false;
    }

    void Update()
    {
        if(attack)
        {
            attack = false;
            fireTimer = 0f;
        }
        
    }

    public void StartChill(float _duration)
    {
        if(!isChill)
        {
            Debug.Log("Starting Chill...");
            photonView.RPC("StartChill_RPC", RpcTarget.All, _duration);
        }
    }

    [PunRPC]
    void StartChill_RPC(float _duration)
    {
        StartCoroutine(Chill(_duration));
    }

    IEnumerator Chill(float _duration)
    {
        chillParticle.SetActive(true);
        walkSpeed -= walkSpeed * PlayFabDataStore.playerBaseStats.ChillSlowRate; 

        yield return new WaitForSeconds(_duration);

        isChill = false;
        chillParticle.SetActive(false);
        walkSpeed = PlayFabDataStore.playerBaseStats.WalkSpeed;

    }

    public void SetFireDirection(Vector3 _aimLocation)
    {
        if (photonView.IsMine)
        {
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, _aimLocation, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);
            transform.LookAt(_aimLocation);
        }

    }

    public Vector3 AimLocation
    {
        get
        {
            return aimLocation;
        }
        set
        {
            aimLocation = value;
            transform.LookAt(aimLocation);
        }
    }

    void FixedUpdate()
    {
        fireTimer += Time.fixedDeltaTime;
        if (!photonView.IsMine)
        {
            if((m_Rigidbody.position - networkPosition).magnitude > 5f)
            {
                m_Rigidbody.position = networkPosition;
            }

            m_Rigidbody.position = Vector3.MoveTowards(m_Rigidbody.position, networkPosition, Time.fixedDeltaTime);
            //m_Rigidbody.rotation = Quaternion.RotateTowards(m_Rigidbody.rotation, networkRotation, Time.fixedDeltaTime);
            //movement = Vector2.Lerp(movement, newMovement, Time.fixedDeltaTime * turnSpeed);
            movement = newMovement;
        }
        else
        {
            if(Application.isMobilePlatform)
            {
                horizontal = joystick.Horizontal;
                vertical = joystick.Vertical;
            }
            else
            {
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
            }


            movement = new Vector2(horizontal, vertical);
            
        }

        if(movement == Vector2.zero)
        {
            m_Rigidbody.velocity = Vector3.zero;
        }
        m_Movement.Set(movement.x, 0f, movement.y);
        m_Movement.Normalize();
        //Debug.Log("Movement " + m_Movement);

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
        int anim_x = movement.x < 0 ? -1 : movement.x > 0 ? 1 : 0;
        int anim_y = movement.y < 0 ? -1 : movement.y > 0 ? 1 : 0;
        m_Animator.SetFloat("Horizontal", anim_x);
        m_Animator.SetFloat("Vertical", anim_y);

        if(GetComponent<PlayerCombatManager>().IsDead)
        {
            m_Rigidbody.velocity = Vector3.zero;
        }
    }

    void OnAnimatorMove()
    {

        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude * walkSpeed);
        if (fireTimer > 0.2f)
        {
            m_Rigidbody.MoveRotation(m_Rotation);
        }     
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Rigidbody.position);
            //stream.SendNext(m_Rigidbody.rotation);
            stream.SendNext(m_Rigidbody.velocity);
            stream.SendNext(movement);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            //networkRotation = (Quaternion)stream.ReceiveNext();
            m_Rigidbody.velocity = (Vector3)stream.ReceiveNext();
            newMovement = (Vector2)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            m_Rigidbody.position += m_Rigidbody.velocity * lag;
        }
    }

    public bool Attack
    {
        set
        {
            attack = value;
        }
    }

    public void OnPlayerDeath()
    {
        StopAllCoroutines();
        chillParticle.SetActive(false);
        isChill = false;
        joystick.OnPointerUp(null);
        this.enabled = false;
    }
}
