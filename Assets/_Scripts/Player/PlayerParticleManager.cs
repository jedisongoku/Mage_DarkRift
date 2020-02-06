using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerParticleManager : MonoBehaviour
{
    
    [SerializeField] GameObject frostbite;
    [SerializeField] GameObject strongHeart;
    [SerializeField] GameObject shieldGuard;
    [SerializeField] GameObject healing;
    [SerializeField] GameObject dashTrail;
    [SerializeField] GameObject chill;
    [SerializeField] GameObject rage;
    [SerializeField] GameObject flameCircle;
    [SerializeField] GameObject poison;
    [SerializeField] GameObject dashLock;
    [SerializeField] GameObject distortAim;

    // Start is called before the first frame update

    public void DisableParticles()
    {
        Frostbite(false);
        Chill(false);
        ShieldGuard(false);
        StrongHeart(false);
        Rage(false);
        ShieldGuard(false);
        FlameCircle(false);
        Poison(false);
        DashLock(false);
        DistortAim(false);
        WintersChill(false);
    }

    public void Frostbite(bool _value)
    {
        frostbite.SetActive(_value);
    }

    public void Chill(bool _value)
    {
        chill.SetActive(_value);
    }

    public void Bloodthirst()
    {
        healing.GetComponent<ParticleSystem>().Play();
    }

    public void HpBoost()
    {
        healing.GetComponent<ParticleSystem>().Play();
    }

    public void Rage(bool _value)
    {
        rage.SetActive(_value);
    }

    public void ShieldGuard(bool _value)
    {
        shieldGuard.SetActive(_value);
    }

    public void StrongHeart(bool _value)
    {
        strongHeart.SetActive(_value);
    }

    public void FlameCircle(bool _value)
    {
        flameCircle.SetActive(_value);
    }

    public void Poison(bool _value)
    {
        poison.SetActive(_value);
    }

    public void DashLock(bool _value)
    {

        if(GetComponent<Player>().IsControllable)
        {
            GetComponent<PlayerCombatManager>().IsDashLocked = _value;
            HUDManager.Instance.DisableDash(_value);
        }
        dashLock.SetActive(_value);
    }

    public void WintersChill(bool _value)
    {
        chill.SetActive(_value);
    }

    public void DistortAim(bool _value)
    {
        if (GetComponent<Player>().IsControllable)
        {
            GetComponent<PlayerCombatManager>().IsAimDistorted = _value;
        }
        distortAim.SetActive(_value);
    }


}
