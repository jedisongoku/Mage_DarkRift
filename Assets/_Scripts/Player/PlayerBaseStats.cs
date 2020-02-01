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
    [SerializeField] private float frostbiteDamageRate;
    [SerializeField] private int frostbiteDuration;
    [SerializeField] private float chillSlowRate;
    [SerializeField] private float chillDuration;
    [SerializeField] private float multishotDelayTime;
    [SerializeField] private float rageStartRate;
    [SerializeField] private float rageDamageRate;
    [SerializeField] private float strongHeartRate;
    [SerializeField] private float poisonPickupTime;
    [SerializeField] private float poisonDamageRate;
    [SerializeField] private int poisonDuration;




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

    public float StrongHeartRate
    {
        get
        {
            return strongHeartRate;
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

    public float MultishotDelayTime
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

    public float PoisonDamageRate
    {
        get
        {
            return poisonDamageRate;
        }
    }

    public int PoisonDuration
    {
        get
        {
            return poisonDuration;
        }
    }
}
