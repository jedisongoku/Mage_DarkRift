using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHealthManager : MonoBehaviourPun
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    private float playerMaxHealth;
    
    private float healthGenerationRate;
    private float shieldGuardDamageReductionRate;

    private float playerhealth;
    private float damageTaken;
    private int damageOrigin;
    private float healthGenerationTimer;
    private int bloodthirstHealAmount;
    private int hpBoostAmount;
    private GameObject playerModel;

    [Header("Runes")]
    [SerializeField] private GameObject frostbiteParticle;
    [SerializeField] private GameObject healingParticle;
    [SerializeField] private GameObject strongHeartParticle;
    [SerializeField] private GameObject shieldGuardParticle;
    [SerializeField] private GameObject rageParticle;
    bool isBloodthirst = false;
    bool isHpBoost = false;
    bool isStrongHeart = false;
    bool isShieldGuard = false;
    bool isRage = false;

    int flameDurationTick = 0;


    private void OnDisable()
    {
        GameManager.OnPlayerKill -= BloodthirstHeal;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        if (photonView.IsMine)
        {
            StartCoroutine(HealhtRegeneration());
        }
        SetPlayerBaseStats();
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
        strongHeartParticle.SetActive(false);
        shieldGuardParticle.SetActive(false);
        frostbiteParticle.SetActive(false);




    }

    IEnumerator HealhtRegeneration()
    {
        yield return new WaitForSeconds(1f);
        if (playerhealth < playerMaxHealth)
        {
            playerhealth += playerMaxHealth * healthGenerationRate;
            if (playerhealth > playerMaxHealth)
            {
                playerhealth = playerMaxHealth;
            }

            photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, 0);
        }
        StartCoroutine(HealhtRegeneration());

    }

    [PunRPC]
    void SetShieldGuard()
    {
        shieldGuardParticle.SetActive(true);
    }

    [PunRPC]
    void SetStrongHeart()
    {
        strongHeartParticle.SetActive(true);
    }

    public void StartFrostbite(int _damageOrigin)
    {
        Debug.Log("Starting Flame...");
        photonView.RPC("StartFrostbite_RPC", RpcTarget.All, _damageOrigin);
        
    }

    [PunRPC]
    void StartFrostbite_RPC(int _damageOrigin)
    {
        flameDurationTick = 0;
        frostbiteParticle.SetActive(true);
        StartCoroutine(Frostbite(_damageOrigin));
    }

    IEnumerator Frostbite(int _damageOrigin)
    {
        
        yield return new WaitForSeconds(1f);
        if(flameDurationTick < PlayerBaseStats.Instance.FlameDuration)
        {
            if(photonView.IsMine)
            {
                playerhealth -= (playerMaxHealth * PlayerBaseStats.Instance.FlameDamageRate) / PlayerBaseStats.Instance.FlameDuration;
                photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, damageOrigin);
            }
            flameDurationTick++;
            
            if (playerhealth > 0)
            {
                StartCoroutine(Frostbite(_damageOrigin));
            }
        }
        else
        {
            flameDurationTick = 0;
            frostbiteParticle.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)  
    {
        if(photonView.IsMine && other.gameObject.layer == LayerMask.NameToLayer("Damageable"))
        {
            Debug.Log("Collider Entered " + other.gameObject.layer);
            if (other.gameObject.GetComponent<PrimarySkillController>().PlayerOrigin != photonView.ViewID)
            {
                //Debug.Log("Call RPC TakeDamage ");
                //TakeDamage(damageTaken);
            }
        }
    
    }

    void BloodthirstHeal()
    {
        if(isBloodthirst)
        {
            if(playerhealth + bloodthirstHealAmount > playerMaxHealth)
            {
                playerhealth = playerMaxHealth;
            }
            else
            {
                playerhealth += bloodthirstHealAmount;
            }
            photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, 0);
        }
    }


    public void TakeDamage(float _damage)
    {
        if(photonView.IsMine)
        {
            if (isShieldGuard)
            {
                _damage -= _damage * shieldGuardDamageReductionRate;
            }
            if (playerhealth - _damage <= 0)
            {
                playerhealth = 0;
            }
            else
            {
                playerhealth -= _damage;
            }
            damageTaken = 0;

            photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, damageOrigin);
        }
    }


    [PunRPC]
    void UpdateHealth(float _health, int _damageOrigin)
    {
        healthGenerationTimer = 0;

        playerhealth = _health;
        if(playerhealth <= 0)
        {
            StopAllCoroutines();
            GameManager.Instance.KillFeed(_damageOrigin);
            GetComponent<PlayerCombatManager>().IsDead = true;
            frostbiteParticle.SetActive(false);
            if (photonView.IsMine)
            {
                healthBar.transform.parent.gameObject.SetActive(false);
            }
            
        }
        else if(playerhealth == playerMaxHealth)
        {
            if (photonView.IsMine)
            {
                healthBar.transform.parent.gameObject.SetActive(true);
            }
        }
        healthBar.GetComponent<Image>().fillAmount = playerhealth / playerMaxHealth;
        healthText.text = Mathf.CeilToInt(playerhealth) + "/" + playerMaxHealth;
        if((playerhealth / playerMaxHealth <= PlayerBaseStats.Instance.RageStartRate) && isRage)
        {
            rageParticle.SetActive(true);
        }
        else
        {
            rageParticle.SetActive(false);
        }
    }

    [PunRPC]
    void SetMaxHealth(float _playerHealth, float _maxHealth)
    {
        healingParticle.SetActive(false);
        healingParticle.SetActive(true);
        playerhealth = _playerHealth;
        playerMaxHealth = _maxHealth;
        healthBar.GetComponent<Image>().fillAmount = playerhealth / playerMaxHealth;
        healthText.text = Mathf.CeilToInt(playerhealth) + "/" + playerMaxHealth;
    }

    [PunRPC]
    void SetRage(bool _isRage)
    {
        isRage = _isRage;
    }

    public void RespawnPlayer()
    {
        SetPlayerBaseStats();
        if (photonView.IsMine)
        {
            photonView.RPC("UpdateHealth", RpcTarget.All, playerMaxHealth, 0);
            StartCoroutine(HealhtRegeneration());
        }
    }

    public GameObject PlayerModel
    {
        set
        {
            playerModel = value;
        }
    }

    public bool Rage
    {
        get
        {
            return ((playerhealth / playerMaxHealth <= PlayerBaseStats.Instance.RageStartRate) && isRage);
        }
        set
        {
            isRage = true;
            photonView.RPC("SetRage", RpcTarget.AllBuffered, isRage);
        }

    }
    public float DamageTaken
    {
        get
        {
            return damageTaken;
        }
        set
        {
            damageTaken = value;
        }
    }

    public int DamageOrigin
    {
        get
        {
            return damageOrigin;
        }
        set
        {
            damageOrigin = value;
        }
    }

    public bool Bloodthirst
    {
        get
        {
            return isBloodthirst;
        }
        set
        {
            isBloodthirst = value;
            if(isBloodthirst)
            {
                GameManager.OnPlayerKill += BloodthirstHeal;
                Debug.Log(value);
            }
            
        }
    }

    public bool HpBoost
    {
        get
        {
            return isHpBoost;
        }
        set
        {
            isHpBoost = value;
            if(isHpBoost)
            {
                playerMaxHealth += hpBoostAmount;
                if(playerhealth + hpBoostAmount > playerMaxHealth)
                {
                    playerhealth = playerMaxHealth;
                }
                else
                {
                    playerhealth += hpBoostAmount;
                }
                photonView.RPC("SetMaxHealth", RpcTarget.All, playerhealth, playerMaxHealth);
            }
            Debug.Log(value);
        }
    }
    public bool StrongHeart
    {
        set
        {
            isStrongHeart = value;
        }
    }
    public float HealthGenerationRate
    {
        get
        {
            return healthGenerationRate;
        }
        set
        {
            healthGenerationRate = value;
            if (isStrongHeart) photonView.RPC("SetStrongHeart", RpcTarget.All);
            Debug.Log(value);
        }
    }

    public bool ShieldGuard
    {
        set
        {
            isShieldGuard = value;
            if(isShieldGuard) photonView.RPC("SetShieldGuard", RpcTarget.All);
        }
    }
}
