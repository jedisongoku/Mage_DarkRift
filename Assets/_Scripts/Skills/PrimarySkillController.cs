using DarkRift.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimarySkillController : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 1.5f;
    [SerializeField] private float maxTravelDistance;

    private Vector3 impactNormal;
    private Vector3 particleMoveDirection = Vector3.zero;
    private bool isPlayer = false;
    
    private float travelDistance = 0f;
    private Vector3 initialPosition = Vector3.zero;
    public IClient ServerClient { get; set; }
    public int PlayerOrigin { get; set; }
    public bool Traveling { get; set; }
    public bool FrostNova { get; set; }
    public bool Hit { get; set; }


    // Start is called before the first frame update
    void OnEnable()
    {
        travelDistance = 0;
        initialPosition = transform.position;
        Hit = false;
        Traveling = false;
        FrostNova = false;
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

        if(Hit)
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
        if(Traveling)
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
                        Debug.Log("Server Apply Damage");
                        ServerManager.Instance.serverPlayersInScene[ServerClient].GetComponent<PlayerCombatManager>().ReadyForDamage(other.gameObject.GetComponent<Player>().ServerClient);
                    }
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

    private void Destroy(GameObject other)
    {
        GameObject obj;
        if (FrostNova)
        {
            obj = ObjectPooler.Instance.GetPrimarySkillFrostNovaPrefab();
        }
        else
        {
            obj = ObjectPooler.Instance.GetPrimarySkillExplosionPrefab();
        }
        
        if(isPlayer)
        {
            if(FrostNova)
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
            if(!FrostNova)
            {
                obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, impactNormal);
            }
            
        }
        
        obj.SetActive(true);
        Hit = true;
        Invoke("DelayDisable", 0.25f);
    }

    private void DelayDisable()
    {
        gameObject.SetActive(false);
    }
}
