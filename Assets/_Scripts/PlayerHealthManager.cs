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
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private int playerhealth;
    private int playerMaxHealth;
    private float healthGenerationRate;
    private int bloodthirstHealAmount;
    private int hpBoostAmount;
    private float shieldGuardDamageReductionRate;
    
    private bool isBloodthirst;
    private bool isHpBoost;
    private bool isStrongHeart;
    private bool isShieldGuard;
    private bool isRage;

    void Start()
    {
        player = GetComponent<Player>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        //StartCoroutine(HealhtRegeneration());
        SetPlayerBaseStats();
        //UpdateHealth();
        StartCoroutine(HealhtRegeneration());
    }

    void SetPlayerBaseStats()
    {
        playerMaxHealth = PlayerBaseStats.Instance.Health;
        healthGenerationRate = PlayerBaseStats.Instance.HealthGenerationRate;
        bloodthirstHealAmount = PlayerBaseStats.Instance.BloodthirstHealAmount;
        hpBoostAmount = PlayerBaseStats.Instance.HpBoostAmount;
        shieldGuardDamageReductionRate = PlayerBaseStats.Instance.ShieldGuardDamageReductionRate;
        playerhealth = playerMaxHealth;
        isBloodthirst = false;
        isHpBoost = false;
        isStrongHeart = false;
        isShieldGuard = false;
        isRage = false;
        //strongHeartParticle.SetActive(false);
        //shieldGuardParticle.SetActive(false);
        //frostbiteParticle.SetActive(false);

        Invoke("UpdateHealth", 0.25f);
    }

    void UpdateHealth()
    {
        Debug.Log("health " + playerhealth);
        if(playerhealth <= 0)
        {
            Invoke("Dead", 0.1f);
            m_Animator.SetTrigger("Dead");
            //dead here
        }
        else
        {
            //Debug.Log(playerhealth + "/" + playerMaxHealth + " - " + (float)playerhealth / (float)playerMaxHealth);
            playerHealthBar.fillAmount = (float)playerhealth / (float)playerMaxHealth;
            playerHealthText.text = playerhealth.ToString();
        }
        
        if(player.IsServer)
        {
            SendHealthMessage();
        }
        
    }

    #region Server Only Calls
    public void TakeDamage(int _damageTaken, IClient _damageOrigin)
    {
        //Add other rune variables here before applying the damage to self

        playerhealth = (playerhealth - _damageTaken) <= 0 ? 0 : (playerhealth - _damageTaken);
        if(playerhealth <= 0)
        {
            ScoreManager.Instance.UpdateScoreboard(_damageOrigin.ID, player.ID);            
        }
        UpdateHealth();
        //SendHealthMessage();
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
        Debug.Log("Message Received in manager");
        playerhealth = _health;
        UpdateHealth();
    }
    
    void Dead()
    {
        Debug.Log("Dead");
        GetComponent<CapsuleCollider>().enabled = false;
        playerUI.SetActive(false);
        player.IsDead = true;
        if(player.IsControllable)
        {
            HUDManager.Instance.OnPlayerDeath();
        }
        
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
            playerhealth += Mathf.CeilToInt(playerMaxHealth * healthGenerationRate);

            if (playerhealth > playerMaxHealth)
            {
                playerhealth = playerMaxHealth;
            }
            UpdateHealth();

        }
        StartCoroutine(HealhtRegeneration());

    }
}
