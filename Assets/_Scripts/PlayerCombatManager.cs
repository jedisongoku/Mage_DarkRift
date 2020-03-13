﻿using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Photon.Pun;

public class PlayerCombatManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private PlayerHealthManager playerHealthManager;
  
    Animator m_Animator;
    bool isDead = false;
    Vector3 mousePosition = Vector3.zero;
    Vector3 spawnPosition = Vector3.zero;

    [Header("Player Skills Stats")]

    [SerializeField] private float primarySkillCooldown = 3f;
    [SerializeField] private float secondarySkillCooldown = 8f;
    [SerializeField] private GameObject primarySkillSpawnLocation;
    private int primarySkillDamage = 0;
    private float primarySkillCooldownTimer = 3f;
    private float secondarySkillCooldownTimer = 8f;

    [Header("Player")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private MeshRenderer playerBaseRenderer;
    [SerializeField] private Material playerBaseColor;
    [SerializeField] private GameObject runeActivatedBlue;
    [SerializeField] private GameObject runeActivatedRed;
    [SerializeField] private GameObject dashTrail;


    [Header("Runes")]
    bool isFrostbite = false;
    bool isPoison = false;
    bool isChill = false;
    bool isBouncy = false;
    bool isRage = false;
    bool isMultiShot = false;
    bool isFrostNova = false;



    #region Private Methods
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            //playerMovementController.enabled = true;
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
            playerBaseRenderer.material = playerBaseColor;


        }

        m_Animator = GetComponent<Animator>();
        SetPlayerBaseStats();
    }

    // Update is called once per frame
    void Update()
    {
        

        if(photonView.IsMine && !isDead)
        {
            primarySkillCooldownTimer += Time.deltaTime;
            secondarySkillCooldownTimer += Time.deltaTime;

            //HUDManager.Instance.SetPrimarySkillCooldownUI = 1 - primarySkillCooldownTimer / primarySkillCooldown;
            //HUDManager.Instance.SetSecondarySkillCooldownUI = 1 - secondarySkillCooldownTimer / secondarySkillCooldown;

            if (Input.GetButtonDown("Fire1"))
            {
                //Get Player Fire Direction
                PrimarySkill();
                

            }
            if(Input.GetButtonDown("Dash"))
            {
                Debug.Log("DASHING");
                SecondarySkill();
            }
        }     
    }

    public void PrimarySkill()
    {
        if (primarySkillCooldownTimer >= primarySkillCooldown)
        {
            playerMovementController.SetFireDirection();
            primarySkillCooldownTimer = 0f;
            photonView.RPC("UsePrimarySkill", RpcTarget.AllViaServer, playerMovementController.MousePosition, primarySkillSpawnLocation.transform.position, isMultiShot, playerHealthManager.Rage);
        }
    }

    public void SecondarySkill()
    {
        if (secondarySkillCooldownTimer >= secondarySkillCooldown)
        {
            secondarySkillCooldownTimer = 0f;
            photonView.RPC("UseSecondarySkill", RpcTarget.AllViaServer);
            Debug.Log("DASHING THROUGH THE SNOW");
        }
    }

    [PunRPC]
    void UseSecondarySkill()
    {
        m_Animator.SetTrigger("Dashing");
        dashTrail.SetActive(false);
        dashTrail.SetActive(true);
    }

    [PunRPC]
    void UsePrimarySkill(Vector3 _mousePosition, Vector3 _spawnPosition, bool _isMultishot, bool _isRage)
    {       
        playerMovementController.MousePosition = _mousePosition;
        playerMovementController.Attack = true;
        spawnPosition = _spawnPosition;
        mousePosition = _mousePosition;
        isRage = _isRage;
        StartCoroutine(PrimarySkill(0f));
        if(_isMultishot) StartCoroutine(PrimarySkill(PlayerBaseStats.Instance.Multishot));
    }
    IEnumerator PrimarySkill(float _delayTime)
    {
        m_Animator.SetTrigger("Attacking");
        Vector3 heading = mousePosition - primarySkillSpawnLocation.transform.position;
        Vector3 direction = heading / heading.magnitude;
        yield return new WaitForSeconds(_delayTime);
        GameObject obj = ObjectPooler.Instance.GetPrimarySkillPrefab();
        //if (obj == null) StopCoroutine(PrimarySkill());
        obj.transform.position = primarySkillSpawnLocation.transform.position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        obj.GetComponent<PrimarySkillController>().SetParticleMoveDirection = new Vector3(direction.x, 0, direction.z);
        obj.GetComponent<PrimarySkillController>().PlayerOrigin = photonView.ViewID;
        obj.GetComponent<PrimarySkillController>().DamageDone = primarySkillDamage;
        if (isFrostbite) obj.GetComponent<PrimarySkillController>().Frostbite = isFrostbite;
        if (isChill) obj.GetComponent<PrimarySkillController>().Chill = isChill;
        if (isRage) obj.GetComponent<PrimarySkillController>().Rage = isRage;
        if (isFrostNova) obj.GetComponent<PrimarySkillController>().FrostNova = isFrostNova;
        obj.GetComponent<PrimarySkillController>().Traveling = true;


        //GameObject particle = Instantiate(primarySkillPrefab, primarySkillSpawnLocation.transform.position, Quaternion.identity) as GameObject;
    }
    void DisablePlayer()
    {
        if(photonView.IsMine)
        {
            playerMovementController.enabled = false;
        }    
        GetComponent<CapsuleCollider>().enabled = false;
    }

    void SetPlayerBaseStats()
    {
        primarySkillDamage = PlayerBaseStats.Instance.PrimarySkillDamage;
        primarySkillCooldown = PlayerBaseStats.Instance.PrimarySkillCooldown;
        secondarySkillCooldown = PlayerBaseStats.Instance.SecondarySkillCooldown;
        isFrostbite = false;
        isPoison = false;
        isChill = false;
        isBouncy = false;
        isRage = false;
        isMultiShot = false;
        isFrostNova = false;
    }

    public void RespawnPlayer()
    {
        SetPlayerBaseStats();
        PlayerRuneManager.Instance.RestartPlayerRunes();
        photonView.RPC("RespawnClients", RpcTarget.All, GameManager.Instance.SpawnLocationIndex);

    }

    [PunRPC]
    void RespawnClients(int _spawnLocationIndex)
    {
        if(!photonView.IsMine)
        {
            playerUI.SetActive(false);
            playerModel.SetActive(false);
            playerBaseRenderer.enabled = false;
            Invoke("EnableRespawnedPlayerClients", 1f);
        }
        transform.position = GameManager.Instance.SpawnLocation(_spawnLocationIndex);
        m_Animator.SetTrigger("Respawn");
        GetComponent<CapsuleCollider>().enabled = true;
        playerHealthManager.RespawnPlayer();
        IsDead = false;
        playerMovementController.enabled = true;

        if (photonView.IsMine)
        {
            
            Invoke("RespawnFollowCamera", 1f);
            
        }
        

    }

    [PunRPC]
    void IncreasePrimarySkillDamage(int _damage)
    {
        primarySkillDamage = _damage;
    }

    void RespawnFollowCamera()
    {
        GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
    }
    void EnableRespawnedPlayerClients()
    {
        playerUI.SetActive(true);
        playerModel.SetActive(true);
        playerBaseRenderer.enabled = true;
    }
    #endregion
    #region Public Methods

    public void RuneActivated()
    {
        //photonView.RPC("RuneActivated_RPC", RpcTarget.All);
    }

    [PunRPC]
    void RuneActivated_RPC()
    {
        if (photonView.IsMine)
        {
            runeActivatedBlue.SetActive(false);
            runeActivatedBlue.SetActive(true);

        }
        else
        {
            runeActivatedRed.SetActive(false);
            runeActivatedRed.SetActive(true);
        }
    }
    public bool IsDead
    {
        get
        {
            return isDead;
        }
        set
        {
            isDead = value;
            if(IsDead)
            {
                ScoreManager.Instance.UpdateScore();
                if (photonView.IsMine)
                {
                    HUDManager.Instance.OnPlayerDeath();
                    ScoreManager.Instance.Score = 0;
                    GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = null;
                }
                else
                {
                    playerUI.SetActive(false);
                }

                m_Animator.SetTrigger("isDead");
                
                DisablePlayer();
            }
        }
    }

    public GameObject PrimarySkillSpawnLocation
    {
        set
        {
            primarySkillSpawnLocation = value;
        }
    }

    public GameObject PlayerModel
    {
        set
        {
            playerModel = value;
        }
    }

    public int PrimarySkillDamage
    {
        get
        {
            return primarySkillDamage;
        }
        set
        {
            primarySkillDamage = value;
            photonView.RPC("IncreasePrimarySkillDamage", RpcTarget.All, primarySkillDamage);
            Debug.Log(value);
        }
    }
    public float PrimarySkillCooldown
    {
        get
        {
            return primarySkillCooldown;
        }
        set
        {
            primarySkillCooldown = value;
            Debug.Log(value);
        }
    }

    public float SecondarySkillCooldown
    {
        get
        {
            return secondarySkillCooldown;
        }
        set
        {
            secondarySkillCooldown = value;
            Debug.Log(value);
        }
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
            Debug.Log(value);
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
            Debug.Log(value);
        }
    }

    public bool Poison
    {
        get
        {
            return isPoison;
        }
        set
        {
            isPoison = value;
            Debug.Log(value);
        }
    }

    public bool Bouncy
    {
        get
        {
            return isBouncy;
        }
        set
        {
            isBouncy = value;
            Debug.Log(value);
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
            Debug.Log(value);
        }
    }

    public bool MultiShot
    {
        get
        {
            return isMultiShot;
        }
        set
        {
            isMultiShot = value;
            Debug.Log(value);
        }
    }

    public bool FrostNova
    {
        get
        {
            return isFrostNova;
        }
        set
        {
            isFrostNova = value;
            Debug.Log(value);
        }
    }

    #endregion

}