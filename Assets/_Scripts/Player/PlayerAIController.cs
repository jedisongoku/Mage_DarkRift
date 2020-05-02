﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using TMPro;
using System.Linq;

public class PlayerAIController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] CapsuleCollider searchCollider;

    float turnSpeed = 8;
    public enum botState {patrol, attack, defense, spawn, flank};
    public botState botPlayerState;
    public botState botPlayerCurrentState;

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
        

        botController = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playersInRange = new List<GameObject>();
        GetComponent<PlayerMovementController>().isPlayer = false;
        playerCombatManager.isPlayer = false;
        playerNameText.text = "Mage" + Random.Range(1000, 9999);
        playerCombatManager.killFeedName = playerNameText.text;

        StartCoroutine(StateSwitcher(botState.spawn, 0));
        defenseHealthThreshold = Random.Range(30, 60);

        if(PhotonNetwork.IsMasterClient)
        {
            searchCollider.enabled = true;
            StartCoroutine(SanityCheck());
        }
            

    }

    IEnumerator StateSwitcher(botState newState, float time)
    {
        Debug.Log("State Switcher " + newState);
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
    }

    IEnumerator Spawn()
    {
        botPlayerCurrentState = botState.spawn;

        yield return new WaitForSeconds(1);

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

        targetPlayer = ClosestBotPlayer(targetPlayer);
        if (targetPlayer != null) StartCoroutine(StateSwitcher(botState.attack, 0));

        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        if (botPlayerState == botState.patrol) StartCoroutine(Patrol());
    }

    IEnumerator Attack()
    {
        botPlayerCurrentState = botState.attack;

        targetPlayer = ClosestBotPlayer(targetPlayer);
        playerCombatManager.BotPlayerAutoAttack(targetPlayer);

        if (targetPlayer != null)
        {
            if (Vector3.Distance(targetPlayer.transform.position, transform.position) > 6f && botController.enabled)
            {
                botController.SetDestination(targetPlayer.transform.position);

            }
            else if (botController.velocity.magnitude < botController.speed)
            {
                StartCoroutine(StateSwitcher(botState.flank, Random.Range(0.2f, 1f)));
            }
        }
        else
        {
            StartCoroutine(StateSwitcher(botState.patrol, 0));
        }

        if (playerHealthManager.PlayerHealth < defenseHealthThreshold)
        {
            if (playerCombatManager.BotPlayerPrimarySkillCharge() == 0)
            {
                StartCoroutine(StateSwitcher(botState.defense, Random.Range(0.1f, 0.5f)));
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (botPlayerState == botState.attack) StartCoroutine(Attack());
    }

    IEnumerator Flank()
    {
        if (botPlayerCurrentState != botState.flank)
        {
            botPlayerCurrentState = botState.flank;
            if(botController.enabled)
                botController.SetDestination(GetPatrolDestionation());
        }

        targetPlayer = ClosestBotPlayer(targetPlayer);
        playerCombatManager.BotPlayerAutoAttack(targetPlayer);

        if (targetPlayer != null)
        {
            if (Vector3.Distance(transform.position, targetPlayer.transform.position) > 6f)
            {
                StartCoroutine(StateSwitcher(botState.attack, 0));
            }
        }
        else
        {
            StartCoroutine(StateSwitcher(botState.patrol, 0));
        }

        yield return new WaitForSeconds(1);

        if (botPlayerState == botState.flank) StartCoroutine(Flank());
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
                    if (Vector3.Distance(targetPlayer.transform.position, GameManager.Instance.SpawnLocation(i)) > Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i)))
                    {
                        if (botController.enabled)
                            botController.SetDestination(GameManager.Instance.SpawnLocation(i));
                        //RemoveTargetPlayer(targetPlayer);
                        break;
                    }
                }
            }
            else
            {
                StartCoroutine(StateSwitcher(botState.patrol, 0));
            }


            if (playerCombatManager.BotPlayerSecondarySkillAvailable())
            {
                StartCoroutine(Dash());
            }
        }
        else
        {
            targetPlayer = ClosestBotPlayer(targetPlayer);
            playerCombatManager.BotPlayerAutoAttack(targetPlayer);

            if (GetComponent<PlayerHealthManager>().PlayerHealth > 80 || botController.velocity.magnitude < botController.speed)
            {
                StartCoroutine(StateSwitcher(botState.patrol, Random.Range(0.2f, 1f)));
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (botPlayerState == botState.defense) StartCoroutine(Defense());
    }

    /*
    void Update()
    {
        if(PhotonNetwork.IsMasterClient && !playerCombatManager.IsDead && botController.enabled)
        {
            switch (botPlayerState)
            {
                case botState.spawn:

                    botPlayerCurrentState = botState.spawn;
                    StartCoroutine(StateSwitcher(botState.patrol, 0));

                    break;

                case botState.patrol:

                    if (botPlayerCurrentState != botState.patrol || Vector3.Distance(transform.position, botController.destination) < botController.stoppingDistance)
                    {
                        botPlayerCurrentState = botState.patrol;
                        botController.SetDestination(GetPatrolDestionation());
                    }
                    targetPlayer = ClosestBotPlayer(targetPlayer);
                    if(targetPlayer != null) StartCoroutine(StateSwitcher(botState.attack, 0));
                    break;

                case botState.attack:

                    botPlayerCurrentState = botState.attack;

                    targetPlayer = ClosestBotPlayer(targetPlayer);
                    playerCombatManager.BotPlayerAutoAttack(targetPlayer);

                    if(targetPlayer != null)
                    {
                        if (Vector3.Distance(targetPlayer.transform.position, transform.position) > 6f)
                        {
                            botController.SetDestination(targetPlayer.transform.position);

                        }
                        else if (botController.velocity.magnitude < botController.speed)
                        {
                            StartCoroutine(StateSwitcher(botState.flank, Random.Range(0.2f, 1f)));
                        }
                    }
                    else
                    {
                        StartCoroutine(StateSwitcher(botState.patrol, 0));
                    }

                    if (playerHealthManager.PlayerHealth < defenseHealthThreshold)
                    {
                        if (playerCombatManager.BotPlayerPrimarySkillCharge() == 0)
                        {
                            StartCoroutine(StateSwitcher(botState.defense, Random.Range(0.1f, 0.5f)));
                        }
                    }


                    break;
                case botState.defense:

                    if(botPlayerCurrentState != botState.defense)
                    {
                        botPlayerCurrentState = botState.defense;

                        if(targetPlayer != null)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                if (Vector3.Distance(targetPlayer.transform.position, GameManager.Instance.SpawnLocation(i)) > Vector3.Distance(transform.position, GameManager.Instance.SpawnLocation(i)))
                                {
                                    botController.SetDestination(GameManager.Instance.SpawnLocation(i));
                                    //RemoveTargetPlayer(targetPlayer);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            StartCoroutine(StateSwitcher(botState.patrol, 0));
                        }


                        if (playerCombatManager.BotPlayerSecondarySkillAvailable())
                        {
                            StartCoroutine(Dash());
                        }
                    }
                    else
                    {
                        targetPlayer = ClosestBotPlayer(targetPlayer);
                        playerCombatManager.BotPlayerAutoAttack(targetPlayer);

                        if(GetComponent<PlayerHealthManager>().PlayerHealth > 80 || botController.velocity.magnitude < botController.speed)
                        {
                            StartCoroutine(StateSwitcher(botState.patrol, Random.Range(0.2f, 1f)));
                        }
                    }
                    

                    break;
                
                case botState.flank:

                    if(botPlayerCurrentState != botState.flank)
                    {
                        botPlayerCurrentState = botState.flank;
                        botController.SetDestination(GetPatrolDestionation());
                    }

                    targetPlayer = ClosestBotPlayer(targetPlayer);
                    playerCombatManager.BotPlayerAutoAttack(targetPlayer);

                    if(targetPlayer != null)
                    {
                        if (Vector3.Distance(transform.position, targetPlayer.transform.position) > 6f)
                        {
                            StartCoroutine(StateSwitcher(botState.attack, 0));
                        }
                    }
                    else
                    {
                        StartCoroutine(StateSwitcher(botState.patrol, 0));
                    }
                    break;
            }
        }
        
    }*/

    IEnumerator Dash()
    {

        yield return new WaitForSeconds(0.2f);

        playerCombatManager.BotPlayerDash();
    }

    IEnumerator SanityCheck()
    {
        if(playersInRange.Count > 0)
        {
            //SelectTarget();
        }

        yield return new WaitForSeconds(1);
        
        if(botController.velocity.magnitude < 3 && playersInRange.Count == 0)
        {
            StartCoroutine(StateSwitcher(botState.patrol, 0));
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

    public void OnDeath()
    {
        StopAllCoroutines();
        botController.isStopped = true;
    }

    void SelectTarget()
    {
        foreach (var player in playersInRange)
        {
            if(player.GetComponent<PlayerCombatManager>().isSearchable && !player.GetComponent<PlayerCombatManager>().IsDead)
            {
                targetPlayer = player;
                StartCoroutine(StateSwitcher(botState.attack, 0));
                break;
            }   
        }
        if(targetPlayer == null) StartCoroutine(StateSwitcher(botState.patrol, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient)
        {
            playersInRange.Add(other.gameObject);
            StartCoroutine(StateSwitcher(botState.attack, 0));

        }
    }

    private void OnTriggerStay(Collider other)
    {
        
        if (other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient && !playerCombatManager.IsDead)
        {
            if (!playersInRange.Contains(other.gameObject) && !other.GetComponent<PlayerCombatManager>().IsDead)
            {
                playersInRange.Add(other.gameObject);
            }
            else if(playersInRange.Contains(other.gameObject) && other.GetComponent<PlayerCombatManager>().IsDead)
            {
                playersInRange.Remove(other.gameObject);
                //RemoveTargetPlayer(other.gameObject);
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8 && PhotonNetwork.IsMasterClient)
        {
            playersInRange.Remove(other.gameObject);
            if(playersInRange.Count == 0)
                StartCoroutine(StateSwitcher(botState.patrol, 0));
        }
    }

    void RemoveTargetPlayers(GameObject targetPlayer)
    {

        if(playersInRange.Contains(targetPlayer))
        {
            playersInRange.Remove(targetPlayer);
            Debug.Log("Removing Player");
            SelectTarget();
        }
    }

    public void OnBotPlayerDeath()
    {
        StopAllCoroutines();
        playersInRange.Clear();
        targetPlayer = null;
        botController.enabled = false;
    }

    public void RespawnBotPlayer()
    {
        defenseHealthThreshold = Random.Range(25, 50);
        botController.enabled = true;
        botController.ResetPath();

        StartCoroutine(StateSwitcher(botState.spawn, 0));
        Debug.Log("Bot Respawned");
    }

    public GameObject TargetPlayer
    {
        get
        {
            return targetPlayer;
        }
    }

    GameObject ClosestBotPlayer(GameObject targetEnemy)
    {
        GameObject closestEnemy = null;
        float distance = 12;

        if (targetEnemy != null)
        {
            if (!targetEnemy.GetComponent<PlayerCombatManager>().IsDead && targetEnemy.GetComponent<PlayerCombatManager>().isSearchable
                && distance > Vector3.Distance(targetEnemy.transform.position, transform.position))
            {
                Debug.Log("Attack same enemy");
                return targetEnemy;
            }
        }
        Debug.Log("Attack different enemy");

        foreach (var enemy in playersInRange)
        {
            if(enemy != null)
            {
                if (enemy.gameObject.layer == 8)
                {
                    if (!enemy.GetComponent<PlayerCombatManager>().IsDead && LineOfSight(enemy.gameObject) && enemy.GetComponent<PlayerCombatManager>().isSearchable)
                    {
                        if (distance > Vector3.Distance(enemy.gameObject.transform.position, transform.position))
                        {
                            distance = Vector3.Distance(enemy.gameObject.transform.position, transform.position);
                            closestEnemy = enemy.gameObject;
                        }
                    }
                }
            }
            

        }
        return closestEnemy;
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

    PlayerCombatManager FindTarget()
    {
        return Object.FindObjectsOfType<PlayerCombatManager>()
            .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
            .Where(p => p.isSearchable)
            .FirstOrDefault();
    }
}
