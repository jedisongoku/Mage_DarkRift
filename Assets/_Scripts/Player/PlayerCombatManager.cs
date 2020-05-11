using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerCombatManager : MonoBehaviourPun
{
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private PlayerHealthManager playerHealthManager;
    [SerializeField] private CapsuleCollider playerDamageCollider;
    public string killFeedName;
    public int totalKills = 0;
    private FloatingJoystick aimJoystick;
  
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
    private float primarySkillRecharge;

    [Header("Player")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private ParticleSystem playerBase;
    [SerializeField] private GameObject playerShadow;
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
    private Vector3 primarySkillSpawnLocationOffset = new Vector3(0.5f, 0.5f, 0);
    private float primarSkillchargeTimer_1;

    [Header("Bush Materials")]
    [SerializeField] private SkinnedMeshRenderer[] playerMeshRenderers;
    private Shader toonLitShader;
    private Shader standardShader;
    private Shader transparentShader;
    public bool isTransparent { get; set; }
    public bool isInvisible { get; set; }
    public bool canBeSeen { get; set; }
    public bool isSearchable { get; set; }
    private int bushCount { get; set; }


    public bool isInBush { get; set; }


    [Header("Runes")]
    bool isFrostbite = false;
    bool isChill = false;
    bool isRage = false;
    bool isFrostNova = false;
    bool isSecondChance = false;

    GameObject closestEnemy;
    public bool isPlayer = true;



    #region Private Methods
    private void Awake()
    {
        
        m_Animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        toonLitShader = Shader.Find("Toon/Lit");
        standardShader = Shader.Find("Standard");
        transparentShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (photonView.IsMine && isPlayer)
        {
            //playerMovementController.enabled = true;
            aimJoystick = HUDManager.Instance.AimJoystick;
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
            killFeedName = PhotonNetwork.LocalPlayer.NickName;
            //GetComponent<CapsuleCollider>().radius = 0.75f;
            GetComponent<PlayerLevelManager>().RegisterOnKill();

        }
        else
        {
            primarySkillCharges.SetActive(false);
            playerHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(playerHealthBar.GetComponent<RectTransform>().sizeDelta.x, 0.3f);
            playerHealthBar.transform.Find("HealthBar").GetComponent<Image>().sprite = enemyHealthBarTexture;
            playerNameText.color = enemyNameColor;
            playerBase.startColor = enemyNameColor;
            if(isPlayer)
                killFeedName = PhotonNetwork.GetPhotonView(photonView.ViewID).Owner.NickName;
            else
                playerModel.GetComponent<MeshRenderersInModel>().AddAnimationRendererUpdate();
        }

        ScoreManager.Instance.StartScoreboard(killFeedName);
        SetPlayerBaseStats();
        


    }

    // Update is called once per frame
    void Update()
    {

        if(photonView.IsMine && !isDead && isPlayer)
        {
            primarySkillCooldownTimer += Time.deltaTime;
            secondarySkillCooldownTimer += Time.deltaTime;
            HUDManager.Instance.SetSecondarySkillCooldownUI = secondarySkillCooldownTimer / PlayFabDataStore.playerBaseStats.SecondarySkillCooldown;

            //TOUCH 0
            if(Input.touchCount > 0)
            {
                
                if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    if (Input.GetTouch(0).position.x > Screen.width / 2)
                    {
                        //Right side of the screen
                        DrawAimLine();
                    }

                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended && !HUDManager.Instance.runeSelected)
                {
                    if (Input.GetTouch(0).position.x > Screen.width / 2)
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
                                closestEnemy = ClosestEnemy(closestEnemy);
                                if (closestEnemy != null)
                                {
                                    PrimarySkill(closestEnemy.transform.position);
                                }
                                else
                                {
                                    PrimarySkill(transform.position + transform.forward);
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
                    if (Input.GetTouch(1).position.x > Screen.width / 2)
                    {
                        //Right side of the screen
                        DrawAimLine();
                    }
                }
                else if (Input.GetTouch(1).phase == TouchPhase.Ended && !HUDManager.Instance.runeSelected)
                {
                    if (Input.GetTouch(1).position.x > Screen.width / 2)
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
                                closestEnemy = ClosestEnemy(closestEnemy);
                                if (closestEnemy != null)
                                {
                                    PrimarySkill(closestEnemy.transform.position);
                                }
                                else
                                {
                                    PrimarySkill(transform.position + transform.forward);
                                }
                            }

                            aimAssist.SetPosition(1, aimAssist.GetPosition(0));
                        }
                    }
                }
            }
            
            if(Input.GetButtonDown("Fire1") && !Application.isMobilePlatform)
            {
                if (primarySkillCooldownTimer >= primarySkillCooldown && primarySkillCharge > 0)
                {
                    closestEnemy = ClosestEnemy(closestEnemy);
                    if (closestEnemy != null)
                    {
                        
                        PrimarySkill(closestEnemy.transform.position);
                    }
                    else
                    {
                        PrimarySkill(transform.position + transform.forward);
                    }
                }
                
            }
            if(Input.GetButtonDown("Dash"))
            {
                //Debug.Log("DASHING");
                SecondarySkill();
            }
        } 
        else if(!isPlayer)
        {
            primarySkillCooldownTimer += Time.deltaTime;
            secondarySkillCooldownTimer += Time.deltaTime;
        }

    }

    GameObject ClosestEnemy(GameObject targetEnemy)
    {
        GameObject closestEnemy = null;
        float distance = 12;
        
        /*
        if (targetEnemy != null)
        {
            if (!targetEnemy.GetComponent<PlayerCombatManager>().IsDead && LineOfSight(targetEnemy) &&
                targetEnemy.GetComponent<PlayerCombatManager>().isSearchable && distance > Vector3.Distance(targetEnemy.transform.position, transform.position))
            {
                //Debug.Log("Attack same enemy");
                return targetEnemy;
            }
        }*/
        //Debug.Log("Attack different enemy");

        foreach (var enemy in PhotonNetwork.PhotonViews)
        {
            if(enemy.gameObject.layer == 8)
            {
                if (enemy.ViewID != photonView.ViewID && !enemy.GetComponent<PlayerCombatManager>().IsDead && LineOfSight(enemy.gameObject) &&
                enemy.GetComponent<PlayerCombatManager>().isSearchable)
                {
                    if (distance > Vector3.Distance(enemy.gameObject.transform.position, transform.position))
                    {
                        distance = Vector3.Distance(enemy.gameObject.transform.position, transform.position);
                        closestEnemy = enemy.gameObject;
                    }
                }
            }
            
        }
        return closestEnemy;
    }

    void DrawAimLine()
    {

        var aimDirection = new Vector3(aimJoystick.Horizontal, 0, aimJoystick.Vertical);
        
        if(aimDirection.x > 0.1f || aimDirection.x < -0.1f || aimDirection.y > 0.1f || aimDirection.y < -0.1f)
        {
            aimDirection = new Vector3(aimJoystick.Horizontal, 0, aimJoystick.Vertical);
            float distance = Vector3.Distance(transform.position, transform.position + aimDirection);
            distance = 1 / distance;
            aimDirection *= distance * 10;
        }
        aimAssist.SetPosition(0, transform.position + Vector3.up);
        aimAssist.SetPosition(1, transform.position + Vector3.up + aimDirection);
        aimLocation = transform.position + aimDirection;
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

    IEnumerator PrimarySkillChargerReady(int _chargeID)
    {
        primarySkillUICharges[_chargeID - 1].SetActive(false);
        if(_chargeID == PlayFabDataStore.playerBaseStats.PrimarySkillCharge)
        {
            primarSkillchargeTimer_1 = 0;
        }
        else
        {
            primarySkillUIChargeProgressBars[_chargeID].fillAmount = 0;
        }

        primarySkillUIChargeProgressBars[_chargeID - 1].fillAmount = primarSkillchargeTimer_1 / primarySkillRecharge;

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(PrimarySkillCharger(_chargeID));
    }

    IEnumerator PrimarySkillCharger(int _chargeID)
    {
        primarSkillchargeTimer_1 += Time.deltaTime;

        primarySkillUIChargeProgressBars[_chargeID - 1].fillAmount = primarSkillchargeTimer_1 / primarySkillRecharge;

        yield return new WaitForSeconds(0);
        if(primarSkillchargeTimer_1 < primarySkillRecharge)
        {
            StartCoroutine(PrimarySkillCharger(_chargeID));
        }
        else
        {
            primarySkillUICharges[_chargeID - 1].SetActive(true);
            primarySkillCharge++;
            if(primarySkillCharge < PlayFabDataStore.playerBaseStats.PrimarySkillCharge)
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
            //playerMovementController.SetFireDirection(_aimLocation);
            primarySkillCooldownTimer = 0f;
            StopAllCoroutines();
            StartCoroutine(PrimarySkillChargerReady(primarySkillCharge));
            primarySkillCharge--;
            photonView.RPC("UsePrimarySkill", RpcTarget.AllViaServer, _aimLocation, transform.position + transform.forward / 2 + transform.up, playerHealthManager.Rage);
        }
    }

    public void SecondarySkill()
    {
        if (secondarySkillCooldownTimer >= secondarySkillCooldown)
        {
            secondarySkillCooldownTimer = 0f;
            photonView.RPC("UseSecondarySkill", RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    void UseSecondarySkill()
    {
        if(!IsDead)
            m_Animator.SetTrigger("Dashing");
        if(!isInBush)
        {
            dashTrail.SetActive(false);
            dashTrail.SetActive(true);
        }
        
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
        Vector3 heading = aimLocation - transform.position + transform.forward / 2 + transform.up;
        Vector3 direction = heading / heading.magnitude;

        yield return new WaitForSeconds(_delayTime);

        GameObject obj = ObjectPooler.Instance.GetPrimarySkillPrefab();
        //if (obj == null) StopCoroutine(PrimarySkill());
        obj.transform.position = transform.position + transform.forward / 2 + transform.up;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        PrimarySkillController controller = obj.GetComponent<PrimarySkillController>();

        //Where stats applied to the particle object
        controller.DamageOrigin = this.gameObject;
        controller.PlayerViewID = photonView.ViewID;
        //controller.SetParticleDestination = aimLocation;
        controller.SetParticleMoveDirection = new Vector3(direction.x, 0, direction.z);
        controller.Traveling = true;
    }

    public void ApplyDamageToEnemy(GameObject _enemy)
    {
        _enemy.GetComponent<PlayerHealthManager>().OnPlayerHit(photonView.ViewID, primarySkillDamage, Frostbite, Chill, FrostNova, Rage);
    }




    void SetPlayerBaseStats()
    {
        if(!isSecondChance)
        {
            primarySkillDamage = PlayFabDataStore.playerBaseStats.PrimarySkillDamage;
            primarySkillCooldown = PlayFabDataStore.playerBaseStats.PrimarySkillCooldown;
            primarySkillCharge = PlayFabDataStore.playerBaseStats.PrimarySkillCharge;
            primarySkillRecharge = PlayFabDataStore.playerBaseStats.PrimarySkillRecharge;
            secondarySkillCooldown = PlayFabDataStore.playerBaseStats.SecondarySkillCooldown;
            isFrostbite = false;
            isChill = false;
            isRage = false;
            isFrostNova = false;


            if (photonView.IsMine && isPlayer) HUDManager.Instance.ResetContinueGemCost();
        }

        isSecondChance = false;
        isSearchable = true;

        //Games played statistic added
        if(isPlayer)
            PlayFabApiCalls.instance.UpdateStatistics("Games Played", 1);

        secondarySkillCooldownTimer = secondarySkillCooldown;
    }

    public void RespawnPlayer()
    {
        if(isPlayer)
        {
            GetComponent<PlayerMovementController>().enabled = true;
            primarySkillCharges.SetActive(true);

            if (!isSecondChance)
            {
                GetComponent<PlayerLevelManager>().ResetOnRespawn();
                PlayerRuneManager.Instance.RestartPlayerRunes();
            }
        }
        else
        {
            GetComponent<PlayerLevelManager>().ResetOnRespawn();
        }
        


        photonView.RPC("RespawnClients", RpcTarget.All, GameManager.Instance.SpawnLocationIndex);

    }

    [PunRPC]
    void RespawnClients(int _spawnLocationIndex)
    {
        if(!photonView.IsMine || !isPlayer)
        {
            playerUI.SetActive(false);
            playerModel.SetActive(false);
            playerBase.gameObject.SetActive(false);
            Invoke("EnableRespawnedPlayerClients", 0.5f);
        }
        
        SetPlayerBaseStats();

        transform.position = GameManager.Instance.SpawnLocation(_spawnLocationIndex);
        m_Animator.SetTrigger("Respawn");
        GetComponent<CapsuleCollider>().enabled = true;
        playerDamageCollider.enabled = true;
        playerHealthManager.RespawnPlayer();
        IsDead = false;
        playerMovementController.enabled = true;
        bushCount = 0;

        if (photonView.IsMine && isPlayer)
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
        playerBase.gameObject.SetActive(true);
        if (!isPlayer)
        {
            GetComponent<PlayerAIController>().RespawnBotPlayer();
        }
    }
    #endregion
    #region Public Methods

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
                TurnOnNormalShader();
                DisablePlayer();
                //Deaths statistic added
                if (isPlayer)
                {
                    PlayFabApiCalls.instance.UpdateStatistics("Deaths", 1);
                    PlayFabApiCalls.instance.UpdateStatistics("Max Level", GetComponent<PlayerLevelManager>().GetPlayerLevel());
                }
                //Max Level statistic added

                if (!isPlayer)
                {
                    GetComponent<PlayerAIController>().OnBotPlayerDeath();
                    if(photonView.IsMine)
                    {
                        if (PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount - GameManager.Instance.botPlayerCount >= 0)
                        {
                            Invoke("RespawnBotPlayer", GameManager.Instance.RespawnCooldown);
                        }
                        else
                        {
                            GameManager.Instance.botPlayerCount--;
                            PhotonNetwork.Destroy(photonView);
                            HUDManager.Instance.UpdateTotalPlayerCount();
                        }
                    }
                    
                    
                }
            }
        }
    }

    public void DisablePlayer()
    {
        if (photonView.IsMine && isPlayer)
        {
            playerMovementController.enabled = false;
            primarySkillCharges.SetActive(false);
            HUDManager.Instance.OnPlayerDeath();

            //ScoreManager.Instance.Score = 0;
            
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = null;
            aimAssist.SetPosition(1, aimAssist.GetPosition(0));
            aimJoystick.OnPointerUp(null);
        }
        else
        {
            if (!isPlayer) GetComponent<PlayerAIController>().OnDeath();
            playerUI.SetActive(false);

        }

        m_Animator.SetTrigger("Dead");
        GetComponent<CapsuleCollider>().enabled = false;
        playerDamageCollider.enabled = false;
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
            playerModel.GetComponent<MeshRenderersInModel>().AddAnimationRendererUpdate();
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
        primarySkillDamage = Mathf.RoundToInt(PlayFabDataStore.playerBaseStats.PrimarySkillDamage * PlayFabDataStore.playerBaseStats.DamageBoostMultiplier);
    }

    public float PrimarySkillRecharge
    {
        get
        {
            return primarySkillRecharge;
        }
        set
        {
            primarySkillRecharge = value;
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
            if(isFrostbite) photonView.RPC("Frostbite_RPC", RpcTarget.AllBuffered);
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

    public bool SecondChance
    {
        get
        {
            return isSecondChance;
        }
        set
        {
            isSecondChance = value;
            if (isSecondChance) photonView.RPC("SecondChance_RPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void SecondChance_RPC()
    {
        isSecondChance = true;
        playerHealthManager.isSecondChance = true;
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
        isFrostNova = true;
    }

    void TurnOnNormalShader()
    {
        playerBase.gameObject.SetActive(true);
        isInvisible = false;
        isTransparent = false;
        canBeSeen = false;
        bushCount = 0;
        foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
        {
            render.enabled = true;
            render.material.shader = standardShader;
        }
        foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
        {
            render.enabled = true;
            render.material.shader = standardShader;
        }

        playerModel.SetActive(true);
    }

    void SwitchPlayerElements(bool value)
    {
        playerUI.SetActive(value);
        playerShadow.SetActive(value);
        
        if (!photonView.IsMine || !isPlayer)
        {
            //dashTrail.SetActive(value);
            playerHealthManager.SwitchShieldVisibility(value);
            playerHealthManager.SwitchRageVisibility(value);
            playerHealthManager.SwitchStrongHearthVisibility(value);
            playerHealthManager.SwitchFrostbiteVisibility(value);
            playerMovementController.SwitchChillVisibility(value);
            playerBase.gameObject.SetActive(value);
        }


    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 15)
        {
            if (photonView.IsMine && isInBush && isPlayer)
            {
                bushCount++;
                if (!isTransparent)
                {
                    isTransparent = true;
                    isSearchable = false;
                    foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                    {
                        if (render.transform.name.Equals("Halo"))
                        {
                            render.enabled = false;
                        }
                        else
                        {
                            render.material.shader = transparentShader;
                        }

                    }
                    foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                    {
                        render.material.shader = transparentShader;
                    }
                }
            }
            else if (isInBush)
            {
                bushCount++;
                if (!isInvisible && !canBeSeen)
                {
                    isInvisible = true;
                    isSearchable = false;
                    playerModel.SetActive(false);
                    SwitchPlayerElements(false);
                }
            }
        }
        

    }*/

    public void PlayerCanBeSeen()
    {
        canBeSeen = true;
        isSearchable = true;
        Debug.Log("Player can be seen");
        ApplyBushChanges();

    }

    public void PlayerIsInvisible()
    {
        canBeSeen = false;
        isSearchable = false;
        Debug.Log("Player is invisible");
        ApplyBushChanges();
    }
    public void BushEntered()
    {
        if (photonView.IsMine && isPlayer)
        {
            isInBush = true;
            isTransparent = true;
            isSearchable = false;
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                if (render.transform.name.Equals("Halo"))
                {
                    render.enabled = false;
                }
                else
                {
                    render.material.shader = transparentShader;
                }

            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.material.shader = transparentShader;
            }
        }
        else
        {
            isInBush = true;
            isInvisible = true;
            isSearchable = false;
            playerModel.SetActive(false);
            SwitchPlayerElements(false);
        }
    }

    public void BushExited()
    {
        if (photonView.IsMine && isPlayer)
        {
            isInBush = false;
            isTransparent = false;
            isSearchable = true;
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                if (render.transform.name.Equals("Halo"))
                {
                    render.enabled = true;
                }
                else
                {
                    render.material.shader = standardShader;
                }

            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.material.shader = standardShader;
            }

        }
        else
        {
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                if (render.transform.name.Equals("Halo"))
                {
                    render.enabled = true;
                }
                else
                {
                    render.material.shader = standardShader;
                }

            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.material.shader = standardShader;
            }

            isInvisible = false;
            canBeSeen = false;
            isSearchable = true;
            isInBush = false;
            playerModel.SetActive(true);
            SwitchPlayerElements(true);
        }
    }

    void ApplyBushChanges()
    {
        if (!photonView.IsMine && canBeSeen && isInvisible && isPlayer)
        {
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                if (render.transform.name.Equals("Halo"))
                {
                    render.enabled = false;
                }
                else
                {
                    render.material.shader = transparentShader;
                }

            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.material.shader = transparentShader;
            }
            playerUI.SetActive(true);
            playerModel.SetActive(true);
        }
        else if (!photonView.IsMine && !canBeSeen && isInvisible)
        {
            playerModel.SetActive(false);
            playerUI.SetActive(false);
        }
        else if (photonView.IsMine && canBeSeen && isInvisible && !isPlayer)
        {
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                if (render.transform.name.Equals("Halo"))
                {
                    render.enabled = false;
                }
                else
                {
                    render.material.shader = transparentShader;
                }

            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.material.shader = transparentShader;
            }
            playerUI.SetActive(true);
            playerModel.SetActive(true);
        }
        else if (photonView.IsMine && !canBeSeen && isInvisible && !isPlayer)
        {
            playerModel.SetActive(false);
            playerUI.SetActive(false);
        }
    }
    /*
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 15)
        {
            if (photonView.IsMine && isInBush && isPlayer)
            {
                bushCount--;
                if (isTransparent && bushCount == 0)
                {
                    isTransparent = false;
                    isSearchable = true;
                    foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                    {
                        if (render.transform.name.Equals("Halo"))
                        {
                            render.enabled = true;
                        }
                        else
                        {
                            render.material.shader = standardShader;
                        }

                    }
                    foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                    {
                        render.material.shader = standardShader;
                    }

                    //playerBase.gameObject.SetActive(true);
                    isInBush = false;

                }

            }
            else if (isInBush)
            {
                bushCount--;
                if (isInvisible && bushCount == 0)
                {
                    foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                    {
                        if (render.transform.name.Equals("Halo"))
                        {
                            render.enabled = true;
                        }
                        else
                        {
                            render.material.shader = standardShader;
                        }

                    }
                    foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                    {
                        render.material.shader = standardShader;
                    }
                    
                    isInvisible = false;
                    canBeSeen = false;
                    isSearchable = true;
                    isInBush = false;
                    playerModel.SetActive(true);
                    SwitchPlayerElements(true);
                    

                }
            }
        }
    }
    */

    #endregion

    #region BotPlayer

    public void BotPlayerAutoAttack(GameObject targetPlayer)
    {
        
        if(targetPlayer != null && !isDead && LineOfSight(targetPlayer))
        {
            PrimarySkill(targetPlayer.transform.position);
        }
        
    }

    public void BotPlayerDash()
    {
        if(!IsDead)
            SecondarySkill();
    }

    public int BotPlayerPrimarySkillCharge()
    {
        return primarySkillCharge;
    }

    public bool BotPlayerSecondarySkillAvailable()
    {
        return secondarySkillCooldownTimer >= secondarySkillCooldown;
    }

    void RespawnBotPlayer()
    {
        RespawnPlayer();
    }


    public void UpdateTotalKills()
    {
        photonView.RPC("UpdateTotalKills_RPC", RpcTarget.Others, totalKills);
    }

    [PunRPC]
    void UpdateTotalKills_RPC(int _totalKills)
    {
        totalKills = _totalKills;
    }
    #endregion

}