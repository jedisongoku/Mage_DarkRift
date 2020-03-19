using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseStats : MonoBehaviour
{
    public static PlayerBaseStats Instance;

    [SerializeField] private float walkSpeed;
    [SerializeField] private int health;
    [SerializeField] private float healthGenerationRate;
    [SerializeField] private int primarySkillDamage;
    [SerializeField] private float primarySkillCooldown;
    [SerializeField] private int primarySkillCharge;
    [SerializeField] private float primarySkillRecharge;
    [SerializeField] private float secondarySkillCooldown;
    [SerializeField] private int bloodthirstHealAmount;
    [SerializeField] private int hpBoostAmount;
    [SerializeField] private float shieldGuardDamageReductionRate;
    [SerializeField] private float frostbiteDamageRate;
    [SerializeField] private int frostbiteDuration;
    [SerializeField] private float chillSlowRate;
    [SerializeField] private float chillDuration;
    [SerializeField] private float multishotDelayTime;
    [SerializeField] private float rageStartRate;
    [SerializeField] private float rageDamageRate;
    [SerializeField] private float poisonPickupTime;
    [SerializeField] private float damageBoostMultiplier;
    [SerializeField] private float attackSpeedMultiplier;
    [SerializeField] private float dashSpeedMultiplier;
    [SerializeField] private float strongHeartMultiplier;



    void Awake()
    {
        Instance = this;
    }
    public int Health
    {
        get
        {
            return health;
        }
    }

    public float DamageBoostMultiplier
    {
        get
        {
            return damageBoostMultiplier;
        }
    }

    public float AttackSpeedMultiplier
    {
        get
        {
            return attackSpeedMultiplier;
        }
    }

    public float DashSpeedMultiplier
    {
        get
        {
            return dashSpeedMultiplier;
        }
    }

    public float StrongHeartMultiplier
    {
        get
        {
            return strongHeartMultiplier;
        }
    }
    public int PrimarySkillDamage
    {
        get
        {
            return primarySkillDamage;
        }
    }

    public float PrimarySkillCooldown
    {
        get
        {
            return primarySkillCooldown;
        }
    }

    public int PrimarySkillCharge
    {
        get
        {
            return primarySkillCharge;
        }
    }

    public float PrimarySkillRecharge
    {
        get
        {
            return primarySkillRecharge;
        }
    }

    public float SecondarySkillCooldown
    {
        get
        {
            return secondarySkillCooldown;
        }
    }

    public float HealthGenerationRate
    {
        get
        {
            return healthGenerationRate;
        }
    }

    public int BloodthirstHealAmount
    {
        get
        {
            return bloodthirstHealAmount;
        }
    }

    public int HpBoostAmount
    {
        get
        {
            return hpBoostAmount;
        }
    }

    public float ShieldGuardDamageReductionRate
    {
        get
        {
            return shieldGuardDamageReductionRate;
        }
    }

    public float FrostbiteDamageRate
    {
        get
        {
            return frostbiteDamageRate;
        }
    }

    public int FrostbiteDuration
    {
        get
        {
            return frostbiteDuration;
        }
    }

    public float WalkSpeed
    {
        get
        {
            return walkSpeed;
        }
    }

    public float ChillSlowRate
    {
        get
        {
            return chillSlowRate;
        }
    }

    public float ChillDuration
    {
        get
        {
            return chillDuration;
        }
    }

    public float Multishot
    {
        get
        {
            return multishotDelayTime;
        }
    }

    public float RageDamageRate
    {
        get
        {
            return rageDamageRate;
        }
    }

    public float RageStartRate
    {
        get
        {
            return rageStartRate;
        }
    }

    public float PoisonPickupTime
    {
        get
        {
            return poisonPickupTime;
        }
    }
}
