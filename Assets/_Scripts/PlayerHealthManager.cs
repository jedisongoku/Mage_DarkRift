﻿using System.Collections;
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
    private GameObject playerUI;

    [Header("Runes")]
    [SerializeField] private GameObject frostbiteParticle;
    [SerializeField] private GameObject healingParticle;
    [SerializeField] private GameObject strongHeartParticle;
    [SerializeField] private GameObject shieldGuardParticle;
    [SerializeField] private GameObject rageParticle;
    bool isFrostbite = false;
    bool isBloodthirst = false;
    bool isHpBoost = false;
    bool isStrongHeart = false;
    bool isShieldGuard = false;
    bool isRage = false;

    int frostbiteDurationTick = 0;


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
        }/*
        foreach(var item in HUDManager.Instance.playerUIList)
        {
            if (!item.activeInHierarchy)
            {
                playerUI = item;
                item.SetActive(true);
                StartCoroutine(PlayerUIFollow());
                break;
            }
            
        }*/
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
        isFrostbite = false;
        isBloodthirst = false;
        isHpBoost = false;
        isStrongHeart = false;
        isShieldGuard = false;
        isRage = false;
        strongHeartParticle.SetActive(false);
        shieldGuardParticle.SetActive(false);
        frostbiteParticle.SetActive(false);

    }

    IEnumerator PlayerUIFollow()
    {
        //playerUI.transform.position = Camera.main.WorldToScreenPoint(GameManager.Instance.GetCurrentPlayer.transform.position);
        var targetPosition = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 130, 0);
        //playerUI.transform.position = targetPosition;
        playerUI.transform.position = Vector2.Lerp(playerUI.transform.position, targetPosition, Time.deltaTime * 15);
        //playerUI.transform.position += new Vector3(0, 10, 0);

        yield return new WaitForSeconds(0);


        StartCoroutine(PlayerUIFollow());
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

            photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, 0, false);
        }
        StartCoroutine(HealhtRegeneration());

    }

    

    public void StartFrostbite(int _damageOrigin)
    {
        if(!isFrostbite)
        {
            Debug.Log("Starting Frostbite...");
            isFrostbite = true;
            photonView.RPC("StartFrostbite_RPC", RpcTarget.All, _damageOrigin);
        }
        
        
    }

    [PunRPC]
    void StartFrostbite_RPC(int _damageOrigin)
    {
        Debug.Log("Frostbite Everywhere");
        frostbiteDurationTick = 0;
        frostbiteParticle.SetActive(true);
        StartCoroutine(Frostbite(_damageOrigin));
    }

    IEnumerator Frostbite(int _damageOrigin)
    {
        
        yield return new WaitForSeconds(1f);
        if(frostbiteDurationTick < PlayerBaseStats.Instance.FrostbiteDuration)
        {
            if(photonView.IsMine)
            {
                playerhealth -= (playerMaxHealth * PlayerBaseStats.Instance.FrostbiteDamageRate) / PlayerBaseStats.Instance.FrostbiteDuration;
                photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, damageOrigin, false);
            }
            frostbiteDurationTick++;
            
            if (playerhealth > 0)
            {
                StartCoroutine(Frostbite(_damageOrigin));
            }
        }
        else
        {
            frostbiteDurationTick = 0;
            isFrostbite = false;
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
            photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, 0, false);
        }
    }

    public bool CanTakeDamage()
    {
        return photonView.IsMine;
    }

    public void OnPlayerHit(int _playerViewId, float _damageDone, bool _isFrostbite, bool _isChill, bool _isFrostNova, bool _isRage)
    {
        DamageOrigin = _playerViewId;
        Debug.Log("Frosbite: " + _isFrostbite + "  Chill: " + _isChill);
        if (_isRage) _damageDone += _damageDone * PlayerBaseStats.Instance.RageDamageRate;  
        if (_isFrostbite) StartFrostbite(DamageOrigin);
        if (_isChill) GetComponent<PlayerMovementController>().StartChill(PlayerBaseStats.Instance.ChillDuration);

        TakeDamage(_damageDone, _isFrostNova);

    }

    public void TakeDamage(float _damage, bool _isFrostNova)
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

        photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, damageOrigin, _isFrostNova);
    }

    void EnableExplosionParticle(bool _isFrostNova)
    {
        GameObject obj;
        if (_isFrostNova)
        {
            obj = ObjectPooler.Instance.GetPrimarySkillFrostNovaPrefab();
            obj.transform.position = transform.position + Vector3.up;
        }
        else
        {
            
            obj = ObjectPooler.Instance.GetPrimarySkillExplosionPrefab();
            obj.transform.position = transform.position + Vector3.up;
            obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.zero);
        }

        obj.SetActive(true);

    }


    [PunRPC]
    void UpdateHealth(float _health, int _damageOrigin, bool _FrostNova)
    {
        healthGenerationTimer = 0;

        if(_damageOrigin != 0 && Mathf.Abs(playerhealth - _health) > 5)
        {
            EnableExplosionParticle(_FrostNova);
        }

        playerhealth = _health;
        if(playerhealth <= 0 && !GetComponent<PlayerCombatManager>().IsDead)
        {
            GameManager.Instance.KillFeed(_damageOrigin);
            OnPlayerDeath();

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
        healthText.text = Mathf.CeilToInt(playerhealth).ToString();
        if((playerhealth / playerMaxHealth <= PlayerBaseStats.Instance.RageStartRate) && isRage)
        {
            rageParticle.SetActive(true);
        }
        else
        {
            rageParticle.SetActive(false);
        }
    }

    void OnPlayerDeath()
    {
        StopAllCoroutines();
        
        GetComponent<PlayerCombatManager>().IsDead = true;
        GetComponent<PlayerMovementController>().OnPlayerDeath();

        isFrostbite = false;
        strongHeartParticle.SetActive(false);
        shieldGuardParticle.SetActive(false);
        frostbiteParticle.SetActive(false);
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
            photonView.RPC("UpdateHealth", RpcTarget.All, playerMaxHealth, 0, false);
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
                photonView.RPC("HpBoost_RPC", RpcTarget.OthersBuffered, playerhealth, playerMaxHealth);
            }
            Debug.Log(value);
        }
    }

    [PunRPC]
    void HpBoost_RPC(float _playerHealth, float _maxHealth)
    {
        healingParticle.SetActive(false);
        healingParticle.SetActive(true);
        playerhealth = _playerHealth;
        playerMaxHealth = _maxHealth;
        healthBar.GetComponent<Image>().fillAmount = playerhealth / playerMaxHealth;
        healthText.text = Mathf.CeilToInt(playerhealth).ToString();
    }
    public bool StrongHeart
    {
        set
        {
            isStrongHeart = value;
            if (isStrongHeart) photonView.RPC("StrongHeart_RPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void SetShieldGuard()
    {
        shieldGuardParticle.SetActive(true);
    }

    [PunRPC]
    void StrongHeart_RPC()
    {
        strongHeartParticle.SetActive(true);
        healthGenerationRate *= PlayerBaseStats.Instance.StrongHeartMultiplier;
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
            if (isStrongHeart) photonView.RPC("StrongHeart_RPC", RpcTarget.All);
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
