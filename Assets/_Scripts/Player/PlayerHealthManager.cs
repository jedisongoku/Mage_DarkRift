using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHealthManager : MonoBehaviourPun
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    private float playerMaxHealth;
    
    private float healthGenerationRate;
    private float shieldGuardDamageReductionRate;

    private float playerhealth;
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
    [SerializeField] private GameObject respawnParticle;
    bool isFrostbite = false;
    bool isBloodthirst = false;
    bool isHpBoost = false;
    bool isStrongHeart = false;
    bool isShieldGuard = false;
    bool isRage = false;
    bool isRespawnShield = false;

    int frostbiteDurationTick = 0;

    bool CanHeal { get; set; }
    public bool isSecondChance { get; set; }


    private void OnDisable()
    {
        GameManager.OnPlayerKill -= BloodthirstHeal;
    }
    private void Awake()
    {
        SetPlayerBaseStats();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        if (photonView.IsMine)
        {
            StartCoroutine(HealthRegeneration());
            
        }
        
    }

    void SetPlayerBaseStats()
    {
        if(!isSecondChance)
        {
            playerMaxHealth = PlayFabDataStore.playerBaseStats.Health;
            healthGenerationRate = PlayFabDataStore.playerBaseStats.HealthGenerationRate;
            bloodthirstHealAmount = PlayFabDataStore.playerBaseStats.BloodthirstHealAmount;
            hpBoostAmount = PlayFabDataStore.playerBaseStats.HpBoostAmount;
            shieldGuardDamageReductionRate = PlayFabDataStore.playerBaseStats.ShieldGuardDamageReductionRate;
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
            CanHeal = true;
        }
        else
        {
            if(isStrongHeart) strongHeartParticle.SetActive(true);
            if(isShieldGuard) shieldGuardParticle.SetActive(true);
        }
        isSecondChance = false;
        StartCoroutine(RespawnShield());
        

    }

    IEnumerator RespawnShield()
    {
        isRespawnShield = true;
        respawnParticle.SetActive(true);

        yield return new WaitForSeconds(2);

        isRespawnShield = false;
        respawnParticle.SetActive(false);
    }

    IEnumerator HealthRegeneration()
    {
        yield return new WaitForSeconds(1f);
        if (playerhealth < playerMaxHealth)
        {
            if(CanHeal && !isFrostbite)
            {
                playerhealth += playerMaxHealth * healthGenerationRate;
                if (playerhealth > playerMaxHealth)
                {
                    playerhealth = playerMaxHealth;
                }

                photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, 0, false);
            }
            
        }
        StartCoroutine(HealthRegeneration());

    }

    

    public void StartFrostbite(int _damageOrigin)
    {
        if(!isFrostbite)
        {
            isFrostbite = true;
            photonView.RPC("StartFrostbite_RPC", RpcTarget.All, _damageOrigin);
        }
        
        
    }

    [PunRPC]
    void StartFrostbite_RPC(int _damageOrigin)
    {
        frostbiteDurationTick = 0;
        frostbiteParticle.SetActive(true);
        StartCoroutine(Frostbite(_damageOrigin));
    }

    IEnumerator Frostbite(int _damageOrigin)
    {
        
        yield return new WaitForSeconds(1f);
        if(frostbiteDurationTick < PlayFabDataStore.playerBaseStats.FrostbiteDuration)
        {
            if(photonView.IsMine)
            {
                TakeDamage((PlayFabDataStore.playerBaseStats.PrimarySkillDamage * PlayFabDataStore.playerBaseStats.FrostbiteDamageRate) / PlayFabDataStore.playerBaseStats.FrostbiteDuration);
                //playerhealth -= (PlayFabDataStore.playerBaseStats.PrimarySkillDamage * PlayFabDataStore.playerBaseStats.FrostbiteDamageRate) / PlayFabDataStore.playerBaseStats.FrostbiteDuration;
                //photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, damageOrigin, true);
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
            photonView.RPC("Bloodthirst_UpdateHealth", RpcTarget.All, playerhealth);
        }
    }

    [PunRPC]
    void Bloodthirst_UpdateHealth(float _health)
    {
        playerhealth = _health;
        healingParticle.SetActive(false);
        healingParticle.SetActive(true);

        UpdateHealthBar();
    }

    public bool CanTakeDamage()
    {
        return photonView.IsMine;
    }

    public void OnPlayerHit(int _playerViewId, float _damageDone, bool _isFrostbite, bool _isChill, bool _isFrostNova, bool _isRage)
    {
        DamageOrigin = _playerViewId;
        if (_isRage) _damageDone = _damageDone * PlayFabDataStore.playerBaseStats.RageDamageRate;  
        if (_isFrostbite) StartFrostbite(DamageOrigin);
        if (_isChill) GetComponent<PlayerMovementController>().StartChill(PlayFabDataStore.playerBaseStats.ChillDuration);

        TakeDamage(_damageDone);

    }

    public void TakeDamage(float _damage)
    {
        if(isRespawnShield)
        {
            _damage = 0;
        }

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
        CancelInvoke("DelayedHealthRegenrationStart");
        CanHeal = false;
        Invoke("DelayedHealthRegenrationStart", 3f);

        if (damageOrigin == GameManager.Instance.GetCurrentPlayerViewID && !GetComponent<PlayerCombatManager>().isPlayer) ShowFloatingCombatText(_damage);
        bool frostbite = _damage < 5 ? true : false;
        photonView.RPC("UpdateHealth", RpcTarget.All, playerhealth, damageOrigin, frostbite);
    }

    void DelayedHealthRegenrationStart()
    {
        CanHeal = true;
    }

    void EnableExplosionParticle()
    {
        GameObject obj;

        obj = ObjectPooler.Instance.GetPrimarySkillExplosionPrefab();
        obj.transform.position = transform.position + Vector3.up;
        obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.zero);

        obj.SetActive(true);

    }

    public void InformNewUser(Player _newPlayer)
    {
        photonView.RPC("InformNewUser_RPC", _newPlayer, playerhealth);
    }
    [PunRPC]
    void InformNewUser_RPC(float _health)
    {
        playerhealth = _health;
        OnPlayerDeath();
        UpdateHealthBar();

    }

    void UpdateHealthBar()
    {
        healthBar.GetComponent<Image>().fillAmount = playerhealth / playerMaxHealth;
        healthText.text = Mathf.CeilToInt(playerhealth).ToString();
    }

    [PunRPC]
    void UpdateHealth(float _health, int _damageOrigin, bool _isFrostbite)
    {
        healthGenerationTimer = 0;

        if (_damageOrigin != 0 && !_isFrostbite)
        {
            EnableExplosionParticle();
        }
        float damage = playerhealth - _health;
        playerhealth = _health;
        if(playerhealth <= 0 && !GetComponent<PlayerCombatManager>().IsDead)
        {
            GameManager.Instance.KillFeed(_damageOrigin, this.gameObject);
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

        UpdateHealthBar();

        if (_damageOrigin == GameManager.Instance.GetCurrentPlayerViewID && _isFrostbite)
        {
            //Debug.Log("Forstbite Text " + damage);
            //ShowFloatingCombatText(damage);
        }

        if((playerhealth / playerMaxHealth <= PlayFabDataStore.playerBaseStats.RageStartRate) && isRage && !GetComponent<PlayerCombatManager>().IsDead)
        {
            if(!GetComponent<PlayerCombatManager>().isInvisible)
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
        if(photonView.IsMine) PhotonNetwork.CleanRpcBufferIfMine(photonView);



        GetComponent<PlayerCombatManager>().IsDead = true;
        GetComponent<PlayerMovementController>().OnPlayerDeath();

        isFrostbite = false;
        strongHeartParticle.SetActive(false);
        shieldGuardParticle.SetActive(false);
        frostbiteParticle.SetActive(false);
        rageParticle.SetActive(false);
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
            StartCoroutine(HealthRegeneration());
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
            return ((playerhealth / playerMaxHealth <= PlayFabDataStore.playerBaseStats.RageStartRate) && isRage);
        }
        set
        {
            isRage = true;
            photonView.RPC("SetRage", RpcTarget.AllBuffered, isRage);
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
                photonView.RPC("HpBoost_RPC", RpcTarget.AllBuffered, playerhealth, playerMaxHealth);
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
            if (isStrongHeart) photonView.RPC("StrongHeart_RPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void SetShieldGuard()
    {
        isShieldGuard = true;
        shieldGuardParticle.SetActive(true);
    }

    [PunRPC]
    void StrongHeart_RPC()
    {
        isStrongHeart = true;
        strongHeartParticle.SetActive(true);
        healthGenerationRate *= PlayFabDataStore.playerBaseStats.StrongHeartMultiplier;
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
            if (isStrongHeart) photonView.RPC("StrongHeart_RPC", RpcTarget.AllBuffered);
            Debug.Log(value);
        }
    }

    public bool ShieldGuard
    {
        set
        {
            isShieldGuard = value;
            if(isShieldGuard) photonView.RPC("SetShieldGuard", RpcTarget.AllBuffered);
        }
    }

    void ShowFloatingCombatText(float amount)
    {
        GameObject obj = ObjectPooler.Instance.GetFloatingCombatTextPrefab();
        //Debug.Log("Damage amount " + amount);
        if(amount == 0)
        {
            obj.GetComponent<TextMeshPro>().text = "BLOCK";
        }
        else
        {
            obj.GetComponent<TextMeshPro>().text = Mathf.RoundToInt(amount).ToString();
        }
        
        obj.transform.position = transform.position;
        obj.SetActive(true);
        
    }

    public void SwitchShieldVisibility(bool value)
    {
        if (isShieldGuard) shieldGuardParticle.SetActive(value);
    }

    public void SwitchRageVisibility(bool value)
    {
        if (Rage) rageParticle.SetActive(value);
    }

    public void SwitchStrongHearthVisibility(bool value)
    {

        if (isStrongHeart) strongHeartParticle.SetActive(value);
    }

    public void SwitchFrostbiteVisibility(bool value)
    {
        if (isFrostbite) frostbiteParticle.SetActive(value);
    }

    public float PlayerHealth
    {
        get
        {
            return playerhealth;
        }
    }
}
