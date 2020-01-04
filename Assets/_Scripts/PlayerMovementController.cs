using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;
using Cinemachine;

public class PlayerMovementController : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;
    
    public bool IsControllable { get; set; }
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;

    private Quaternion m_Rotation;
    private Vector3 m_NetworkPosition = Vector3.zero;
    private Vector3 m_Movement = Vector3.zero;

    private float walkSpeed;
    private float turnSpeed;
    private float horizontal;
    private float vertical;
    private float fireTimer;

    public UnityClient Client { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        Debug.LogWarning("Everything works");
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        SetBaseStats();
        
        
    }

    private void Start()
    {
        if (IsControllable)
        {
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        }
    }

    void SetBaseStats()
    {
        walkSpeed = PlayerBaseStats.Instance.WalkSpeed;
        turnSpeed = PlayerBaseStats.Instance.TurnSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        fireTimer += Time.deltaTime;

        if(IsControllable)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            
        }
        else
        {
            
            if((m_Rigidbody.position - m_NetworkPosition).magnitude > 3)
            {
                m_Rigidbody.position = m_NetworkPosition;
            }
            m_Rigidbody.position = Vector3.MoveTowards(m_Rigidbody.position, m_NetworkPosition, Time.fixedDeltaTime);
        }

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
        m_Animator.SetFloat("Horizontal", horizontal);
        m_Animator.SetFloat("Vertical", vertical);

        if(IsControllable)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(m_Rigidbody.position.x);
                writer.Write(m_Rigidbody.position.z);
                writer.Write(horizontal);
                writer.Write(vertical);

                using (Message message = Message.Create(Tags.MovePlayerTag, writer))
                    Client.SendMessage(message, SendMode.Unreliable);
            }
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

    public void SetMovement(Vector3 _networkPosition, float _horizontal, float _vertical)
    {
        m_NetworkPosition = _networkPosition;
        horizontal = _horizontal;
        vertical = _vertical;
    }

    public void RebindAnimator()
    {
        m_Animator.Rebind();
    }


}
