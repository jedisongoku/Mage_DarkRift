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
    private float primarySkillRecharge;

    [Header("Player")]
    private GameObject playerModel;
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
    private bool isTransparent { get; set; }
    private bool isInvisible { get; set; }
    public bool canBeSeen { get; set; }
    private int bushCount { get; set; }


    [Header("Runes")]
    bool isFrostbite = false;
    bool isChill = false;
    bool isRage = false;
    bool isFrostNova = false;

    bool leftFirstTouch = false;
    bool rightFirstTouch = false;



    #region Private Methods
    private void Awake()
    {
        SetPlayerBaseStats();
        m_Animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        toonLitShader = Shader.Find("Toon/Lit");
        standardShader = Shader.Find("Standard");
        transparentShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (photonView.IsMine)
        {
            //playerMovementController.enabled = true;
            aimJoystick = HUDManager.Instance.AimJoystick;
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
            



        }
        else
        {
            primarySkillCharges.SetActive(false);
            playerHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(playerHealthBar.GetComponent<RectTransform>().sizeDelta.x, 0.3f);
            playerHealthBar.transform.Find("HealthBar").GetComponent<Image>().sprite = enemyHealthBarTexture;
            playerNameText.color = enemyNameColor;
            playerBase.startColor = enemyNameColor;
        }


        
        
    }

    // Update is called once per frame
    void Update()
    {

        if(photonView.IsMine && !isDead)
        {
            primarySkillCooldownTimer += Time.deltaTime;
            secondarySkillCooldownTimer += Time.deltaTime;
            HUDManager.Instance.SetSecondarySkillCooldownUI = secondarySkillCooldownTimer / PlayFabDataStore.playerBaseStats.SecondarySkillCooldown;

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
                        DrawAimLine();
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
                                GameObject closestEnemy = ClosestEnemy();
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
                        DrawAimLine();
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
                                GameObject closestEnemy = ClosestEnemy();
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
            if(Input.GetButtonDown("Dash"))
            {
                Debug.Log("DASHING");
                SecondarySkill();
            }
        }     
    }

    GameObject ClosestEnemy()
    {
        GameObject closestEnemy = null;
        float distance = 13;
        foreach (var enemy in PhotonNetwork.PhotonViews)
        {
            if (enemy.ViewID != photonView.ViewID && !enemy.GetComponent<PlayerCombatManager>().IsDead && LineOfSight(enemy.gameObject))
            {
                if (distance > (enemy.gameObject.transform.position - transform.position).magnitude)
                {
                    distance = (enemy.gameObject.transform.position - transform.position).magnitude;
                    closestEnemy = enemy.gameObject;
                }
            }

        }
        return closestEnemy;
    }

    void DrawAimLine()
    {

        var aimDistance = new Vector3(aimJoystick.Horizontal, 0, aimJoystick.Vertical);
        
        if(aimDistance.x > 0.1f || aimDistance.x < -0.1f || aimDistance.y > 0.1f || aimDistance.y < -0.1f)
        {
            aimDistance = new Vector3(aimJoystick.Horizontal, 0, aimJoystick.Vertical);
        }
        aimAssist.SetPosition(0, transform.position + Vector3.up);
        aimAssist.SetPosition(1, transform.position + Vector3.up + aimDistance * 10);
        aimLocation = transform.position + aimDistance * 13;
    }

    bool LineOfSight(GameObject enemy)
    {
        RaycastHit hit;
        var rayDirection = enemy.transform.position - transform.position;
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
            playerMovementController.SetFireDirection(_aimLocation);
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
        controller.SetParticleMoveDirection = new Vector3(direction.x, 0, direction.z);
        controller.DamageOrigin = this.gameObject;
        controller.PlayerViewID = photonView.ViewID;
        controller.Traveling = true;
    }

    public void ApplyDamageToEnemy(GameObject _enemy)
    {
        _enemy.GetComponent<PlayerHealthManager>().OnPlayerHit(photonView.ViewID, primarySkillDamage, Frostbite, Chill, FrostNova, Rage);
    }




    void SetPlayerBaseStats()
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
    }

    public void RespawnPlayer()
    {
        GetComponent<PlayerMovementController>().enabled = true;
        primarySkillCharges.SetActive(true);
        
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
            playerBase.gameObject.SetActive(false);
            Invoke("EnableRespawnedPlayerClients", 1f);
        }
        SetPlayerBaseStats();
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
        playerBase.gameObject.SetActive(true);
        BushManager.OnPlayerDeath();
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
                TurnOnNormalShader();
                Debug.Log("DEAD");
                DisablePlayer();
            }
        }
    }

    public void DisablePlayer()
    {
        if (photonView.IsMine)
        {
            playerMovementController.enabled = false;
            primarySkillCharges.SetActive(false);
            HUDManager.Instance.OnPlayerDeath();

            //ScoreManager.Instance.Score = 0;
            GetComponent<PlayerLevelManager>().ResetOnDeath();
            GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = null;
            aimAssist.SetPosition(1, aimAssist.GetPosition(0));
            aimJoystick.OnPointerUp(null);
        }
        else
        {

            playerUI.SetActive(false);

        }

        m_Animator.SetTrigger("Dead");
        GetComponent<CapsuleCollider>().enabled = false;
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
            //playerMeshRenderers = playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            //Debug.Log("Player has " + playerMeshRenderers.Length + " renderers");
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
    }

    void SwitchPlayerElements(bool value)
    {
        playerUI.SetActive(value);
        playerShadow.SetActive(value);
        playerBase.gameObject.SetActive(value);


    }

    private void OnTriggerEnter(Collider other)
    {
        if(photonView.IsMine && other.gameObject.layer == 15)
        {
            bushCount++;
            if (!isTransparent)
            {
                playerBase.gameObject.SetActive(false);
                isTransparent = true;
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                {
                    render.material.shader = transparentShader;
                }
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                {
                    render.material.shader = transparentShader;
                }
            }
        }
        else if (other.gameObject.layer == 15)
        {
            bushCount++;
            if (!isInvisible && !canBeSeen)
            {
                isInvisible = true;
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                {
                    render.gameObject.SetActive(false);
                    
                }
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                {
                    render.gameObject.SetActive(false);
                }
                SwitchPlayerElements(false);
            }
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if(!photonView.IsMine && canBeSeen && isInvisible)
        {
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                render.gameObject.SetActive(true);
                render.material.shader = transparentShader;
            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.gameObject.SetActive(true);
                render.material.shader = transparentShader;
            }
            playerUI.SetActive(true);
        }
        else if(!photonView.IsMine && !canBeSeen && isInvisible)
        {
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
            {
                render.gameObject.SetActive(false);
            }
            foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
            {
                render.gameObject.SetActive(false);
            }
            playerUI.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if(photonView.IsMine && other.gameObject.layer == 15)
        {
            bushCount--;
            if (isTransparent && bushCount == 0)
            {
                playerBase.gameObject.SetActive(true);
                
                isTransparent = false;
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                {
                    render.material.shader = standardShader;
                }
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                {
                    render.material.shader = standardShader;
                }
                
            }
        }
        else if(other.gameObject.layer == 15)
        {
            bushCount--;
            if (isInvisible && bushCount == 0)
            {
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().MeshRenderers)
                {
                    render.gameObject.SetActive(true);
                    render.material.shader = standardShader;
                }
                foreach (var render in playerModel.GetComponent<MeshRenderersInModel>().SkinnedMeshRenderers)
                {
                    render.gameObject.SetActive(true);
                    render.material.shader = standardShader;
                }
                isInvisible = false;
                canBeSeen = false;
                SwitchPlayerElements(true);
                
            }
        }


    }


    #endregion

}