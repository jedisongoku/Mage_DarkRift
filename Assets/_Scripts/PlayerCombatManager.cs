using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Server;
using System.Collections;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    private Player player;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private PlayerMovementManager playerMovementManager;
    private PlayerHealthManager playerHealthManager;

    Plane plane = new Plane(Vector3.up, Vector3.zero);

    public GameObject primarySkillSpawnLocation;
    private int primarySkillDamage;
    private float primarySkillCooldown;
    private float secondarySkillCooldown;
    private bool isFrostbite;
    private bool isPoison;
    private bool isChill;
    private bool isBouncy;
    private bool isRage;
    private bool isMultiShot;
    private bool isFrostNova;

    private float primarySkillCooldownTimer;
    private float secondarySkillCooldownTimer;
    private float turnSpeed = 20f;
    private Vector3 mousePosition;
    

    public bool IsPrimary { get; set; }
    public bool IsSecondary { get; set; }

    void Awake()
    {
        player = GetComponent<Player>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        playerMovementManager = GetComponent<PlayerMovementManager>();
        playerHealthManager = GetComponent<PlayerHealthManager>();

        SetPlayerBaseStats();
        
    }
    void SetPlayerBaseStats()
    {
        primarySkillDamage = PlayerBaseStats.Instance.PrimarySkillDamage;
        primarySkillCooldown = PlayerBaseStats.Instance.PrimarySkillCooldown;
        secondarySkillCooldown = PlayerBaseStats.Instance.SecondarySkillCooldown;
        primarySkillCooldownTimer = primarySkillCooldown;
        secondarySkillCooldownTimer = secondarySkillCooldown;
        isFrostbite = false;
        isPoison = false;
        isChill = false;
        isBouncy = false;
        isRage = false;
        isMultiShot = false;
        isFrostNova = false;
    }

    // Update is called once per frame
    void Update()
    {
        primarySkillCooldownTimer += Time.deltaTime;
        secondarySkillCooldownTimer += Time.deltaTime;


        if (player.IsControllable && !player.IsDead)
        {
            if (Input.GetButtonDown("Dash"))
            {
                SecondarySkill();
            }
            if (Input.GetButtonDown("Fire1"))
            {
                PrimarySkill();
            }
        }

    }

    void PrimarySkill()
    {
        if (primarySkillCooldownTimer >= primarySkillCooldown)
        {
            SetFireDirection();
            primarySkillCooldownTimer = 0;
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(mousePosition.x);
                writer.Write(mousePosition.y);
                writer.Write(mousePosition.z);

                using (Message message = Message.Create(NetworkTags.PrimarySkillTag, writer))
                    player.Client.SendMessage(message, SendMode.Reliable);
            }
            //playerMovementController.SetFireDirection();
            //primarySkillCooldownTimer = 0f;
            //photonView.RPC("UsePrimarySkill", RpcTarget.AllViaServer, playerMovementController.MousePosition, primarySkillSpawnLocation.transform.position, isMultiShot, playerHealthManager.Rage);
        }
    }

    public void PrimarySkillMessageReceived(float _x, float _y, float _z)
    {
        primarySkillCooldownTimer = 0;
        mousePosition = new Vector3(_x, _y, _z);
        StartCoroutine(UsePrimarySkill(0f));
    }

    IEnumerator UsePrimarySkill(float _delayTime)
    {
        playerMovementManager.TurnForAttack(mousePosition);
        m_Animator.SetTrigger("Attacking");
        Vector3 heading = mousePosition - primarySkillSpawnLocation.transform.position;
        Vector3 direction = heading / heading.magnitude;

        yield return new WaitForSeconds(_delayTime);

        GameObject obj = ObjectPooler.Instance.GetPrimarySkillPrefab();
        //if (obj == null) StopCoroutine(PrimarySkill());

        obj.transform.position = primarySkillSpawnLocation.transform.position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        obj.GetComponent<PrimarySkillController>().SetParticleMoveDirection = new Vector3(direction.x, 0, direction.z);
        obj.GetComponent<PrimarySkillController>().Traveling = true;
        obj.GetComponent<PrimarySkillController>().PlayerOrigin = player.ID;
        if(player.IsServer) obj.GetComponent<PrimarySkillController>().ServerClient = player.ServerClient;


        /*
        obj.GetComponent<PrimarySkillController>().PlayerOrigin = photonView.ViewID;
        obj.GetComponent<PrimarySkillController>().DamageDone = primarySkillDamage;
        if (isFrostbite) obj.GetComponent<PrimarySkillController>().Frostbite = isFrostbite;
        if (isChill) obj.GetComponent<PrimarySkillController>().Chill = isChill;
        if (isRage) obj.GetComponent<PrimarySkillController>().Rage = isRage;
        if (isFrostNova) obj.GetComponent<PrimarySkillController>().FrostNova = isFrostNova;*/


        //GameObject particle = Instantiate(primarySkillPrefab, primarySkillSpawnLocation.transform.position, Quaternion.identity) as GameObject;
    }

    void SetFireDirection()
    {
        float enter = 0f;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out enter))
        {
            mousePosition = ray.GetPoint(enter);   
        }
        //Vector3 desiredForward = Vector3.RotateTowards(transform.forward, mousePosition, turnSpeed * Time.deltaTime, 0f);
        //Quaternion m_Rotation = Quaternion.LookRotation(desiredForward);
        //transform.LookAt(mousePosition);



    }

    void SecondarySkill()
    {
        Debug.Log("Dash Timer " + secondarySkillCooldownTimer + " / " + secondarySkillCooldown);
        if (secondarySkillCooldownTimer >= secondarySkillCooldown)
        {
            secondarySkillCooldownTimer = 0;
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(true);

                using (Message message = Message.Create(NetworkTags.SecondarySkillTag, writer))
                    player.Client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    public void SecondarySkillMessageReceived()
    {
        m_Animator.SetTrigger("Dashing");
    }

    
    public void ReadyForDamage(IClient _enemyClient)
    {
        int damageToApply = primarySkillDamage;
        if (isRage) damageToApply += Mathf.RoundToInt(damageToApply * PlayerBaseStats.Instance.RageDamageRate);
        ServerManager.Instance.serverPlayersInScene[_enemyClient].GetComponent<PlayerHealthManager>().TakeDamage(damageToApply, player.ServerClient);
        //playerHealthManager.TakeDamage(damageToApply, _enemyClient);
    }
}
