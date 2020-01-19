using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseStats : MonoBehaviour
{
    public static PlayerBaseStats Instance;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private int health;
    [SerializeField] private float healthGenerationRate;
    [SerializeField] private int primarySkillDamage;
    [SerializeField] private float primarySkillCooldown;
    [SerializeField] private float primarySkillBoostMultiplier;
    [SerializeField] private float primarySkillSpeedMultiplier;
    [SerializeField] private float secondarySkillCooldown;
    [SerializeField] private float secondarySkillSpeedMultiplier;
    [SerializeField] private int bloodthirstHealAmount;
    [SerializeField] private int hpBoostAmount;
    [SerializeField] private float shieldGuardDamageReductionRate;
    [SerializeField] private float flameDamageRate;
    [SerializeField] private int flameDuration;
    [SerializeField] private float chillSlowRate;
    [SerializeField] private float chillDuration;
    [SerializeField] private float multishotDelayTime;
    [SerializeField] private float rageStartRate;
    [SerializeField] private float rageDamageRate;
    [SerializeField] private float poisonPickupTime;



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

    public float PrimarySkillBoostMultiplier
    {
        get
        {
            return primarySkillBoostMultiplier;
        }
    }

    public float PrimarySkillSpeedMultiplier
    {
        get
        {
            return primarySkillSpeedMultiplier;
        }
    }

    public float SecondarySkillCooldown
    {
        get
        {
            return secondarySkillCooldown;
        }
    }

    public float SecondarySkillSpeedMultiplier
    {
        get
        {
            return secondarySkillSpeedMultiplier;
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

    public float FlameDamageRate
    {
        get
        {
            return flameDamageRate;
        }
    }

    public int FlameDuration
    {
        get
        {
            return flameDuration;
        }
    }

    public float WalkSpeed
    {
        get
        {
            return walkSpeed;
        }
    }

    public float TurnSpeed
    {
        get
        {
            return turnSpeed;
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
