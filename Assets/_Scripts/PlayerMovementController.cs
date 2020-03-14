using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerMovementController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private float walkSpeed = 1f;
    private VariableJoystick joystick;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    //AudioSource m_AudioSource;
    Vector3 m_Movement;
    
    Vector3 mousePosition;
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
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 20;
        

        //m_AudioSource = GetComponent<AudioSource>();
        
    }

    void Start()
    {
        SetPlayerBaseStats();
        joystick = HUDManager.Instance.MovementJoystick;
    }

    void OnDisable()
    {
        chillParticle.SetActive(false);
    }

    void SetPlayerBaseStats()
    {
        walkSpeed = PlayerBaseStats.Instance.WalkSpeed;
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
        walkSpeed -= walkSpeed * PlayerBaseStats.Instance.ChillSlowRate; 

        yield return new WaitForSeconds(_duration);

        chillParticle.SetActive(false);
        walkSpeed = PlayerBaseStats.Instance.WalkSpeed;

    }

    public void SetFireDirection(Vector3 _aimLocation)
    {
        if (photonView.IsMine)
        {
            
            float enter = 0f;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out enter))
            {
                mousePosition = ray.GetPoint(enter);
                //Debug.DrawLine(transform.position, mousePosition, Color.gray);

            }
            Debug.Log("Mouseposition " + mousePosition);
            Debug.Log("aimlocation " + _aimLocation);

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, _aimLocation, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);
            transform.LookAt(_aimLocation);

        }

    }

    public Vector3 MousePosition
    {
        get
        {
            return mousePosition;
        }
        set
        {
            mousePosition = value;
            transform.LookAt(mousePosition);
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
            movement = Vector2.Lerp(movement, newMovement, Time.fixedDeltaTime * turnSpeed);
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

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
        m_Animator.SetFloat("Horizontal", movement.x);
        m_Animator.SetFloat("Vertical", movement.y);
        



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
}
