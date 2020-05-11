using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using PlayFab.ClientModels;

public class HUDManager : MonoBehaviourPunCallbacks
{
    public static HUDManager Instance;

    [Header("LaunchPanel")]
    [SerializeField] public GameObject launchPanel;
    [SerializeField] private Text launchLoadingText;
    [SerializeField] private Image launchLoadingBar;
    public bool isContentDownloaded { get; set; }
    public bool isLoginSuccess { get; set; }

    [Header("Menu Panel")]
    [SerializeField] public GameObject menuPanel;
    [SerializeField] private Text playerName;
    [SerializeField] public GameObject characterLocation;

    [SerializeField] private Text menuCoinsCurrencyText;
    [SerializeField] private Text menuGemsCurrencyText;
    [SerializeField] private Text menuEnergyCurrencyText;


    [Header("Waiting Area Panel")]
    public GameObject waitingAreaPanel;
    public Text roomStatusText;

    [Header("Loading Panel")]
    [SerializeField] public GameObject loadingPanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private Image loadingBar;

    [Header("Runes Panel")]
    [SerializeField] public GameObject runesPanel;

    [Header("Shop Panel")]
    [SerializeField] public GameObject shopPanel;

    [SerializeField] private Text shopCoinsCurrencyText;
    [SerializeField] private Text shopGemsCurrencyText;
    [SerializeField] private Text shopEnergyCurrencyText;
    [SerializeField] private Text remainingGemAdText;
    [SerializeField] private Text remainingCoinAdText;
    [SerializeField] private GameObject energyAnimation;
    [SerializeField] private GameObject gemAnimation;
    [SerializeField] private GameObject coinAnimation;
    [SerializeField] private GameObject refillWithAdIcon;
    [SerializeField] private GameObject refillWithGemIcon;
    [SerializeField] private GameObject buyGem30;
    [SerializeField] private GameObject buyGem80;
    [SerializeField] private GameObject buyGem170;
    [SerializeField] private GameObject buyGem360;
    [SerializeField] private GameObject buyGem950;
    [SerializeField] private GameObject buyGem2500;
    [SerializeField] private GameObject dailyGem;
    [SerializeField] private GameObject dailyCoin;


    [Header("Skin Panel")]
    [SerializeField] public GameObject skinPanel;
    [SerializeField] public Button buyWithCoinButton;
    [SerializeField] public Button buyWithGemButton;
    [SerializeField] public Button selectSkinButton;

    [SerializeField] private Text skinCoinsCurrencyText;
    [SerializeField] private Text skinGemsCurrencyText;
    [SerializeField] private Text skinEnergyCurrencyText;

    [Header("Leaderboard Panel")]
    [SerializeField] public GameObject leaderboardPanel;
    [SerializeField] public GameObject leaderboardContent;

    [Header("Profile Panel")]
    [SerializeField] public GameObject profilePanel;
    [SerializeField] public Text gamesPlayedText;
    [SerializeField] public Text totalKillsText;
    [SerializeField] public Text totalDeathsText;
    [SerializeField] public Text killStreakText;
    [SerializeField] public Text kdText;
    [SerializeField] public Text rankText;
    [SerializeField] private Text profilePlayerName;
    [SerializeField] private Image connectGameCenterImage;
    [SerializeField] private Sprite googlePlaySprite;
    [SerializeField] private Sprite gameCenterSprite;
    [SerializeField] private Text gameCenterConnectText;

    [Header("Game Panel")]
    [SerializeField] private DynamicJoystick movementJoystick;
    [SerializeField] private FloatingJoystick aimJoystick;
    [SerializeField] public GameObject[] playerUIList;
    [SerializeField] public GameObject[] killFeed;
    [SerializeField] public GameObject runeInfoPanel;
    [SerializeField] public Text totalKillsScoreText;
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
    public GameObject coinReward;
    public Text fps;
    public Text respawnEnergyText;
    public Text totalPlayersText;
    public Text continueGemText;
    public Text continueGemCostText;
    public Button continueButton;
    private float continueGemMultiplier = 1.5f;
    private int continueGemCost = 2;
    public bool runeSelected;


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

        Application.targetFrameRate = 9999;
        QualitySettings.vSyncCount = 0;
        

        //ActivatePanels(menuPanel.name);   
        StartCoroutine(Applaunch());
    }

    void Update()
    {
        fps.text = PhotonNetwork.GetPing() + "ms";
        
    }

    

    IEnumerator Applaunch()
    {
        if(isContentDownloaded)
        {
            switch (PhotonNetwork.NetworkClientState)
            {
                case ClientState.ConnectingToMasterServer:
                    launchLoadingText.text = "Downloading Content";
                    break;
                case ClientState.ConnectedToMasterServer:
                    launchLoadingBar.fillAmount += 0.3f;
                    launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
                    break;

                default:
                    launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
                    break;
            }
        }
        else if(!isLoginSuccess)
        {
            launchLoadingBar.fillAmount += Time.deltaTime / 25;
            launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
        }
        
        
        yield return new WaitForSeconds(0);

        if (launchLoadingBar.fillAmount < 1)
        {
            if(!isContentDownloaded)
            {
                if (isLoginSuccess)
                {
                    launchLoadingBar.fillAmount += Time.deltaTime;
                }
            }
            else
            {
                launchLoadingBar.fillAmount += Time.deltaTime / 10;
            }
            launchLoadingText.text = Mathf.RoundToInt(launchLoadingBar.fillAmount * 100) + "%";
            StartCoroutine(Applaunch());
      
        }
        else
        {
            ActivatePanels(menuPanel.name);
            
            UpdateMenuPanel();
            //SoundManager.Instance.SwitchSound(true);
        }

    }

    void UpdateMenuPanel()
    {
        UpdatePlayerName();
        UpdateCurrencies();
    }

    public void UpdatePlayerName()
    {
        playerName.text = PlayFabDataStore.playerProfile.playerName;
    }

    public void UpdateCurrencies()
    {
        Debug.Log("Updated Currencies");
        menuCoinsCurrencyText.text = PlayFabDataStore.vc_coins.ToString();
        menuGemsCurrencyText.text = PlayFabDataStore.vc_gems.ToString();
        menuEnergyCurrencyText.text = PlayFabDataStore.vc_energy + "/50";

        shopCoinsCurrencyText.text = PlayFabDataStore.vc_coins.ToString();
        shopGemsCurrencyText.text = PlayFabDataStore.vc_gems.ToString();
        shopEnergyCurrencyText.text = PlayFabDataStore.vc_energy + "/50";

        skinCoinsCurrencyText.text = PlayFabDataStore.vc_coins.ToString();
        skinGemsCurrencyText.text = PlayFabDataStore.vc_gems.ToString();
        skinEnergyCurrencyText.text = PlayFabDataStore.vc_energy + "/50";
    }

    public void RefillEnergyWithGems()
    {
        if(PlayFabDataStore.vc_gems >= 5 && PlayFabDataStore.vc_energy < 50)
        {
            PlayEnergyAnimation(1, PlayFabDataStore.vc_energy, 50 - PlayFabDataStore.vc_energy);
            PlayFabApiCalls.instance.SubtractVirtualCurrency(5, "GM");
            PlayFabApiCalls.instance.AddVirtualCurrency(50 - PlayFabDataStore.vc_energy, "EN");
            
        }
    }

    #endregion

    #region Public Calls

    public DynamicJoystick MovementJoystick
    {
        get
        {
            return movementJoystick;
        }
    }
    public FloatingJoystick AimJoystick
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
            runeOptions[i].transform.Find("RuneImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("RuneTextures/" + PlayerRuneManager.playerRuneList[i].DisplayName);
            runeOptions[i].gameObject.SetActive(true);

        }
        runeSelection.SetActive(true);
    }

    public void SelectRune(int index)
    {
        PlayerRuneManager.Instance.SelectRune(index);
        runeSelection.SetActive(false);
    }

    public void ShowRuneInfo(string title, string description)
    {
        
        runeInfoPanel.transform.Find("RuneName").GetComponent<Text>().text = title;
        runeInfoPanel.transform.Find("RuneDescription").GetComponent<Text>().text = description;
        runeInfoPanel.GetComponent<Animator>().SetTrigger("Info");
    }

    public void OnPlayGameButtonClicked()
    {
        PlayFabDataStore.gameMode = "Deathmatch";

        if (PlayFabDataStore.vc_energy >= 5)
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
            ActivatePanels(shopPanel.name);
        }
        

    }

    public void OnShopButtonClicked()
    {
        UpdateRemainingDailyAds();
        ActivatePanels(shopPanel.name);

    }

    public void UpdateRemainingDailyAds()
    {
        remainingCoinAdText.text = PlayFabDataStore.vc_adCoin.ToString();
        remainingGemAdText.text = PlayFabDataStore.vc_adGem.ToString();
    }

    public void OnSkinButtonClicked()
    {
        ActivatePanels(skinPanel.name);
        OnSkinPreviewed(true, PlayFabDataStore.playerProfile.skinName);
    }

    public void OnRunesButtonClicked()
    {
        ActivatePanels(runesPanel.name);
    }

    public void OnLeaderboardButtonClicked()
    {
        ActivatePanels(leaderboardPanel.name);
    }

    public void OnPanelBackButtonClicked()
    {
        ActivatePanels(menuPanel.name);
    }

    public void OnProfileButtonClicked()
    {
        PlayFabApiCalls.instance.GetPlayerStatistics();
        if (PlayFabDataStore.gameCenterLinked)
            gameCenterConnectText.text = "CONNECTED";
        else
            gameCenterConnectText.text = "CONNECT";

        ActivatePanels(profilePanel.name);
        profilePlayerName.text = PlayFabDataStore.playerProfile.playerName;
#if UNITY_ANDROID
        connectGameCenterImage.sprite = googlePlaySprite;
#else
        connectGameCenterImage.sprite = gameCenterSprite;
#endif
    }

    public void RefreshProfileStatistics()
    {
        gamesPlayedText.text = PlayFabDataStore.playerStatistics["Games Played"].ToString();
        totalKillsText.text = PlayFabDataStore.playerStatistics["Total Kills"].ToString();
        totalDeathsText.text = PlayFabDataStore.playerStatistics["Total Deaths"].ToString();
        killStreakText.text = PlayFabDataStore.playerStatistics["Kill Streak"].ToString();
        if (PlayFabDataStore.playerStatistics["Total Kills"] > 0)
            kdText.text = (PlayFabDataStore.playerStatistics["Total Kills"] / PlayFabDataStore.playerStatistics["Total Deaths"]).ToString();
        else kdText.text = "0";
    }

    public void OnGameCenterConnectButtonClicked()
    {
        if(!PlayFabDataStore.gameCenterLinked)
            PlayFabApiCalls.instance.LinkGameAccount();
    }

    public void OnSkinPreviewed(bool isOwned, string name)
    {
        MenuSkinController.instance.ShowcaseSkin(name);

        Debug.Log("isowned " + isOwned);
        Debug.Log("name " + name);
        bool buyWithCoin = PlayFabDataStore.gameSkinCatalog[name].currencyType == "CO" ? true : false;
        Debug.Log("coin " + buyWithCoin);

        if (name == PlayFabDataStore.playerProfile.skinName)
        {
            buyWithCoinButton.interactable = false;
            buyWithGemButton.interactable = false;
        }
        else
        {
            buyWithCoinButton.interactable = true;
            buyWithGemButton.interactable = true;

        }

        if (buyWithCoin)
        {
            buyWithCoinButton.transform.Find("Buy").gameObject.SetActive(!isOwned);
            buyWithCoinButton.transform.Find("Select").gameObject.SetActive(isOwned);

            buyWithCoinButton.transform.Find("Buy/CostText").GetComponent<Text>().text = PlayFabDataStore.gameSkinCatalog[name].cost.ToString();

            buyWithCoinButton.onClick.RemoveAllListeners();
            if(isOwned) buyWithCoinButton.onClick.AddListener(MakeSkinActive);
            else buyWithCoinButton.onClick.AddListener(BuySkin);


        }
        else
        {
            buyWithGemButton.transform.Find("Buy").gameObject.SetActive(!isOwned);
            buyWithGemButton.transform.Find("Select").gameObject.SetActive(isOwned);

            buyWithGemButton.transform.Find("Buy/CostText").GetComponent<Text>().text = PlayFabDataStore.gameSkinCatalog[name].cost.ToString();

            buyWithGemButton.onClick.RemoveAllListeners();
            if (isOwned) buyWithGemButton.onClick.AddListener(MakeSkinActive);
            else buyWithGemButton.onClick.AddListener(BuySkin);
        }

        Debug.Log(buyWithCoinButton.gameObject.name);
        buyWithCoinButton.gameObject.SetActive(buyWithCoin);
        buyWithGemButton.gameObject.SetActive(!buyWithCoin);


        

    }

    public void SavePlayerName()
    {
        //update playername playfab
        if(profilePlayerName.GetComponentInParent<InputField>().text.Length > 0)
        {
            PlayFabDataStore.playerProfile.playerName = profilePlayerName.GetComponentInParent<InputField>().text;
            playerName.text = PlayFabDataStore.playerProfile.playerName;
            PhotonNetwork.LocalPlayer.NickName = PlayFabDataStore.playerProfile.playerName;
            PlayFabApiCalls.instance.UpdateProfile();
        }
        
    }

    public void MakeSkinActive()
    {
        PlayFabDataStore.playerProfile.skinName = MenuSkinController.instance.activeSkin.name;
        //Update on playfab
        PlayFabApiCalls.instance.UpdateProfile();

        buyWithCoinButton.interactable = false;
        buyWithGemButton.interactable = false;


    }

    public void BuySkin()
    {
        Debug.Log(MenuSkinController.instance.activeSkin.name);
        PlayFabApiCalls.instance.PurchaseItem(PlayFabDataStore.gameSkinCatalog[MenuSkinController.instance.activeSkin.name].itemID, PlayFabDataStore.gameSkinCatalog[MenuSkinController.instance.activeSkin.name].cost,
            PlayFabDataStore.gameSkinCatalog[MenuSkinController.instance.activeSkin.name].currencyType);
    }

    public void InsufficientFunds_VC()
    {
        ActivatePanels(shopPanel.name);
    }

    public void On1v1Clicked()
    {
        PlayFabDataStore.gameMode = "Duel";

        ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
        roomPropterties.Add("Level", PlayFabDataStore.gameMode);
        PhotonNetwork.JoinRandomRoom(roomPropterties, 0);
        loadingBar.fillAmount = 0;
        ActivatePanels(loadingPanel.name);
        StartCoroutine(GameSceneLoading());
    }

    public void OnPlayerDeath()
    {
        playerControllerPanel.SetActive(false);
        respawnEnergyText.text = PlayFabDataStore.vc_energy + "/" + 50;

        if(PlayFabDataStore.vc_gems < continueGemCost)
        {
            continueButton.interactable = false;
        }
        else
        {
            continueButton.interactable = true;
        }

        continueGemText.text = PlayFabDataStore.vc_gems.ToString();
        continueGemCostText.text = "x" + continueGemCost;

        deathPanel.SetActive(true);
        

        GameManager.Instance.CanRespawn = false;
        isRespawnRequested = false;
        respawnCooldownText.gameObject.SetActive(true);
        StartCoroutine(RespawnCooldown(GameManager.Instance.RespawnCooldown));
        runeSelection.SetActive(false);

        coinReward.SetActive(false);
        if (GameManager.playerKillCount > 0)
        {
            coinReward.transform.Find("CoinText").GetComponent<Text>().text = GameManager.Instance.GetRewardAmount().ToString();
            coinReward.SetActive(true);

            PlayFabApiCalls.instance.UpdateStatistics("Lifetime Kills", GameManager.playerKillCount);
            PlayFabApiCalls.instance.UpdateStatistics("Kill Streak", GameManager.playerKillCount);
            PlayFabApiCalls.instance.AddVirtualCurrency(GameManager.Instance.GetRewardAmount(), "CO");
            //Make api calls
        }
        //respawnButton.SetActive(true);
    }

    public void OnContinueButtonClicked()
    {
        if (PlayFabDataStore.vc_gems >= continueGemCost)
        {
            GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerCombatManager>().SecondChance = true;
            
            if (GameManager.Instance.CanRespawn)
            {
                //OnGameLevelLoaded(); 
                GameManager.Instance.RespawnPlayer();
                playerControllerPanel.SetActive(true);
            }
            isRespawnRequested = true;
            //playerControllerPanel.SetActive(true);
            deathPanel.SetActive(false);

            PlayFabApiCalls.instance.SubtractVirtualCurrency(continueGemCost, "GM");
            continueGemCost = Mathf.CeilToInt(continueGemCost * continueGemMultiplier);
            Debug.Log("continue gem cost " + continueGemCost);
        }

    }
    public void OnRespawnButtonClicked()
    {
        Debug.Log("Energy " + PlayFabDataStore.vc_energy);

        if (PlayFabDataStore.vc_energy >= 5)
        {
            if (GameManager.Instance.CanRespawn)
            {
                //OnGameLevelLoaded(); 
                GameManager.Instance.RespawnPlayer();
                playerControllerPanel.SetActive(true);
            }
            isRespawnRequested = true;
            //playerControllerPanel.SetActive(true);
            deathPanel.SetActive(false);

            PlayFabApiCalls.instance.SubtractVirtualCurrency(5, "EN");
        }
        else
        {
            //ask user to refill
            Debug.Log("Watch ads");
            UnityAds.instance.RefillEnergyAd();
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

    public void ResetContinueGemCost()
    {
        Debug.Log("Continue Gem Reset");
        continueGemCost = 2;
    }

    public void ShowDeathPanel()
    {
        respawnEnergyText.text = "50/50";
        deathPanel.SetActive(true);
    }

    public void OnGameLevelLoaded()
    {
        //ActivatePanels(loadingPanel.name);
        Debug.Log("Game Level Loaded");
        loadingBar.fillAmount = 0.95f;
        runesPanel.SetActive(false);
        deathPanel.SetActive(false);
        playerControllerPanel.SetActive(true);
        foreach (var item in scoreboardItems)
            item.SetActive(false);
        UpdateTotalPlayerCount();


    }

    public void UpdateTotalPlayerCount()
    {
        int totalPlayers = GameManager.Instance.botPlayerCount + PhotonNetwork.CurrentRoom.PlayerCount > 8 ? 8 : GameManager.Instance.botPlayerCount + PhotonNetwork.CurrentRoom.PlayerCount;
        totalPlayersText.text = totalPlayers + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        //Debug.Log("Total Views " + PhotonNetwork.PhotonViews.Length);
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
            SoundManager.Instance.SwitchSound(false);
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
        runesPanel.SetActive(panelToBeActivated.Equals(runesPanel.name));
        profilePanel.SetActive(panelToBeActivated.Equals(profilePanel.name));
        menuPanel.SetActive(panelToBeActivated.Equals(menuPanel.name));
        shopPanel.SetActive(panelToBeActivated.Equals(shopPanel.name));
        leaderboardPanel.SetActive(panelToBeActivated.Equals(leaderboardPanel.name));
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

    public void RuneUISelection()
    {
        runeSelected = true;
        Invoke("DelayedRuneUIRelease", 0.1f);
    }

    void DelayedRuneUIRelease()
    {
        runeSelected = false;
    }

    public void EnergyCurrencyTextUpdate(int amount)
    {
        shopEnergyCurrencyText.text = amount + "/50";
    }

    public void GemCurrencyTextUpdate(int amount)
    {
        shopGemsCurrencyText.text = amount.ToString();
    }

    public void CoinCurrencyTextUpdate(int amount)
    {
        shopCoinsCurrencyText.text = amount.ToString();
    }
    public void PlayEnergyAnimation(int id, int currentAmount, int amountAdded)
    {
        switch(id)
        {
            case 0:
                energyAnimation.transform.position = refillWithAdIcon.transform.position;
                energyAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 1:
                energyAnimation.transform.position = refillWithGemIcon.transform.position;
                energyAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
        }
        energyAnimation.SetActive(true);
    }

    public void PlayGemAnimation(int id, int currentAmount, int amountAdded)
    {
        switch (id)
        {
            case 0:
                gemAnimation.transform.position = buyGem30.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 1:
                gemAnimation.transform.position = buyGem80.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 2:
                gemAnimation.transform.position = buyGem170.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 3:
                gemAnimation.transform.position = buyGem360.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 4:
                gemAnimation.transform.position = buyGem950.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 5:
                gemAnimation.transform.position = buyGem2500.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
            case 6:
                gemAnimation.transform.position = dailyGem.transform.position;
                gemAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;
        }
        gemAnimation.SetActive(true);
    }

    public void PlayCoinAnimation(int id, int currentAmount, int amountAdded)
    {
        switch (id)
        {
            case 0:
                coinAnimation.transform.position = dailyCoin.transform.position;
                coinAnimation.GetComponent<CurrencyAnimation>().SetAmount(currentAmount, amountAdded);
                break;

        }
        coinAnimation.SetActive(true);
    }

    public void UpdateTotalKillsScoreText(int score)
    {
        totalKillsScoreText.text = score + "/20";
    }
    #endregion
}
