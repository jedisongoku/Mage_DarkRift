using DarkRift;
using DarkRift.Server;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] GameObject playerUI;
    [SerializeField] Image playerHealthBar;
    [SerializeField] TextMeshProUGUI playerHealthText;

    private Player player;
    private PlayerCombatManager playerCombatManager;
    private PlayerParticleManager playerParticleManager;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private int playerhealth;
    private int playerMaxHealth;
    public float HealthGenerationRate { get; set; }
    private float frostbiteDurationTick;
    private float poisonDurationTick;

    [Header("Runes")]
    private bool isBloodthirstActive;
    private bool isShieldGuardActive;
    public bool Rage { get; set; }
    private bool HasPlayerRaged { get; set; }
    private bool IsFrostbited { get; set; }
    private bool IsPoisoned { get; set; }


    void Start()
    {
        player = GetComponent<Player>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playerParticleManager = GetComponent<PlayerParticleManager>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();


        //StartCoroutine(HealhtRegeneration());
        SetPlayerBaseStats();
        //UpdateHealth();
        if(player.IsServer)
        {
            StartCoroutine(HealhtRegeneration());
        }
        
    }


    void SetPlayerBaseStats()
    {
        playerMaxHealth = PlayerBaseStats.Instance.Health;
        HealthGenerationRate = PlayerBaseStats.Instance.HealthGenerationRate;
        playerhealth = playerMaxHealth;
        Bloodthirst = false;
        ShieldGuard = false;
        Rage = false;
        IsFrostbited = false;
        IsPoisoned = false;

        Invoke("UpdateHealth", 0.25f);
    }

    void UpdateHealth()
    {
        //Debug.Log("health " + playerhealth);
        if(playerhealth <= 0 && !player.IsDead)
        {
            player.IsDead = true;
            m_Animator.SetTrigger("Dead");
            Invoke("Dead", 0.1f);
        }
        else
        {
            playerHealthBar.fillAmount = (float)playerhealth / (float)playerMaxHealth;
            playerHealthText.text = playerhealth.ToString();
        }

        if (((float)playerhealth / (float)playerMaxHealth <= PlayerBaseStats.Instance.RageStartRate) && Rage && !HasPlayerRaged)
        {
            HasPlayerRaged = true;
            playerCombatManager.Rage = true;
            SendRageParticleMessage(true, PlayerRuneManager.Rage_ID);
            
        }
        else if (((float)playerhealth / (float)playerMaxHealth >= PlayerBaseStats.Instance.RageStartRate) && Rage && HasPlayerRaged)
        {
            HasPlayerRaged = false;
            playerCombatManager.Rage = false;
            SendRageParticleMessage(false, PlayerRuneManager.Rage_ID);
        }

        if (player.IsServer)
        {
            SendHealthMessage();
        }
        
    }

    public void ApplyFrostbite(IClient _damageOrigin, ushort _runeID)
    {
        if(!IsFrostbited)
        {
            IsFrostbited = true;
            playerParticleManager.Frostbite(true);
            StartCoroutine(Frostbite(_damageOrigin, _runeID));
            SendFrostbiteParticleMessage(true, _runeID);
        }
        
    }

    void SendFrostbiteParticleMessage(bool _value, ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID,
            Frostbite = _value
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    IEnumerator Frostbite(IClient _damageOrigin, ushort _runeID)
    {

        yield return new WaitForSeconds(1f);
        if (frostbiteDurationTick < PlayerBaseStats.Instance.FrostbiteDuration)
        {
            int damage = Mathf.RoundToInt((playerMaxHealth * PlayerBaseStats.Instance.FrostbiteDamageRate) / PlayerBaseStats.Instance.FrostbiteDuration);
            TakeDamage(damage, _damageOrigin);
            frostbiteDurationTick++;

            if (playerhealth > 0)
            {
                StartCoroutine(Frostbite(_damageOrigin, _runeID));
            }
        }
        else
        {
            IsFrostbited = false;
            frostbiteDurationTick = 0;
            SendFrostbiteParticleMessage(false, _runeID);
            playerParticleManager.Frostbite(false);
        }
    }

    #region Server Only Calls
    public void TakeDamage(int _damageTaken, IClient _damageOrigin)
    {
        //Add other rune variables here before applying the damage to self
        if (isShieldGuardActive)
        {
            _damageTaken -= Mathf.RoundToInt(_damageTaken * PlayerBaseStats.Instance.ShieldGuardDamageReductionRate);
        }

        playerhealth = (playerhealth - _damageTaken) <= 0 ? 0 : (playerhealth - _damageTaken);
        if(playerhealth <= 0)
        {
            ScoreManager.Instance.UpdateScoreboard(_damageOrigin.ID, player.ID);            
        }
        UpdateHealth();
    }

    void SendHealthMessage()
    {
        HealthMessageModel newMessage = new HealthMessageModel()
        {
            NetworkID = (ushort)player.ID,
            Health = playerhealth
        };

        using (Message message = Message.Create(NetworkTags.HealthPlayerTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    #endregion

    public void HealthMessageReceived(int _health)
    {
        //Debug.Log("Message Received in manager");
        playerhealth = _health;
        UpdateHealth();
    }
    
    void Dead()
    {
        Debug.Log("Dead");
        playerParticleManager.DisableParticles();
        GetComponent<CapsuleCollider>().enabled = false;
        playerUI.SetActive(false);

        if(player.IsControllable) HUDManager.Instance.OnPlayerDeath();

        if(player.IsControllable || player.IsServer) player.StartRespawnTimer();

    }

    public void RespawnPlayer()
    {
        SetPlayerBaseStats();
        GetComponent<CapsuleCollider>().enabled = true;
        playerUI.SetActive(true);
    }

    //Health Regen happens both in client and server. Server overrides the health on take damage.
    IEnumerator HealhtRegeneration()
    {
        yield return new WaitForSeconds(1f);
        if (playerhealth < playerMaxHealth)
        {
            playerhealth += Mathf.CeilToInt(playerMaxHealth * HealthGenerationRate);

            if (playerhealth > playerMaxHealth)
            {
                playerhealth = playerMaxHealth;
            }
            UpdateHealth();

        }
        StartCoroutine(HealhtRegeneration());

    }
    public bool ShieldGuard
    {
        get
        {
            return isShieldGuardActive;
        }
        set
        {
            isShieldGuardActive = value;
            if (isShieldGuardActive) SendShieldGuardParticleMessage(PlayerRuneManager.ShieldGuard_ID);
        }
    }

    public void StrongHeart()
    {
        HealthGenerationRate += PlayerBaseStats.Instance.StrongHeartRate;
        SendStrongHeartParticleMessage(PlayerRuneManager.StrongHeart_ID);
    }

    public bool Bloodthirst
    {
        get
        {
            return isBloodthirstActive;
        }
        set
        {
            isBloodthirstActive = value;

        }
    }

    public void BloodthirstHeal()
    {
        if (isBloodthirstActive)
        {
            if (playerhealth + PlayerBaseStats.Instance.BloodthirstHealAmount > playerMaxHealth)
            {
                playerhealth = playerMaxHealth;
            }
            else
            {
                playerhealth += PlayerBaseStats.Instance.BloodthirstHealAmount;
            }
            SendBloodthirstParticleMessage(PlayerRuneManager.Bloodthirst_ID);
        }
    }

    void SendBloodthirstParticleMessage(ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    public void HpBoost()
    {
        playerMaxHealth += PlayerBaseStats.Instance.HpBoostAmount;
        if (playerhealth + PlayerBaseStats.Instance.HpBoostAmount > playerMaxHealth)
        {
            playerhealth = playerMaxHealth;
        }
        else
        {
            playerhealth += PlayerBaseStats.Instance.HpBoostAmount;
        }
        
        SendIncreaseMaxHPMessage();
        SendHpBoostParticleMessage(PlayerRuneManager.HpBoost_ID);
        UpdateHealth();

    }

    public void SendIncreaseMaxHPMessage()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((ushort)player.ID);
            writer.Write((ushort)playerMaxHealth);

            using (Message message = Message.Create(NetworkTags.IncreaseHealthTag, writer))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Reliable);
        }
    }

    public void IncreaseMaxHP(ushort _playerMaxHeath)
    {
        Debug.Log("MAX HEALTH INCOMING " + _playerMaxHeath);
        playerMaxHealth = _playerMaxHeath;
        Debug.Log("MAX HEALTH SET " + playerMaxHealth);
    }

    void SendHpBoostParticleMessage(ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    void SendRageParticleMessage(bool _value, ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            Rage = _value,
            ParticleID = _runeID
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    void SendShieldGuardParticleMessage(ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    void SendStrongHeartParticleMessage(ushort _runeID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _runeID
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    #region Poison
    public void ApplyPoison(IClient _damageOrigin, ushort _particleID)
    {
        if (!IsPoisoned)
        {
            poisonDurationTick = 0;
            IsPoisoned = true;
            playerParticleManager.Poison(true);
            StartCoroutine(Poison(_damageOrigin, _particleID));
            SendPoisonParticleMessage(true, _particleID);
        }

    }

    void SendPoisonParticleMessage(bool _value, ushort _particleID)
    {
        ParticleEffectMessageModel newMessage = new ParticleEffectMessageModel()
        {
            NetworkID = (ushort)player.ID,
            ParticleID = _particleID,
            Poison = _value
        };

        using (Message message = Message.Create(NetworkTags.ParticleEffectTag, newMessage))
            foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                c.SendMessage(message, SendMode.Reliable);
    }

    IEnumerator Poison(IClient _damageOrigin, ushort _particleID)
    {

        yield return new WaitForSeconds(1f);
        if (poisonDurationTick < PlayerBaseStats.Instance.PoisonDuration)
        {
            //Debug.Log("Poison Tick " + poisonDurationTick);
            int damage = Mathf.RoundToInt((playerMaxHealth * PlayerBaseStats.Instance.PoisonDamageRate) / PlayerBaseStats.Instance.PoisonDuration);
            TakeDamage(damage, _damageOrigin);
            poisonDurationTick++;

            if (playerhealth > 0)
            {
                StartCoroutine(Poison(_damageOrigin, _particleID));
            }
        }
        else
        {
            IsPoisoned = false;
            poisonDurationTick = 0;
            SendPoisonParticleMessage(false, _particleID);
            playerParticleManager.Poison(false);
        }
    }

    #endregion
}
