﻿using System.Collections;
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

    [Header("Menu Panel")]
    [SerializeField] public GameObject menuPanel;
    [SerializeField] private Text playerName;
    [SerializeField] private GameObject characterLocation;


    [Header("Waiting Area Panel")]
    public GameObject waitingAreaPanel;
    public Text roomStatusText;

    [Header("Loading Panel")]
    [SerializeField] public GameObject loadingPanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private Image loadingBar;

    [Header("Game Panel")]
    [SerializeField] private VariableJoystick movementJoystick;
    [SerializeField] private VariableJoystick aimJoystick;
    [SerializeField] public GameObject[] playerUIList;
    public GameObject[] scoreboardItems;
    public GameObject gamePanel;
    public GameObject exitGameButton;
    public GameObject respawnButton;
    public GameObject poisonShopPanel;
    public Image primarySkillCooldownImage;
    public Image secondarySkillCooldownImage;
    public GameObject scoreboardContent;
    public GameObject scoreboardPlayerPref;
    public GameObject runeSelection;
    public Button[] runeOptions;
    public Text fps;

    private string gameMode = "Deathmatch";


    #region Unity Mono Calls
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        DontDestroyOnLoad(this);
        Instance = this;
        //ActivatePanels(menuPanel.name);
        if (Application.isMobilePlatform)
        {
            movementJoystick.gameObject.SetActive(true);
        }
        else
        {
            movementJoystick.gameObject.SetActive(true);
        }
        StartCoroutine(Applaunch());
    }

    void Update()
    {
        fps.text = "fps: " + 1 / Time.deltaTime + " PING: " + PhotonNetwork.GetPing();
        
    }

    

    IEnumerator Applaunch()
    {
        switch (PhotonNetwork.NetworkClientState)
        {
            case ClientState.ConnectedToMasterServer:
                launchLoadingBar.fillAmount += 0.01f;
                break;

            default:
                break;
        }

        yield return new WaitForSeconds(0);

        if (launchLoadingBar.fillAmount < 1)
        {
            launchLoadingBar.fillAmount += Time.deltaTime / 25;
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
    }

    void UpdatePlayerName()
    {
        playerName.text = PersistData.instance.GameData.PlayerName;
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
        for(int i = 0; i < 3; i++)
        {
            runeOptions[i].GetComponentInChildren<Text>().text = PlayerRuneManager.playerRuneList[i].DisplayName;
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
        ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
        roomPropterties.Add("Level", gameMode);
        PhotonNetwork.JoinRandomRoom(roomPropterties, 0);
        ActivatePanels(loadingPanel.name);
        StartCoroutine(GameSceneLoading());

    }

    public void OnPlayerDeath()
    {
        respawnButton.SetActive(true);
    }
    public void OnRespawnButtonClicked()
    {
        OnGameLevelLoaded();
        respawnButton.SetActive(false);
        GameManager.Instance.RespawnPlayer();
    }

    public void OnGameLevelLoaded()
    {
        //ActivatePanels(loadingPanel.name);
        loadingBar.fillAmount = 0.75f;
        //StartCoroutine(GameSceneLoading());
    }

    IEnumerator GameSceneLoading()
    {
        loadingBar.fillAmount += Time.deltaTime / 5;
        loadingText.text = Mathf.RoundToInt(loadingBar.fillAmount * 100) + "%";

        yield return new WaitForSeconds(0);
        if(loadingBar.fillAmount < 1)
        {
            StartCoroutine(GameSceneLoading());
        }
        else
        {
            GameManager.Instance.InitializePlayer();
            ActivatePanels(gamePanel.name);
            ScoreManager.Instance.StartScoreboard();
        }
    }

    public void OnExitButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        ActivatePanels(menuPanel.name);
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
        waitingAreaPanel.SetActive(panelToBeActivated.Equals(waitingAreaPanel.name));
        loadingPanel.SetActive(panelToBeActivated.Equals(loadingPanel.name));
        gamePanel.SetActive(panelToBeActivated.Equals(gamePanel.name));
        if(panelToBeActivated.Equals(menuPanel.name))
        {
            characterLocation.SetActive(true);
        }

    }
    #endregion
}
