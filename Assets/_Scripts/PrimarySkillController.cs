using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PrimarySkillController : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 1.5f;
    [SerializeField] private float maxTravelDistance;

    private bool isTraveling = false;
    private Vector3 impactNormal;
    private Vector3 particleMoveDirection = Vector3.zero;
    private float damageDone = 0;
    private float destroyTimer = 0f;
    private bool isHit = false;
    private bool isPlayer = false;
    private int playerViewId;
    private float travelDistance = 0f;
    private Vector3 initialPosition = Vector3.zero;

    [Header("Runes")]
    bool isFrostbite = false;
    bool isPoison = false;
    bool isChill = false;
    bool isBouncy = false;
    bool isRage = false;
    bool isFrostNova = false;

    public int PlayerViewID { get; set; }
    public GameObject DamageOrigin { get; set; }


    // Start is called before the first frame update
    void OnEnable()
    {
        travelDistance = 0;
        initialPosition = transform.position;
        isHit = false;
        isTraveling = false;
        isFrostbite = false;
        isRage = false;
        isChill = false;
        isBouncy = false;
        isFrostNova = false;
        
    }

    public bool Frostbite
    {
        get
        {
            return isFrostbite;
        }
        set
        {
            isFrostbite = value;
        }
    }

    public bool Chill
    {
        get
        {
            return isChill;
        }
        set
        {
            isChill = value;
        }
    }

    public bool Rage
    {
        get
        {
            return isRage;
        }
        set
        {
            isRage = value;
        }
    }

    public int PlayerOrigin
    {
        set
        {
            playerViewId = value;
        }
        get
        {
            return playerViewId;
        }
    }

    


    //Player sets the direction of where the particle will move
    public Vector3 SetParticleMoveDirection
    {
        set
        {
            particleMoveDirection = value;
            StartCoroutine(ParticleTravel());

        }
    }

    //Player sets the damage the particle with hit
    public float DamageDone
    {
        get
        {
            return damageDone;
        }
        set
        {
            damageDone = value;
        }
    }

    public bool Traveling
    {
        set
        {
            isTraveling = value;
        }
    }

    public bool FrostNova
    {
        set
        {
            isFrostNova = value;
        }
    }


    IEnumerator ParticleTravel()
    {
        if (particleMoveDirection != Vector3.zero)
        {
            travelDistance = (transform.position - initialPosition).magnitude;
            if(travelDistance >= maxTravelDistance)
            {

                gameObject.SetActive(false);
            }
            transform.Translate(particleMoveDirection * travelSpeed * Time.deltaTime);
            yield return null;

        }

        if(isHit)
        {
            yield return null;
        }
        else
        {
            StartCoroutine(ParticleTravel());
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(isTraveling)
        {
            Debug.Log("HIT : " + LayerMask.LayerToName(other.gameObject.layer));
            if (other.gameObject.layer == 8 && other.gameObject.GetComponent<PhotonView>() != null)
            {
                if (other.gameObject.GetComponent<PhotonView>().ViewID != PlayerViewID)
                {

                    //if (isRage) damageDone += damageDone * PlayerBaseStats.Instance.RageDamageRate;
                    //other.gameObject.GetComponent<PlayerHealthManager>().DamageTaken = damageDone;

                    //if (other.gameObject.GetComponent<PlayerHealthManager>().CanTakeDamage()) ApplyDamage(other.gameObject);
                    if (other.gameObject.GetComponent<PlayerHealthManager>().CanTakeDamage()) DamageOrigin.GetComponent<PlayerCombatManager>().ApplyDamageToEnemy(other.gameObject);
                    /*
                    other.gameObject.GetComponent<PlayerHealthManager>().DamageOrigin = playerViewId;
                    other.gameObject.GetComponent<PlayerHealthManager>().TakeDamage(damageDone);
                    if (isFrostbite) other.gameObject.GetComponent<PlayerHealthManager>().StartFrostbite(playerViewId);
                    if (isChill) other.gameObject.GetComponent<PlayerMovementController>().StartChill(PlayerBaseStats.Instance.ChillDuration);*/

                    isPlayer = true;
                    Destroy(other.gameObject);
                }
            }
            else if (other.gameObject.layer == 11)
            {
                isPlayer = false;
                Destroy(other.gameObject);
            }
        }  
        
    }

    void ApplyDamage(GameObject _enemy)
    {
        _enemy.GetComponent<PlayerHealthManager>().OnPlayerHit(playerViewId, damageDone, isFrostbite, isChill, isFrostNova, isRage);
    }

    private void Destroy(GameObject other)
    {
        GameObject obj;
        if (isFrostNova && !isPlayer)
        {
            obj = ObjectPooler.Instance.GetPrimarySkillFrostNovaPrefab();
            obj.transform.position = transform.position;
            obj.SetActive(true);
        }
        
        
        isHit = true;
        Invoke("DelayDisable", 0.25f);
    }

    private void DelayDisable()
    {
        gameObject.SetActive(false);
    }
}
