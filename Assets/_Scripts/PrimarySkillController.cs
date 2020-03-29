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
    private Vector3 particleDestination = Vector3.zero;
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
            Traveling = true;

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
        //Debug.Log("Collided with :" + other.gameObject.name);
        if(isTraveling)
        {
            if (other.gameObject.layer == 16 && other.transform.parent.gameObject.GetComponent<PhotonView>() != null)
            {
                Debug.Log("Collided with :" + other.gameObject.name);
                if (other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID != PlayerViewID)
                {
                    if (other.transform.parent.gameObject.GetComponent<PlayerHealthManager>().CanTakeDamage()) DamageOrigin.GetComponent<PlayerCombatManager>().ApplyDamageToEnemy(other.transform.parent.gameObject);

                    isPlayer = true;
                    Destroy(other.transform.parent.gameObject);
                }
            }
            else if (other.gameObject.layer == 11)
            {
                isPlayer = false;
                Destroy(other.gameObject);
            }
        }  
        
    }
    /*
    void ApplyDamage(GameObject _enemy)
    {
        _enemy.GetComponent<PlayerHealthManager>().OnPlayerHit(playerViewId, damageDone, isFrostbite, isChill, isFrostNova, isRage);
    }*/

    private void Destroy(GameObject other)
    {
        GameObject obj;
        if (!isPlayer)
        {
            obj = ObjectPooler.Instance.GetPrimarySkillEnvironmentExplosionPrefab();
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
