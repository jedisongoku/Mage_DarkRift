using DarkRift;
using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    private Player player;
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private int playerMaxHealth;
    private float healthGenerationRate;
    private int bloodthirstHealAmount;
    private int hpBoostAmount;
    private float shieldGuardDamageReductionRate;
    private int playerhealth;
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
}
