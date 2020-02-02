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
    private PlayerParticleManager playerParticleManager;

    Plane plane = new Plane(Vector3.up, Vector3.zero);

    [SerializeField] GameObject dashTrail;

    public GameObject primarySkillSpawnLocation;
    public int PrimarySkillDamage { get; set; }
    public float PrimarySkillCooldown { get; set; }
    public float SecondarySkillCooldown { get; set; }
    public bool Frostbite { get; set; }
    public bool Chill { get; set; }
    public bool MultiShot { get; set; }
    public bool FrostNova { get; set; }
    public bool Poison { get; set; }
    public bool Rage { get; set; }

    public bool IsDashLocked { get; set; }

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
        playerParticleManager = GetComponent<PlayerParticleManager>();

        SetPlayerBaseStats();
        
    }

    public void RespawnPlayer()
    {
        SetPlayerBaseStats();
    }
    void SetPlayerBaseStats()
    {
        PrimarySkillDamage = PlayerBaseStats.Instance.PrimarySkillDamage;
        PrimarySkillCooldown = PlayerBaseStats.Instance.PrimarySkillCooldown;
        SecondarySkillCooldown = PlayerBaseStats.Instance.SecondarySkillCooldown;
        primarySkillCooldownTimer = PrimarySkillCooldown;
        secondarySkillCooldownTimer = SecondarySkillCooldown;
        Frostbite = false;
        Poison = false;
        Chill = false;
        Rage = false;
        MultiShot = false;
        FrostNova = false;
        IsDashLocked = false;
    }

    // Update is called once per frame
    void Update()
    {
        primarySkillCooldownTimer += Time.deltaTime;
        secondarySkillCooldownTimer += Time.deltaTime;
        


        if (player.IsControllable && !player.IsDead)
        {
            HUDManager.Instance.SetPrimarySkillCooldownUI = 1 - primarySkillCooldownTimer / PrimarySkillCooldown;
            HUDManager.Instance.SetSecondarySkillCooldownUI = 1 - secondarySkillCooldownTimer / SecondarySkillCooldown;

            if (Input.GetButtonDown("Dash") && !IsDashLocked)
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
        if (primarySkillCooldownTimer >= PrimarySkillCooldown)
        {
            SetFireDirection();
            primarySkillCooldownTimer = 0;
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(mousePosition.x);
                writer.Write(mousePosition.y);
                writer.Write(mousePosition.z);
                //writer.Write(MultiShot);

                using (Message message = Message.Create(NetworkTags.PrimarySkillTag, writer))
                    player.Client.SendMessage(message, SendMode.Reliable);
            }
            //playerMovementController.SetFireDirection();
            //primarySkillCooldownTimer = 0f;
            //photonView.RPC("UsePrimarySkill", RpcTarget.AllViaServer, playerMovementController.MousePosition, primarySkillSpawnLocation.transform.position, isMultiShot, playerHealthManager.Rage);
        }
    }

    public void PrimarySkillMessageReceived(float _x, float _y, float _z, bool _multishot)
    {
        primarySkillCooldownTimer = 0;
        mousePosition = new Vector3(_x, _y, _z);
        StartCoroutine(UsePrimarySkill(0f));
        if (!player.IsServer) MultiShot = _multishot;
        Debug.Log("Multishot " + MultiShot);
        if(MultiShot) StartCoroutine(UsePrimarySkill(PlayerBaseStats.Instance.MultishotDelayTime));
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
        if (FrostNova) obj.GetComponent<PrimarySkillController>().FrostNova = FrostNova;
        if(player.IsServer) obj.GetComponent<PrimarySkillController>().ServerClient = player.ServerClient;

    }

    void SetFireDirection()
    {
        float enter = 0f;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out enter))
        {
            mousePosition = ray.GetPoint(enter);   
        }

    }

    void SecondarySkill()
    {
        Debug.Log("Dash Timer " + secondarySkillCooldownTimer + " / " + SecondarySkillCooldown);
        if (secondarySkillCooldownTimer >= SecondarySkillCooldown)
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
        if(!IsDashLocked)
        {
            m_Animator.SetTrigger("Dashing");
            dashTrail.SetActive(false);
            dashTrail.SetActive(true);
        }
        
    }

    
    public void ReadyForDamage(IClient _enemyClient)
    {
        int damageToApply = PrimarySkillDamage;
        if (Rage) damageToApply += Mathf.RoundToInt(damageToApply * PlayerBaseStats.Instance.RageDamageRate);
        ServerManager.Instance.serverPlayersInScene[_enemyClient].GetComponent<PlayerHealthManager>().TakeDamage(damageToApply, player.ServerClient);
        if (Chill) ServerManager.Instance.serverPlayersInScene[_enemyClient].GetComponent<PlayerMovementManager>().ApplyChill(PlayerBaseStats.Instance.ChillDuration, PlayerRuneManager.Chill_ID);
        if (Frostbite) ServerManager.Instance.serverPlayersInScene[_enemyClient].GetComponent<PlayerHealthManager>().ApplyFrostbite(player.ServerClient, PlayerRuneManager.Frostbite_ID);
        //playerHealthManager.TakeDamage(damageToApply, _enemyClient);
    }

    public void UpdateCooldownMessage()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((ushort)player.ID);
            writer.Write(PrimarySkillCooldown);
            writer.Write(SecondarySkillCooldown);

            using (Message message = Message.Create(NetworkTags.UpdateCooldownTag, writer))
                player.ServerClient.SendMessage(message, SendMode.Reliable);
        }
    }

    public void UpdateSkillCooldowns(float _primary, float _secondary)
    {
        PrimarySkillCooldown = _primary;
        SecondarySkillCooldown = _secondary;
    }

    public void UpdateMultishotMessage(bool _value, ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID,
            Multishot = _value
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    public void UpdateFrostNovaMessage(bool _value, ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID,
            FrostNova = _value
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    public void ApplyDashLock(IClient _damageOrigin, ushort _PoisonID)
    {
        if (!IsDashLocked)
        {
            IsDashLocked = true;
            playerParticleManager.DashLock(true);
            StartCoroutine(DashLock(_damageOrigin, _PoisonID));
            SendDashLockParticleMessage(true, _PoisonID);
        }

    }

    void SendDashLockParticleMessage(bool _value, ushort _PoisonID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _PoisonID,
            DashLock = _value
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    IEnumerator DashLock(IClient _damageOrigin, ushort _PoisonID)
    {

        yield return new WaitForSeconds(PlayerBaseStats.Instance.DashLockDuration);

        IsDashLocked = false;
        SendDashLockParticleMessage(false, _PoisonID);
        playerParticleManager.DashLock(false);


    }


}
