﻿using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerCombatManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private PlayerHealthManager playerHealthManager;
    private VariableJoystick aimJoystick;
  
    Animator m_Animator;
    bool isDead = false;
    Vector3 aimLocation = Vector3.zero;
    Vector3 spawnPosition = Vector3.zero;

    [Header("Player Skills Stats")]

    private float primarySkillCooldown;
    private float secondarySkillCooldown;
    private int primarySkillCharge;
    private GameObject primarySkillSpawnLocation;
    private int primarySkillDamage = 0;
    private float primarySkillCooldownTimer = 3f;
    private float secondarySkillCooldownTimer = 8f;
    private int primarySkillParticleCount = 5;

    [Header("Player")]
    private GameObject playerModel;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private MeshRenderer playerBaseRenderer;
    [SerializeField] private Material playerBaseColor;
    [SerializeField] private GameObject playerHealthBar;
    [SerializeField] private Sprite enemyHealthBarTexture;
    [SerializeField] private GameObject primarySkillCharges;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Color32 enemyNameColor;

    //[SerializeField] private GameObject runeActivatedBlue;
    //[SerializeField] private GameObject runeActivatedRed;
    [SerializeField] private GameObject dashTrail;
    [SerializeField] private LineRenderer aimAssist;
    public bool canShoot { get; set; }

    [Header("Primary Skill")]
    [SerializeField] public GameObject[] primarySkillUICharges;
    [SerializeField] public Image[] primarySkillUIChargeProgressBars;
    private float primarSkillchargeTimer_1;
    private float primarSkillchargeTimer_2;
    private float primarSkillchargeTimer_3;


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
            aimJoystick = HUDManager.Instance.AimJoystick;
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
            playerBaseRenderer.material = playerBaseColor;
            


        }
        else
        {
            primarySkillCharges.SetActive(false);
            playerHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(playerHealthBar.GetComponent<RectTransform>().sizeDelta.x, 0.3f);
            playerHealthBar.transform.Find("HealthBar").GetComponent<Image>().sprite = enemyHealthBarTexture;
            playerNameText.color = enemyNameColor;
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
            HUDManager.Instance.SetSecondarySkillCooldownUI = secondarySkillCooldownTimer / PlayerBaseStats.Instance.SecondarySkillCooldown;

            //TOUCH 0
            if(Input.touchCount > 0)
            {
                
                if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    if (Input.GetTouch(0).position.x < Screen.width / 2)
                    {
                        //Left side of the screen 
                    }
                    else
                    {
                        //Right side of the screen
                        var aimDistance = new Vector3(aimJoystick.Horizontal, 0, aimJoystick.Vertical);
                        aimAssist.SetPosition(0, transform.position + Vector3.up);
                        aimAssist.SetPosition(1, transform.position + Vector3.up + aimDistance * 10);
                        aimLocation = transform.position + aimDistance * 10;
                    }

                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    if (Input.GetTouch(0).position.x < Screen.width / 2)
                    {
                        //Left side of the screen 
                    }
                    else
                    {
                        //Right side of the screen
                        if (canShoot)
                        {
                            canShoot = false;
                            if ((aimAssist.GetPosition(0) - aimAssist.GetPosition(1)).magnitude > 1)
                            {
                                PrimarySkill(aimLocation);
                            }
                            else
                            {
                                GameObject closestEnemy = null;
                                float distance = 10;
                                foreach (var enemy in PhotonNetwork.PhotonViews)
                                {
                                    if (enemy.ViewID != photonView.ViewID)
                                    {
                                        if (distance > (enemy.gameObject.transform.position - transform.position).magnitude)
                                        {
                                            distance = (enemy.gameObject.transform.position - transform.position).magnitude;
                                            closestEnemy = enemy.gameObject;
                                        }
                                    }

                                }
                                if (closestEnemy != null)
                                {
                                    PrimarySkill(closestEnemy.transform.position);
                                }
                            }

                            aimAssist.SetPosition(1, aimAssist.GetPosition(0));
                        }
                        
                        
                        
                    }

                }
            }
            
            //TOUCH 1
            if(Input.touchCount > 1)
            {
                if (Input.GetTouch(1).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Stationary)
                {
                    if (Input.GetTouch(1).position.x < Screen.width / 2)
                    {
                        //Left side of the screen 
                    }
                    else
                    {
                        //Right side of the screen
                        var aimDistance = new Vector3(aimJoystick.Horizontal, 0, aimJoystick.Vertical).normalized;
                        aimAssist.SetPosition(0, transform.position + Vector3.up);
                        aimAssist.SetPosition(1, transform.position + Vector3.up + aimDistance * 10);
                        aimLocation = transform.position + aimDistance * 10;
                    }

                }
                else if (Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    if (Input.GetTouch(1).position.x < Screen.width / 2)
                    {
                        //Left side of the screen 
                    }
                    else
                    {
                        //Right side of the screen
                        if (canShoot)
                        {
                            canShoot = false;
                            if ((aimAssist.GetPosition(0) - aimAssist.GetPosition(1)).magnitude > 1)
                            {
                                PrimarySkill(aimLocation);
                            }
                            else
                            {
                                GameObject closestEnemy = null;
                                float distance = 10;
                                foreach (var enemy in PhotonNetwork.PhotonViews)
                                {
                                    if (enemy.ViewID != photonView.ViewID)
                                    {
                                        if (distance > (enemy.gameObject.transform.position - transform.position).magnitude)
                                        {
                                            distance = (enemy.gameObject.transform.position - transform.position).magnitude;
                                            closestEnemy = enemy.gameObject;
                                        }
                                    }

                                }
                                if (closestEnemy != null)
                                {
                                    PrimarySkill(closestEnemy.transform.position);
                                }
                            }

                            aimAssist.SetPosition(1, aimAssist.GetPosition(0));
                        }
                        
                        /*
                        aimAssist.SetPosition(1, aimAssist.GetPosition(0));
                        //Get Player Fire Direction
                        PrimarySkill(aimLocation);*/
                    }

                }
            }
            
            if(Input.GetButtonDown("Fire1") && !Application.isMobilePlatform)
            {
                GameObject closestEnemy = null;
                float distance = 1000;
                foreach (var enemy in PhotonNetwork.PhotonViews)
                {
                    if (enemy.ViewID != photonView.ViewID)
                    {
                        if (distance > (enemy.gameObject.transform.position - transform.position).magnitude)
                        {
                            distance = (enemy.gameObject.transform.position - transform.position).magnitude;
                            closestEnemy = enemy.gameObject;
                        }
                    }

                }
                if (closestEnemy != null)
                {
                    PrimarySkill(closestEnemy.transform.position);
                }
            }
            if(Input.GetButtonDown("Dash"))
            {
                Debug.Log("DASHING");
                SecondarySkill();
            }
        }     
    }

    IEnumerator PrimarySkillChargerReady(int _chargeID)
    {
        primarySkillUICharges[_chargeID - 1].SetActive(false);
        if(_chargeID == PlayerBaseStats.Instance.PrimarySkillCharge)
        {
            primarSkillchargeTimer_1 = 0;
        }
        else
        {
            primarySkillUIChargeProgressBars[_chargeID].fillAmount = 0;
        }

        primarySkillUIChargeProgressBars[_chargeID - 1].fillAmount = primarSkillchargeTimer_1 / PlayerBaseStats.Instance.PrimarySkillRecharge;

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(PrimarySkillCharger(_chargeID));
    }

    IEnumerator PrimarySkillCharger(int _chargeID)
    {
        primarSkillchargeTimer_1 += Time.deltaTime;

        primarySkillUIChargeProgressBars[_chargeID - 1].fillAmount = primarSkillchargeTimer_1 / PlayerBaseStats.Instance.PrimarySkillRecharge;

        yield return new WaitForSeconds(0);
        if(primarSkillchargeTimer_1 < PlayerBaseStats.Instance.PrimarySkillRecharge)
        {
            StartCoroutine(PrimarySkillCharger(_chargeID));
        }
        else
        {
            primarySkillUICharges[_chargeID - 1].SetActive(true);
            primarySkillCharge++;
            if(primarySkillCharge < PlayerBaseStats.Instance.PrimarySkillCharge)
            {
                primarySkillUIChargeProgressBars[_chargeID].fillAmount = 0;
                primarSkillchargeTimer_1 = 0;
                StartCoroutine(PrimarySkillCharger(_chargeID + 1));
            }
        }
    }





    public void PrimarySkill(Vector3 _aimLocation)
    {
        if (primarySkillCooldownTimer >= primarySkillCooldown && primarySkillCharge > 0)
        {
            playerMovementController.SetFireDirection(_aimLocation);
            primarySkillCooldownTimer = 0f;
            StopAllCoroutines();
            StartCoroutine(PrimarySkillChargerReady(primarySkillCharge));
            primarySkillCharge--;
            photonView.RPC("UsePrimarySkill", RpcTarget.AllViaServer, _aimLocation, primarySkillSpawnLocation.transform.position, playerHealthManager.Rage);
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
    void UsePrimarySkill(Vector3 _aimLocation, Vector3 _spawnPosition, bool _isRage)
    {       
        playerMovementController.AimLocation = _aimLocation;
        playerMovementController.Attack = true;
        spawnPosition = _spawnPosition;
        aimLocation = _aimLocation;
        isRage = _isRage;
        StartCoroutine(PrimarySkill(0f));
    }
    IEnumerator PrimarySkill(float _delayTime)
    {
        m_Animator.SetTrigger("Attacking");
        Vector3 heading = aimLocation - primarySkillSpawnLocation.transform.position;
        Vector3 direction = heading / heading.magnitude;

        yield return new WaitForSeconds(_delayTime);

        GameObject obj = ObjectPooler.Instance.GetPrimarySkillPrefab();
        //if (obj == null) StopCoroutine(PrimarySkill());
        obj.transform.position = primarySkillSpawnLocation.transform.position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        PrimarySkillController controller = obj.GetComponent<PrimarySkillController>();
        //Where stats applied to the particle object
        controller.SetParticleMoveDirection = new Vector3(direction.x, 0, direction.z);
        controller.DamageOrigin = this.gameObject;
        controller.PlayerViewID = photonView.ViewID;
        controller.Traveling = true;


        /*
        obj.GetComponent<PrimarySkillController>().PlayerOrigin = photonView.ViewID;
        obj.GetComponent<PrimarySkillController>().DamageDone = primarySkillDamage;
        if (isFrostbite) obj.GetComponent<PrimarySkillController>().Frostbite = isFrostbite;
        if (isChill) obj.GetComponent<PrimarySkillController>().Chill = isChill;
        if (isRage) obj.GetComponent<PrimarySkillController>().Rage = isRage;
        if (isFrostNova) obj.GetComponent<PrimarySkillController>().FrostNova = isFrostNova;
        obj.GetComponent<PrimarySkillController>().Traveling = true;*/


        //GameObject particle = Instantiate(primarySkillPrefab, primarySkillSpawnLocation.transform.position, Quaternion.identity) as GameObject;
    }

    public void ApplyDamageToEnemy(GameObject _enemy)
    {
        _enemy.GetComponent<PlayerHealthManager>().OnPlayerHit(photonView.ViewID, primarySkillDamage, Frostbite, Chill, FrostNova, Rage);
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
        primarySkillCharge = PlayerBaseStats.Instance.PrimarySkillCharge;
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
        GetComponent<PlayerMovementController>().enabled = true;
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
        else
        {

        }
        transform.position = GameManager.Instance.SpawnLocation(_spawnLocationIndex);
        m_Animator.SetTrigger("Respawn");
        GetComponent<CapsuleCollider>().enabled = true;
        playerHealthManager.RespawnPlayer();
        IsDead = false;
        playerMovementController.enabled = true;

        if (photonView.IsMine)
        {
            
            Invoke("RespawnFollowCamera", 0.1f);
            
        }
        

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
    /*
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
    }*/
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
                    aimAssist.SetPosition(1, aimAssist.GetPosition(0));
                    aimJoystick.OnPointerUp(null);
                }
                else
                {
                    playerUI.SetActive(false);
                }

                m_Animator.SetTrigger("Dead");
                Debug.Log("DEAD");
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
            photonView.RPC("PrimarySkillDamage_RPC", RpcTarget.AllBuffered, primarySkillDamage);
            Debug.Log(value);
        }
    }

    [PunRPC]
    void PrimarySkillDamage_RPC(int _damage)
    {
        primarySkillDamage = Mathf.RoundToInt(PlayerBaseStats.Instance.PrimarySkillDamage * PlayerBaseStats.Instance.DamageBoostMultiplier);
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
            if(isFrostbite) photonView.RPC("Frostbite_RPC", RpcTarget.AllBuffered);
            Debug.Log(value);
        }
    }

    [PunRPC]
    void Frostbite_RPC()
    {
        isFrostbite = true;
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
            if (isChill) photonView.RPC("Chill_RPC", RpcTarget.AllBuffered);
            Debug.Log(value);
        }
    }

    [PunRPC]
    void Chill_RPC()
    {
        isChill = true;
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
            if (isRage) photonView.RPC("Rage_RPC", RpcTarget.AllBuffered);
            Debug.Log(value);
        }
    }

    [PunRPC]
    void Rage_RPC()
    {
        isRage = true;
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
            if (isMultiShot) photonView.RPC("MultiShot_RPC", RpcTarget.AllBuffered);
            Debug.Log(value);
        }
    }

    [PunRPC]
    void MultiShot_RPC()
    {
        isMultiShot = true;
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
            if (isFrostNova) photonView.RPC("FrostNova_RPC", RpcTarget.AllBuffered);
            Debug.Log(value);
        }
    }

    [PunRPC]
    void FrostNova_RPC()
    {
        isMultiShot = true;
    }

    #endregion

}