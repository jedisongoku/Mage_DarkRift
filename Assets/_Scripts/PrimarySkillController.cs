using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PrimarySkillController : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 1.5f;
    [SerializeField] private float maxTravelDistance;

    private bool isTraveling = false;
    private Vector3 particleMoveDirection = Vector3.zero;
    private bool isHit = false;
    private bool isPlayer = false;
    private int playerViewId;
    private float travelDistance = 0f;
    private Vector3 initialPosition = Vector3.zero;

    public int PlayerViewID { get; set; }
    public GameObject DamageOrigin { get; set; }


    // Start is called before the first frame update
    void OnEnable()
    {
        travelDistance = 0;
        initialPosition = transform.position;
        isHit = false;
        isTraveling = false;
        
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
    public bool Traveling
    {
        set
        {
            isTraveling = value;
        }
    }

    IEnumerator ParticleTravel()
    {
        if (particleMoveDirection != Vector3.zero)
        {
            travelDistance = (transform.position - initialPosition).magnitude;
            if(travelDistance >= maxTravelDistance)
            {
                GameObject obj;
                obj = ObjectPooler.Instance.GetPrimarySkillEnvironmentExplosionPrefab();
                obj.transform.position = transform.position;
                obj.SetActive(true);

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
        if(isTraveling && !isHit)
        {
            if (other.gameObject.layer == 16 && other.transform.parent.gameObject.GetComponent<PhotonView>() != null)
            {
                Debug.Log("Collided with :" + other.gameObject.name);
                if (other.transform.parent.gameObject.GetComponent<PhotonView>().ViewID != PlayerViewID && !isHit)
                {
                    if (other.transform.parent.gameObject.GetComponent<PlayerHealthManager>().CanTakeDamage()) DamageOrigin.GetComponent<PlayerCombatManager>().ApplyDamageToEnemy(other.transform.parent.gameObject);

                    isHit = true;
                    isPlayer = true;
                    Destroy(other.transform.parent.gameObject);
                }
            }
            else if (other.gameObject.layer == 11 || other.gameObject.layer == 21)
            {
                isHit = true;
                isPlayer = false;
                Destroy(other.gameObject);
            }
        }  
        
    }

    private void Destroy(GameObject other)
    {
        GameObject obj;
        if (!isPlayer)
        {
            Debug.Log("Not hittin anyhting");
            obj = ObjectPooler.Instance.GetPrimarySkillEnvironmentExplosionPrefab();
            obj.transform.position = transform.position;
            obj.SetActive(true);
        }
        
        //isHit = true;
        Invoke("DelayDisable", 0.5f);
    }

    private void DelayDisable()
    {
        gameObject.SetActive(false);
    }
}
