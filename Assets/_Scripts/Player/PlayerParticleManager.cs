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
    [SerializeField] GameObject poison;
    [SerializeField] GameObject dashLock;

    // Start is called before the first frame update

    public void DisableParticles()
    {
        Frostbite(false);
        Chill(false);
        ShieldGuard(false);
        StrongHeart(false);
        Rage(false);
        ShieldGuard(false);
        Poison(false);
        DashLock(false);
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

    public void Poison(bool _value)
    {
        poison.SetActive(_value);
    }

    public void DashLock(bool _value)
    {
        //dashLock.SetActive(_value);
    }


}
