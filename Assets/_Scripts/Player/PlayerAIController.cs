using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using TMPro;

public class PlayerAIController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] CapsuleCollider searchCollider;

    float turnSpeed = 8;
    enum botState {patrol, attack, defense, spawn, flank};
    botState botPlayerState;
    botState botPlayerCurrentState;

    NavMeshAgent botController;
    GameObject targetPlayer;
    GameObject threatPlayer;
    Animator m_Animator;
    PlayerHealthManager playerHealthManager;
    PlayerCombatManager playerCombatManager;
    Quaternion m_Rotation = Quaternion.identity;
    Rigidbody m_Rigidbody;
    

    public int spawnLocation;


    List<GameObject> playersInRange;
    Vector3 networkPosition;
    Vector2 newMovement;
    Vector2 movement;

    int defenseHealthThreshold;

    // Start is called before the first frame update
    void Awake()
    {
        playerNameText.text = "Mage" + Random.Range(1000, 9999);

        botController = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playersInRange = new List<GameObject>();
        GetComponent<PlayerMovementController>().isPlayer = false;
        playerCombatManager.isPlayer = false;

        botPlayerState = botState.patrol;
        botPlayerCurrentState = botState.spawn;
        defenseHealthThreshold = Random.Range(30, 60);

        if(PhotonNetwork.IsMasterClient)
        {
            searchCollider.enabled = true;
            StartCoroutine(SanityCheck());
        }
            

    }

    void Update()
    {
     
        if(PhotonNetwork.IsMasterClient && !playerCombatManager.IsDead && botController.enabled)
        {
            switch (botPlayerState)
            {
                case botState.attack:

                    botPlayerCurrentState = botState.attack;

                    if (targetPlayer != null)
                    {
                        if (!targetPlayer.GetComponent<PlayerCombatManager>().IsDead)
                        {
                            if (Vector3.Distance(targetPlayer.transform.position, transform.position) < 12f)
                            {
                                botController.SetDestination(targetPlayer.transform.position);
                                if (playerHealthManager.PlayerHealth > defenseHealthThreshold)
                                {
                                    if (Vector3.Distance(targetPlayer.transform.position, transform.position) <= botController.stoppingDistance && botPlayerCurrentState != botState.flank)
                                    {
                                        if(Random.Range(0,2) == 0)
                                            botPlayerState = botState.flank;
                                    }
                                    else
                                        playerCombatManager.BotPlayerAutoAttack(targetPlayer);
                                    //Keep Attacking

                                }
                                else
                                {

                                    if (playerCombatManager.BotPlayerPrimarySkillCharge() > 0 && Vector3.Distance(targetPlayer.transform.position, transform.position) < 5f
                                            && targetPlayer.GetComponent<PlayerHealthManager>().PlayerHealth < PlayFabDataStore.playerBaseStats.PrimarySkillDamage * playerCombatManager.BotPlayerPrimarySkillCharge())
                                    {
                                        botController.SetDestination(targetPlayer.transform.position);
                                        playerCombatManager.BotPlayerAutoAttack(targetPlayer);
                                    }
                                    else
                                    {
                                        botPlayerState = botState.defense;
                                    }

                                }

                            }
                            else
                            {
                                if (playerCombatManager.BotPlayerSecondarySkillAvailable())
                                {
                                    playerCombatManager.BotPlayerDash();
                                }
                                else if (Vector3.Distance(targetPlayer.transform.position, transform.position) > 13f)
                                {
                                    RemoveTargetPlayer(targetPlayer);
                                }


                            }

                        }
                        else
                        {
                            botPlayerState = botState.patrol;
                            RemoveTargetPlayer(targetPlayer);
                        }
                    }
                    else
                    {
                        RemoveTargetPlayer(targetPlayer);
                    }

                    break;
                case botState.defense:
                    botPlayerCurrentState = botState.defense;
                    for (int i = 0; i < 8; i++)
                    {
                        if (Vector3.Distance(targetPlayer.transform.position, GameManager.Instance.SpawnLocation(i)) > Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i)))
                        {
                            botController.SetDestination(GameManager.Instance.SpawnLocation(i));
                            RemoveTargetPlayer(targetPlayer);
                            break;
                        }
                    }
                    if (playerCombatManager.BotPlayerSecondarySkillAvailable())
                    {
                        StartCoroutine(Dash());
                    }

                    break;
                case botState.patrol:
                    if (botPlayerCurrentState != botState.patrol || Vector3.Distance(transform.position, botController.destination) < botController.stoppingDistance)
                    {
                        botPlayerCurrentState = botState.patrol;
                        botController.SetDestination(GetPatrolDestionation());
                    }
                    break;
                case botState.flank:
                    botPlayerCurrentState = botState.flank;
                    if (Vector3.Distance(targetPlayer.transform.position, transform.position) > botController.stoppingDistance + Random.Range(2, 4))
                        botPlayerState = botState.attack;
                    for (int i = 0; i < 8; i++)
                    {
                        if (Vector3.Distance(targetPlayer.transform.position, GameManager.Instance.SpawnLocation(i)) > Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i)))
                        {
                            botController.SetDestination(GameManager.Instance.SpawnLocation(i));
                            break;
                        }
                    }
                    break;
            }
        }
        
    }

    IEnumerator Dash()
    {

        yield return new WaitForSeconds(0.2f);

        playerCombatManager.BotPlayerDash();
    }

    IEnumerator SanityCheck()
    {
        if(playersInRange.Count > 0 && botPlayerState != botState.attack)
        {
            foreach (var player in playersInRange)
            {
                if (!player.GetComponent<PlayerCombatManager>().isTransparent || !player.GetComponent<PlayerCombatManager>().isInvisible)
                {
                    targetPlayer = player;
                    botPlayerState = botState.attack;
                    break;
                }

            }
        }
        yield return new WaitForSeconds(1);

        if(botController.velocity.magnitude < 3 && playersInRange.Count == 0)
        {
            botPlayerState = botState.patrol;
        }
        StartCoroutine(SanityCheck());
    }

    // Update is called once per frame
    void FixedUpdate()
    {    
        if(!photonView.IsMine)
        {
            if ((m_Rigidbody.position - networkPosition).magnitude > 5f)
            {
                Debug.Log("Position Correction");
                m_Rigidbody.position = networkPosition;
            }

            botController.nextPosition = Vector3.MoveTowards(botController.nextPosition, networkPosition, Vector3.Distance(botController.nextPosition, networkPosition) * (1.0f / PhotonNetwork.SerializationRate));
        }

        movement = new Vector2(botController.velocity.x, botController.velocity.z);
        movement.Normalize();
        m_Animator.SetFloat("Horizontal", movement.x * 5);
        m_Animator.SetFloat("Vertical", movement.y * 5);

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, botController.velocity.normalized, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
    }

    private void OnAnimatorMove()
    {
        if(m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Dash With Root Motion"))
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + botController.velocity.normalized * m_Animator.deltaPosition.magnitude);
        }

        m_Rigidbody.MoveRotation(m_Rotation);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Rigidbody.position);
            stream.SendNext(botController.destination);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            botController.SetDestination((Vector3)stream.ReceiveNext());

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            //botController.nextPosition += botController.velocity * lag;
        }
    }


    void FindPlayer(Vector3 target)
    {
        botController.SetDestination(target);
    }

    Vector3 GetPatrolDestionation()
    {
        int index = spawnLocation;
        do
        {
            index = GameManager.Instance.SpawnLocationIndex;
        } while (index == spawnLocation);
        spawnLocation = index;
        return GameManager.Instance.SpawnLocation(spawnLocation);
    }

    public void OnDeath()
    {
        StopAllCoroutines();
        botController.isStopped = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient)
        {               
            if (other.GetComponent<PhotonView>().IsMine && !other.gameObject.GetComponent<PlayerCombatManager>().isTransparent)
                playersInRange.Add(other.gameObject);
            else if (!other.GetComponent<PhotonView>().IsMine && !other.gameObject.GetComponent<PlayerCombatManager>().isInvisible)
                playersInRange.Add(other.gameObject);
            else if(other.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
            {
                playersInRange.Add(other.gameObject);
            }

            if(playersInRange.Count > 0)
            {
                targetPlayer = playersInRange[0];
                botPlayerState = botState.attack;
            }
            

            if (playersInRange.Count > 1 && botPlayerCurrentState == botState.defense)
            {
                RemoveTargetPlayer(targetPlayer);
            }
            
            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient && !playerCombatManager.IsDead)
        {
            
            if (botPlayerState != botState.attack)
            {
                if (other.GetComponent<PhotonView>().IsMine && !other.gameObject.GetComponent<PlayerCombatManager>().isTransparent)
                {
                    if (playerHealthManager.PlayerHealth > defenseHealthThreshold && botPlayerState != botState.flank)
                    {
                        targetPlayer = other.gameObject;
                        botPlayerState = botState.attack;
                    }
                    else
                    {
                        playerCombatManager.BotPlayerAutoAttack(other.gameObject);
                    }
                        
                }    
                else if (!other.GetComponent<PhotonView>().IsMine && !other.gameObject.GetComponent<PlayerCombatManager>().isInvisible)
                {
                    if (playerHealthManager.PlayerHealth > defenseHealthThreshold && botPlayerState != botState.flank)
                    {
                        targetPlayer = other.gameObject;
                        botPlayerState = botState.attack;
                    }
                    else
                    {
                        playerCombatManager.BotPlayerAutoAttack(other.gameObject);
                    }
                }
                else if (other.gameObject.GetComponent<PlayerCombatManager>().canBeSeen)
                {
                    if (playerHealthManager.PlayerHealth > defenseHealthThreshold && botPlayerState != botState.flank)
                    {
                        targetPlayer = other.gameObject;
                        botPlayerState = botState.attack;
                    }
                    else
                    {
                        playerCombatManager.BotPlayerAutoAttack(other.gameObject);
                    }
                }

            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient)
        { 
            if(other.gameObject == threatPlayer)
            {
                threatPlayer = null;
            }
        }
    }

    void RemoveTargetPlayer(GameObject targetPlayer)
    {
        if(playersInRange.Contains(targetPlayer))
        {
            playersInRange.Remove(targetPlayer);
            if (playersInRange.Count == 0)
            {
                targetPlayer = null;
                botPlayerState = botState.patrol;
            }
            else
            {
                targetPlayer = playersInRange[0];
                botPlayerState = botState.attack;
            }
        }
    }

    public void OnBotPlayerDeath()
    {
        playersInRange.Clear();
        botController.enabled = false;
    }

    public void RespawnBotPlayer()
    {
        defenseHealthThreshold = Random.Range(30, 60);
        botController.enabled = true;
        botController.ResetPath();
        
        botPlayerState = botState.patrol;
        botPlayerCurrentState = botState.spawn;
        Debug.Log("Bot Respawned");
    }

    public GameObject TargetPlayer
    {
        get
        {
            return targetPlayer;
        }
    }
}
