using DarkRift.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public IClient ServerClient { get; set; }

    [Header("Runes")]
    bool isFrostbite = false;
    bool isPoison = false;
    bool isChill = false;
    bool isBouncy = false;
    bool isRage = false;
    bool isFrostNova = false;


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
            Debug.Log("Origin : " + PlayerOrigin);

            

            if (other.gameObject.layer == 8)
            {
                if(other.gameObject.GetComponent<Player>().ID != PlayerOrigin)
                {
                    if (other.gameObject.GetComponent<Player>().IsServer)
                    {
                        //if collision happens on the server, notify the player to get ready for damage to the enemy player
                        ServerManager.Instance.serverPlayersInScene[ServerClient].GetComponent<PlayerCombatManager>().ReadyForDamage(other.gameObject.GetComponent<Player>().ServerClient);
                    }

                    Destroy(other.gameObject);

                }
                
                
                /*
                if (other.gameObject.GetComponent<PhotonView>().ViewID != playerViewId)
                {

                    if (isRage) damageDone += damageDone * PlayerBaseStats.Instance.RageDamageRate;
                    //other.gameObject.GetComponent<PlayerHealthManager>().DamageTaken = damageDone;
                    other.gameObject.GetComponent<PlayerHealthManager>().DamageOrigin = playerViewId;
                    other.gameObject.GetComponent<PlayerHealthManager>().TakeDamage(damageDone);
                    if (isFrostbite) other.gameObject.GetComponent<PlayerHealthManager>().StartFrostbite(playerViewId);
                    if (isChill) other.gameObject.GetComponent<PlayerMovementController>().StartChill(PlayerBaseStats.Instance.ChillDuration);
                    isPlayer = true;
                    
                }*/
            }
            else if (other.gameObject.layer == 11)
            {
                isPlayer = false;
                Destroy(other.gameObject);
            }
        }
        
        
    }

    private void Destroy(GameObject other)
    {
        GameObject obj;
        if (isFrostNova)
        {
            obj = ObjectPooler.Instance.GetPrimarySkillFrostNovaPrefab();
        }
        else
        {
            obj = ObjectPooler.Instance.GetPrimarySkillExplosionPrefab();
        }
        
        if(isPlayer)
        {
            if(isFrostNova)
            {
                obj.transform.position = other.transform.position + Vector3.up;
            }
            else
            {
                obj.transform.position = other.transform.position + Vector3.up;
                obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, impactNormal);
            }  
            
        }
        else
        {
            obj.transform.position = transform.position;
            if(!isFrostNova)
            {
                obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, impactNormal);
            }
            
        }
        
        obj.SetActive(true);
        isHit = true;
        Invoke("DelayDisable", 0.25f);
    }

    private void DelayDisable()
    {
        gameObject.SetActive(false);
    }
}
