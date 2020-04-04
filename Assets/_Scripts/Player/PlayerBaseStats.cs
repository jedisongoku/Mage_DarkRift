using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseStats
{

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
    [SerializeField] private float fasterRechargeMultiplier;
    [SerializeField] private float dashSpeedMultiplier;
    [SerializeField] private float strongHeartMultiplier;
    [SerializeField] private float smartMultiplier;

    public PlayerBaseStats(float _walkSpeed, int _health, float _healthGenerationRate, int _primarySkillDamage, float _primarySkillCooldown, int _primarySKillCharge, float _primarySkillRecharge, float _secondarySkillCooldown,
    int _bloodthirstHealAmount, int _hpBoostAmount, float _shieldGuardDamageReductionRate, float _frostbiteDamageRate, int _frostbiteDuration, float _chillSlowRate, float _chillDuration, float _rageStartRate, float _rageDamageRate,
    float _damageBoostMultiplier, float _fasterRechargeMultiplier, float _dashSpeedMultiplier, float _strongHeartMultiplier, float _smartMultiplier)
    {
        walkSpeed = _walkSpeed;
        health = _health;
        healthGenerationRate = _healthGenerationRate;
        primarySkillDamage = _primarySkillDamage;
        primarySkillCooldown = _primarySkillCooldown;
        primarySkillCharge = _primarySKillCharge;
        primarySkillRecharge = _primarySkillRecharge;
        secondarySkillCooldown = _secondarySkillCooldown;
        bloodthirstHealAmount = _bloodthirstHealAmount;
        hpBoostAmount = _hpBoostAmount;
        shieldGuardDamageReductionRate = _shieldGuardDamageReductionRate;
        frostbiteDamageRate = _frostbiteDamageRate;
        frostbiteDuration = _frostbiteDuration;
        chillSlowRate = _chillSlowRate;
        chillDuration = _chillDuration;
        rageStartRate = _rageStartRate;
        rageDamageRate = _rageDamageRate;
        damageBoostMultiplier = _damageBoostMultiplier;
        fasterRechargeMultiplier = _fasterRechargeMultiplier;
        dashSpeedMultiplier = _dashSpeedMultiplier;
        strongHeartMultiplier = _strongHeartMultiplier;
        smartMultiplier = _smartMultiplier;
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

    public float FasterRechargeMultiplier
    {
        get
        {
            return fasterRechargeMultiplier;
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
        set
        {
            walkSpeed = value;
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

    public float SmartMultiplier
    {
        get
        {
            return smartMultiplier;
        }
    }
}
