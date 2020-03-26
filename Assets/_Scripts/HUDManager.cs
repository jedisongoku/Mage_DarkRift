using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class HUDManager : MonoBehaviourPunCallbacks
{
    public static HUDManager Instance;

    [Header("LaunchPanel")]
    [SerializeField] public GameObject launchPanel;
    [SerializeField] private Text launchLoadingText;
    [SerializeField] private Image launchLoadingBar;
    public bool isContenDownloaded { get; set; }

    [Header("Menu Panel")]
    [SerializeField] public GameObject menuPanel;
    [SerializeField] private Text playerName;
    [SerializeField] public GameObject characterLocation;

    [SerializeField] private Text coinsCurrencyText;
    [SerializeField] private Text gemsCurrencyText;
    [SerializeField] private Text energyCurrencyText;


    [Header("Waiting Area Panel")]
    public GameObject waitingAreaPanel;
    public Text roomStatusText;

    [Header("Loading Panel")]
    [SerializeField] public GameObject loadingPanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private Image loadingBar;

    [Header("Shop Panel")]
    [SerializeField] public GameObject shopPanel;

    [Header("Skin Panel")]
    [SerializeField] public GameObject skinPanel;

    [Header("Game Panel")]
    [SerializeField] private VariableJoystick movementJoystick;
    [SerializeField] private VariableJoystick aimJoystick;
    [SerializeField] public GameObject[] playerUIList;
    [SerializeField] public GameObject[] killFeed;
    public GameObject[] scoreboardItems;
    public GameObject gamePanel;
    public GameObject exitGameButton;
    public GameObject respawnButton;
    public Text respawnCooldownText;
    public GameObject playerControllerPanel;
    public GameObject deathPanel;
    public Image primarySkillCooldownImage;
    public Image secondarySkillCooldownImage;
    public GameObject scoreboardContent;
    public GameObject scoreboardPlayerPref;
    public GameObject runeSelection;
    public Button[] runeOptions;
    public Text fps;

    [Header("Level")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text xpText;
    [SerializeField] private Image xpProgressBar;
 
    private string gameMode = "Deathmatch";
    private bool isRespawnRequested = false;
    
    


    #region Unity Mono Calls
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60;
        
        //ActivatePanels(menuPanel.name);   
        StartCoroutine(Applaunch());
    }

    void Update()
    {
        fps.text = "fps: " + 1 / Time.deltaTime + " PING: " + PhotonNetwork.GetPing();
        
    }

    

    IEnumerator Applaunch()
    {
        if(isContenDownloaded)
        {
            switch (PhotonNetwork.NetworkClientState)
            {
                case ClientState.ConnectingToMasterServer:
                    launchLoadingText.text = "Connecting to server";
                    break;
                case ClientState.ConnectedToMasterServer:
                    launchLoadingBar.fillAmount += 0.02f;
                    launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
                    break;

                default:
                    launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
                    break;
            }
        }
        
        
        yield return new WaitForSeconds(0);

        if (launchLoadingBar.fillAmount < 1)
        {
            if(!isContenDownloaded)
            {
                launchLoadingBar.fillAmount += Time.deltaTime / 5;
            }
            else
            {
                launchLoadingBar.fillAmount += Time.deltaTime / 5;
                
            }
            launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
            StartCoroutine(Applaunch());
      
        }
        else
        {
            ActivatePanels(menuPanel.name);
            
            UpdateMenuPanel();
        }

    }

    void UpdateMenuPanel()
    {
        UpdatePlayerName();
        UpdateCurrencies();
    }

    void UpdatePlayerName()
    {
        playerName.text = PlayFabDataStore.playerName;
    }

    void UpdateCurrencies()
    {
        coinsCurrencyText.text = PlayFabDataStore.vc_coins.ToString();
        gemsCurrencyText.text = PlayFabDataStore.vc_gems.ToString();
        energyCurrencyText.text = PlayFabDataStore.vc_energy + "/50";
    }

    #endregion

    #region Public Calls

    public VariableJoystick MovementJoystick
    {
        get
        {
            return movementJoystick;
        }
    }
    public VariableJoystick AimJoystick
    {
        get
        {
            return aimJoystick;
        }
    }

    public void PrimarySkill()
    {
        //GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerCombatManager>().PrimarySkill();
    }

    public void SecondarySkill()
    {
        GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerCombatManager>().SecondarySkill();
    }

    public void DisplayRunes()
    {
        foreach(var listItem in runeOptions)
        {
            listItem.gameObject.SetActive(false);
        }
        var runeCount = PlayerRuneManager.playerRuneList.Count > 3 ? 3 : PlayerRuneManager.playerRuneList.Count;
        for (int i = 0; i < runeCount; i++)
        {
            runeOptions[i].GetComponentInChildren<Text>().text = PlayerRuneManager.playerRuneList[i].DisplayName;
            runeOptions[i].gameObject.SetActive(true);

        }
        runeSelection.SetActive(true);
    }

    public void SelectRune(int index)
    {
        PlayerRuneManager.Instance.SelectRune(index);
        runeSelection.SetActive(false);
    }

    public void OnPlayGameButtonClicked()
    {     
        if(PlayFabDataStore.vc_energy >= 5)
        {
            ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
            roomPropterties.Add("Level", gameMode);
            PhotonNetwork.JoinRandomRoom(roomPropterties, 0);
            loadingBar.fillAmount = 0;
            ActivatePanels(loadingPanel.name);
            StartCoroutine(GameSceneLoading());

            PlayFabApiCalls.instance.SubtractVirtualCurrency(5, "EN");
        }
        else
        {
            //ask user to refill energy
        }
        

    }

    public void OnShopButtonClicked()
    {
        ActivatePanels(shopPanel.name);
    }

    public void OnSkinButtonClicked()
    {
        ActivatePanels(skinPanel.name);
    }

    public void OnPanelBackButtonClicked()
    {
        ActivatePanels(menuPanel.name);
    }

    public void OnPlayerDeath()
    {
        playerControllerPanel.SetActive(false);
        deathPanel.SetActive(true);
        GameManager.Instance.CanRespawn = false;
        isRespawnRequested = false;
        respawnCooldownText.gameObject.SetActive(true);
        StartCoroutine(RespawnCooldown(GameManager.Instance.RespawnCooldown));
        //respawnButton.SetActive(true);
    }
    public void OnRespawnButtonClicked()
    {
        if(PlayFabDataStore.vc_energy >= 5)
        {
            if (GameManager.Instance.CanRespawn)
            {
                //OnGameLevelLoaded(); 
                GameManager.Instance.RespawnPlayer();
            }
            isRespawnRequested = true;
            playerControllerPanel.SetActive(true);
            deathPanel.SetActive(false);

            PlayFabApiCalls.instance.SubtractVirtualCurrency(5, "EN");
        }
        else
        {
            //ask user to refill
        }
        


    }

    IEnumerator RespawnCooldown(int _respawnCooldown)
    {
        respawnCooldownText.text = "RESPAWN IN " + _respawnCooldown;

        yield return new WaitForSeconds(1f);

        _respawnCooldown--;
        if (_respawnCooldown > 0 )
        {
            
            StartCoroutine(RespawnCooldown(_respawnCooldown));
        }
        else
        {
            GameManager.Instance.CanRespawn = true;
            respawnCooldownText.gameObject.SetActive(false);
            if (isRespawnRequested)
                OnRespawnButtonClicked();

        }
    }

    public void OnGameLevelLoaded()
    {
        //ActivatePanels(loadingPanel.name);
        Debug.Log("Game Level Loaded");
        loadingBar.fillAmount = 0.95f;
        deathPanel.SetActive(false);
        playerControllerPanel.SetActive(true);
        //StartCoroutine(GameSceneLoading());

    }

    IEnumerator GameSceneLoading()
    {
        if(loadingBar.fillAmount <= 0.85f || loadingBar.fillAmount >= 0.95f)
        {
            loadingBar.fillAmount += Time.deltaTime / 2;
        }
        loadingText.text = Mathf.RoundToInt(loadingBar.fillAmount * 100) + "%";

        yield return new WaitForSeconds(0);
        if(loadingBar.fillAmount < 1)
        {
            StartCoroutine(GameSceneLoading());
        }
        else
        {
            GameManager.Instance.InitializePlayer();
            Invoke("ActivateGamePanel", 0.2f);
        }
    }

    public void OnExitButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        loadingBar.fillAmount = 0;
        ActivatePanels(loadingPanel.name);
        StartCoroutine(LobbySceneLoading());
        //characterLocation.SetActive(true);
        //Invoke("SetCanvasCamera", 1);
    }

    IEnumerator LobbySceneLoading()
    {
        loadingBar.fillAmount += Time.deltaTime * 2;
        loadingText.text = Mathf.RoundToInt(loadingBar.fillAmount * 100) + "%";

        yield return new WaitForSeconds(0);
        if (loadingBar.fillAmount < 1)
        {
            StartCoroutine(LobbySceneLoading());
        }
        else
        {
            ActivatePanels(menuPanel.name);
        }
    }

    void SetCanvasCamera()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public GameObject[] ScoreboardItems
    {
        get
        {
            return scoreboardItems;
        }
    }

    public float SetPrimarySkillCooldownUI
    {
        set
        {
            primarySkillCooldownImage.fillAmount = value;
        }
    }

    public float SetSecondarySkillCooldownUI
    {
        set
        {
            secondarySkillCooldownImage.fillAmount = value;
        }
    }

    public void UpdateScoreBoard()
    {
        
    }
    #endregion

    #region Private Calls

    void ActivateGamePanel()
    {
        ActivatePanels(gamePanel.name);
    }
    void ActivatePanels(string panelToBeActivated)
    {
        launchPanel.SetActive(panelToBeActivated.Equals(launchPanel.name));
        menuPanel.SetActive(panelToBeActivated.Equals(menuPanel.name));
        shopPanel.SetActive(panelToBeActivated.Equals(shopPanel.name));
        skinPanel.SetActive(panelToBeActivated.Equals(skinPanel.name));
        waitingAreaPanel.SetActive(panelToBeActivated.Equals(waitingAreaPanel.name));
        loadingPanel.SetActive(panelToBeActivated.Equals(loadingPanel.name));
        gamePanel.SetActive(panelToBeActivated.Equals(gamePanel.name));
        if(panelToBeActivated.Equals(menuPanel.name))
        {
            characterLocation.SetActive(true);
        }

    }

    public void UpdatePlayerLevel(int _level, int _currentXP, int _maxXP)
    {
        levelText.text = _level.ToString();
        xpText.text = _currentXP + "/" + _maxXP;
        xpProgressBar.fillAmount = (float)_currentXP / (float)_maxXP;
    }

    public void StartAppLaunch()
    {
        Debug.Log("Disconnect on HUD");
        launchLoadingBar.fillAmount = 0;
        ActivatePanels(launchPanel.name);
        StartCoroutine(Applaunch());
    }
    #endregion

    #region Ad
    #endregion
}
