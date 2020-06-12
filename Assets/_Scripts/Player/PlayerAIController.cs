﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using TMPro;
using System.Linq;
using Photon.Realtime;

public class PlayerAIController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] CapsuleCollider searchCollider;
    [SerializeField] CapsuleCollider gemCollider;

    float turnSpeed = 8;
    public enum botState {patrol, attack, defense, spawn, flank};
    public botState botPlayerState;
    public botState botPlayerCurrentState;

    NavMeshAgent botController;
    [SerializeField] GameObject targetPlayer;
    Animator m_Animator;
    PlayerHealthManager playerHealthManager;
    PlayerCombatManager playerCombatManager;
    Quaternion m_Rotation = Quaternion.identity;
    Rigidbody m_Rigidbody;
    

    public int spawnLocation;


    public List<GameObject> playersInRange;
    public List<GameObject> nullPlayersRemoved;
    Vector3 networkPosition;
    Vector2 newMovement;
    Vector2 movement;

    int defenseHealthThreshold;
    private float fireTimer;
    private float spawnTimer = 0;

    // Start is called before the first frame update
    void Awake()
    {
        

        botController = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playersInRange = new List<GameObject>();
        GetComponent<PlayerMovementController>().isPlayer = false;
        GetComponent<PlayerLevelManager>().isPlayer = false;
        playerCombatManager.isPlayer = false;
        

        //StartCoroutine(StateSwitcher(botState.spawn, 0));
        defenseHealthThreshold = Random.Range(30, 60);

        if(PhotonNetwork.IsMasterClient)
        {
            searchCollider.enabled = true;
            //StartCoroutine(SanityCheck());

            //playerNameText.text = "Mage" + Random.Range(1000, 9999);
            playerNameText.text = PlayFabDataStore.botNames[Random.Range(0, PlayFabDataStore.botNames.Length)];
            playerCombatManager.killFeedName = playerNameText.text;
            GameManager.OnGameOver += GameOver;
        }
            

    }

    [PunRPC]
    void SetName(string _name)
    {
        Debug.Log("Bot Name Set " + _name);
        playerNameText.text = _name;
        playerCombatManager.killFeedName = _name;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        photonView.RPC("SetName", newPlayer, playerNameText.text);
    }
    /*
    IEnumerator StateSwitcher(botState newState, float time)
    {
        //Debug.Log("State Switcher " + newState);
        yield return new WaitForSeconds(time);

        botPlayerState = newState;
        switch (botPlayerState)
        {
            case botState.spawn:
                if(botPlayerCurrentState != botState.spawn)
                    StartCoroutine(Spawn());
                break;
            case botState.patrol:
                if (botPlayerCurrentState != botState.patrol)
                    StartCoroutine(Patrol());
                break;
            case botState.attack:
                if (botPlayerCurrentState != botState.attack)
                    StartCoroutine(Attack());
                break;
            case botState.flank:
                if (botPlayerCurrentState != botState.flank)
                    StartCoroutine(Flank());
                break;
            case botState.defense:
                if (botPlayerCurrentState != botState.defense)
                    StartCoroutine(Defense());
                break;

        }
    }*/

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if(botPlayerState == botState.spawn)
        {
            if(spawnTimer >= 2f)
            {
                botPlayerState = botState.patrol;
            }
        }
        else if (botPlayerState == botState.patrol)
        {
            Patrol();
        }
        else if (botPlayerState == botState.attack)
        {
            Attack();
        }
        else if (botPlayerState == botState.flank)
        {
            Flank();
        }
        else if (botPlayerState == botState.defense)
        {
            Defense();
        }
    }

    void Patrol()
    {
        if (botPlayerCurrentState != botState.patrol || Vector3.Distance(transform.position, botController.destination) < botController.stoppingDistance)
        {
            botPlayerCurrentState = botState.patrol;
            if (botController.enabled)
                botController.SetDestination(GetPatrolDestionation());
        }

        if (!searchCollider.enabled)
        {
            searchCollider.enabled = true;
            //StartCoroutine(SanityCheck());
        }

        if (targetPlayer != null)
        {
            if (Vector3.Distance(targetPlayer.transform.position, transform.position) > 12)
            {
                targetPlayer = null;
            }
            else
            {
                botPlayerState = botState.attack;
            }
        }
        else
        {
            if (playersInRange.Count > 0)
            {
                targetPlayer = playersInRange[0];
                botPlayerState = botState.attack;
            }
        }
    }

    void Attack()
    {
        botPlayerCurrentState = botState.attack;

        if (targetPlayer != null)
        {
            if (targetPlayer.GetComponent<PlayerCombatManager>().isSearchable)
                playerCombatManager.BotPlayerAutoAttack(targetPlayer);

            if (playerHealthManager.PlayerHealth < defenseHealthThreshold && !playerCombatManager.IsDead)
            {
                if (playerCombatManager.BotPlayerPrimarySkillCharge() == 0)
                {
                    botPlayerState = botState.defense;
                }
                else
                {
                    botPlayerState = botState.flank;
                }
            }
            else if (Vector3.Distance(targetPlayer.transform.position, transform.position) > 8f && botController.enabled)
            {
                botController.SetDestination(targetPlayer.transform.position);
                if (playerCombatManager.BotPlayerSecondarySkillAvailable() && Random.Range(0f, 1f) < 0.5f)
                {
                    playerCombatManager.BotPlayerDash();
                }

            }
            else if(botController.velocity.magnitude < 0.5f && botController.enabled)
            {
                botController.SetDestination(targetPlayer.transform.position);
            }
            
            if (playersInRange.Count == 0 && targetPlayer != null)
            {
                targetPlayer = null;
                botPlayerState = botState.patrol;
                //botPlayerState = botState.flank;
            }
            else if(targetPlayer.GetComponent<PlayerCombatManager>().IsDead)
            {
                if (playersInRange.Contains(targetPlayer)) playersInRange.Remove(targetPlayer);
                targetPlayer = null;
                botPlayerState = botState.patrol;
            }
        }
        else
        {
            if(playersInRange.Count > 0 && playersInRange[0].GetComponent<PlayerCombatManager>().isSearchable)
            {
                targetPlayer = playersInRange[0];
            }
            else
            {
                botPlayerState = botState.patrol;
            }
                
        }
    }

    void Flank()
    {
        if (botPlayerCurrentState != botState.flank)
        {
            botPlayerCurrentState = botState.flank;
            if (botController.enabled)
                botController.SetDestination(GetPatrolDestionation());
        }

        if (targetPlayer != null)
        {
            playerCombatManager.BotPlayerAutoAttack(targetPlayer);
        }

        Invoke("EndFlank", 2f);
        
    }

    void EndFlank()
    {
        if (targetPlayer != null)
        {
            if (Vector3.Distance(transform.position, targetPlayer.transform.position) > 8f)
            {
                botPlayerState = botState.attack;
            }
            else
            {
                botPlayerState = botState.patrol;
            }
        }
        else
        {
            botPlayerState = botState.patrol;
        }
    }

    void Defense()
    {
        if (botPlayerCurrentState != botState.defense)
        {
            botPlayerCurrentState = botState.defense;

            if (targetPlayer != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Vector3.Distance(targetPlayer.transform.position, GameManager.Instance.SpawnLocation(i)) > Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i))
                        && Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i)) > 5)
                    {
                        if (botController.enabled)
                            botController.SetDestination(GameManager.Instance.SpawnLocation(i));
                        break;
                    }
                }

                if (playerCombatManager.BotPlayerSecondarySkillAvailable())
                {
                    playerCombatManager.BotPlayerDash();
                }
            }
        }

        if (targetPlayer != null)
        {
            playerCombatManager.BotPlayerAutoAttack(targetPlayer);
        }

        Invoke("EndDefense", 1f);
  
    }

    void EndDefense()
    {
        if (botPlayerCurrentState == botState.defense)
        {
            if (GetComponent<PlayerHealthManager>().PlayerHealth > defenseHealthThreshold || botController.velocity.magnitude < botController.speed)
            {
                botPlayerState = botState.patrol;
            }
        }
    }

    public bool AttackTimer
    {
        set
        {
            fireTimer = 0;
        }
    }

    #region Deleted Enumarators

    /*
    IEnumerator Spawn()
    {
        botPlayerCurrentState = botState.spawn;

        yield return new WaitForSeconds(2);

        StartCoroutine(StateSwitcher(botState.patrol, 0));
    }

    
    IEnumerator Patrol()
    {
        if (botPlayerCurrentState != botState.patrol || Vector3.Distance(transform.position, botController.destination) < botController.stoppingDistance)
        {
            botPlayerCurrentState = botState.patrol;
            if (botController.enabled)
                botController.SetDestination(GetPatrolDestionation());
        }

        if(!searchCollider.enabled)
        {
            searchCollider.enabled = true;
            StartCoroutine(SanityCheck());
        }

        yield return new WaitForSeconds(0.1f);

        if (targetPlayer != null)
        {
            if(Vector3.Distance(targetPlayer.transform.position, transform.position) > 12)
            {
                targetPlayer = null;
            }
            else
            {
                StartCoroutine(StateSwitcher(botState.attack, 0));
            }          
        }   
        else
        {
            if(playersInRange.Count > 0)
            {
                targetPlayer = playersInRange[0];
                StartCoroutine(StateSwitcher(botState.attack, 0));
            }    
            else
                if (botPlayerState == botState.patrol) StartCoroutine(Patrol());
        }  
    }*/


    /*
    IEnumerator Attack()
    {
        botPlayerCurrentState = botState.attack;

        if (targetPlayer != null)
        {
            if(targetPlayer.GetComponent<PlayerCombatManager>().isSearchable)
                playerCombatManager.BotPlayerAutoAttack(targetPlayer);
            else
            {
                playersInRange.Remove(targetPlayer);
                if (playersInRange.Count > 0) targetPlayer = playersInRange[0];
            }

            if (playerHealthManager.PlayerHealth < defenseHealthThreshold && !playerCombatManager.IsDead)
            {
                if (playerCombatManager.BotPlayerPrimarySkillCharge() == 0)
                {
                    StartCoroutine(StateSwitcher(botState.defense, 0));
                }
            }
            else if (Vector3.Distance(targetPlayer.transform.position, transform.position) > 8f && botController.enabled)
            {
                botController.SetDestination(targetPlayer.transform.position);
                if (playerCombatManager.BotPlayerSecondarySkillAvailable() && Random.Range(0f, 1f) < 0.5f)
                {
                    playerCombatManager.BotPlayerDash();
                }

            }
            else if (botController.velocity.magnitude < botController.speed)
            {
                StartCoroutine(StateSwitcher(botState.flank, 0));
            }
        }
        else
        {
            StartCoroutine(StateSwitcher(botState.patrol, 0));
        }

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
        yield return new WaitForSeconds(0.25f);

        if (botPlayerState == botState.attack && !playerCombatManager.IsDead)
        {
            StartCoroutine(Attack());
        } 
    }

    IEnumerator Flank()
    {
        if (botPlayerCurrentState != botState.flank)
        {
            botPlayerCurrentState = botState.flank;
            if(botController.enabled)
                botController.SetDestination(GetPatrolDestionation());
        }

        if (targetPlayer != null)
        {
            playerCombatManager.BotPlayerAutoAttack(targetPlayer);
        }

        yield return new WaitForSeconds(Random.Range(2f,3f));

        if(targetPlayer != null)
        {
            if (Vector3.Distance(transform.position, targetPlayer.transform.position) > 8f)
            {
                StartCoroutine(StateSwitcher(botState.attack, 0));
            }
            else
            {
                StartCoroutine(StateSwitcher(botState.patrol, 0));
            }
        }
        else
        {
            StartCoroutine(StateSwitcher(botState.patrol, 0));
        }
       
    }

    IEnumerator Defense()
    {
        if (botPlayerCurrentState != botState.defense)
        {
            botPlayerCurrentState = botState.defense;

            if (targetPlayer != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Vector3.Distance(targetPlayer.transform.position, GameManager.Instance.SpawnLocation(i)) > Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i))
                        && Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i)) > 5)
                    {
                        if (botController.enabled)
                            botController.SetDestination(GameManager.Instance.SpawnLocation(i));
                        break;
                    }
                }

                if (playerCombatManager.BotPlayerSecondarySkillAvailable())
                {
                    playerCombatManager.BotPlayerDash();
                }
            }
        }

        if (targetPlayer != null)
        {
            playerCombatManager.BotPlayerAutoAttack(targetPlayer);
        }

        yield return new WaitForSeconds(1);

        if(botPlayerCurrentState == botState.defense)
        {
            if (GetComponent<PlayerHealthManager>().PlayerHealth > defenseHealthThreshold || botController.velocity.magnitude < botController.speed)
            {
                StartCoroutine(StateSwitcher(botState.patrol, 0));
            }
            else if(!playerCombatManager.IsDead)
            {
                StartCoroutine(Defense());
            }
        }
        //if (botPlayerState == botState.defense) StartCoroutine(Defense());
    }*/

    #endregion

    IEnumerator Dash()
    {

        yield return new WaitForSeconds(0f);

        playerCombatManager.BotPlayerDash();
    }

    IEnumerator SanityCheck()
    {
            
        yield return new WaitForSeconds(3);
        
        if(botController.velocity.magnitude < 3 && playersInRange.Count == 0)
        {
            botPlayerState = botState.patrol;
        }
        //StartCoroutine(SanityCheck());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        fireTimer += Time.fixedDeltaTime;

        if (!photonView.IsMine)
        {
            if ((m_Rigidbody.position - networkPosition).magnitude > 5f)
            {
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

        if (fireTimer > 0.15f)
        {
            m_Rigidbody.MoveRotation(m_Rotation);
        }
    }

    private void OnAnimatorMove()
    {
        if(m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Dash With Root Motion"))
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + botController.velocity.normalized * m_Animator.deltaPosition.magnitude);
        }
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
            if(botController.enabled)
                botController.SetDestination((Vector3)stream.ReceiveNext());

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            //botController.nextPosition += botController.velocity * lag;
        }
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

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient)
        {

            playersInRange.Add(other.gameObject);
            if (other.gameObject.GetComponent<PlayerCombatManager>().isPlayer && targetPlayer == null && other.gameObject.GetComponent<PlayerCombatManager>().isSearchable)
                targetPlayer = other.gameObject;

            if (botPlayerState != botState.spawn)
                botPlayerState = botState.attack;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient)
        {
            playersInRange.Remove(other.gameObject);

            if (playersInRange.Count == 0)
            {
                targetPlayer = null;
                botPlayerState = botState.patrol;
            }
            else
            {
                targetPlayer = playersInRange[0];
            }
        }
    }

    public void OnBotPlayerDeath()
    {
        gemCollider.enabled = false;
        searchCollider.enabled = false;
        StopAllCoroutines();
        playersInRange.Clear();
        targetPlayer = null;
        botController.enabled = false;
    }

    public void RespawnBotPlayer()
    {
        defenseHealthThreshold = Random.Range(25, 50);
        botController.enabled = true;
        gemCollider.enabled = true;
        searchCollider.enabled = true;
        botController.ResetPath();

        spawnTimer = 0;
        botPlayerState = botState.spawn;
    }

    bool LineOfSight(GameObject enemy)
    {
        RaycastHit hit;
        var rayDirection = (enemy.transform.position + transform.up) - (transform.position + transform.up);
        if (Physics.Raycast(transform.position + transform.up, rayDirection, out hit))
        {
            if (hit.transform.gameObject.layer == 8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameOver -= GameOver;
        ScoreManager.Instance.RemoveScoreboard(playerCombatManager.killFeedName);
    }

    void GameOver()
    {
        StopAllCoroutines();
        botController.enabled = false;
        this.enabled = false;
    }

    public void OwnershipChanged()
    {
        searchCollider.enabled = true;
        GameManager.OnGameOver += GameOver;

    }
    

}
