﻿using DarkRift;
using DarkRift.Server;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    }

    void UpdateHealth()
    {
        Debug.Log("health " + playerhealth);
        if(playerhealth <= 0)
        {
            Dead();
            m_Animator.SetTrigger("Dead");
            //dead here
        }
        else
        {
            Debug.Log(playerhealth + "/" + playerMaxHealth + " - " + (float)playerhealth / (float)playerMaxHealth);
            playerHealthBar.fillAmount = (float)playerhealth / (float)playerMaxHealth;
            playerHealthText.text = playerhealth.ToString();
        }
        /*
        if(player.IsServer)
        {
            SendHealthMessage();
        }*/
        
    }

    #region Server Only Calls
    public void TakeDamage(int _damageTaken, IClient _damageOrigin)
    {
        //Add other rune variables here before applying the damage to self

        playerhealth = (playerhealth - _damageTaken) <= 0 ? 0 : (playerhealth - _damageTaken);
        if(playerhealth <= 0)
        {
            //register the killer in the scoreboard
        }
        UpdateHealth();
        SendHealthMessage();
    }

    void SendHealthMessage()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            HealthMessageModel newMessage = new HealthMessageModel()
            {
                NetworkID = (ushort)player.ID,
                Health = playerhealth
            };
            //writer.Write(player.ID);
            //writer.Write(playerhealth);
            //probably add the rune applications for particles

            using (Message message = Message.Create(NetworkTags.HealthPlayerTag, newMessage))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Reliable);
        }
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
    }

    void Respawn()
    {
        GetComponent<CapsuleCollider>().enabled = true;
        playerUI.SetActive(true);
        player.IsDead = false;
    }
}
