using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    List<ScorePlayer> scoreboardPlayers = new List<ScorePlayer>();

    [Header("LaunchPanel")]
    [SerializeField] public GameObject launchPanel;
    [SerializeField] private Text launchLoadingText;
    [SerializeField] private Image launchLoadingBar;

    [Header("Menu Panel")]
    [SerializeField] public GameObject menuPanel;
    [SerializeField] private Text playerName;


    [Header("Waiting Area Panel")]
    public GameObject waitingAreaPanel;
    public Text roomStatusText;

    [Header("Loading Panel")]
    public GameObject loadingPanel;

    [Header("Game Panel")]
    public GameObject gamePanel;
    public GameObject respawnButton;
    public Text respawnTimerText;
    public GameObject runeSelection;
    public Image primarySkillCooldownImage;
    public Image secondarySkillCooldownImage;
    public Button secondarySkillButton;
    public GameObject[] scoreboardList;
    public GameObject[] runeSelectionList;
    public VariableJoystick joystick;

    public bool isWaiting { get; private set; }



    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        DontDestroyOnLoad(this);
        StartCoroutine(Applaunch());
    }

    void Update()
    {
        //Debug.Log(PhotonNetwork.NetworkClientState);
    }


    IEnumerator Applaunch()
    {
        switch(PhotonNetwork.NetworkClientState)
        {
            case ClientState.ConnectingToNameServer:
                //launchLoadingBar.fillAmount += 0.01f;
                break;
            case ClientState.ConnectedToNameServer:
                launchLoadingBar.fillAmount += 0.01f;
                break;
            case ClientState.ConnectingToMasterServer:
                //launchLoadingBar.fillAmount += 0.01f;
                break;
            case ClientState.Authenticating:
                launchLoadingBar.fillAmount += 0.01f;
                break;
            case ClientState.ConnectedToMasterServer:
                launchLoadingBar.fillAmount += 0.01f;
                break;
            case ClientState.JoiningLobby:
                //launchLoadingBar.fillAmount += 0.01f;
                break;
            case ClientState.JoinedLobby:
                launchLoadingBar.fillAmount += 0.01f;
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(0);

        if(launchLoadingBar.fillAmount < 1)
        {
            launchLoadingBar.fillAmount += Time.deltaTime / 10;
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

    public void OnPlayGameButtonClicked()
    {

        ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
        Debug.Log("GAME MODE " + PersistData.instance.GameData.GameMode);
        roomPropterties.Add("Level", PersistData.instance.GameData.GameMode);
        PhotonNetwork.JoinRandomRoom(roomPropterties, 0);
        ActivatePanels(waitingAreaPanel.name);

        isWaiting = true;
    }

    public void OnExitButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        ActivatePanels(menuPanel.name);
    }

    void UpdateWaitingRoomState()
    {
        Debug.Log("UpdateRoomStatus");
        if(isWaiting)
        {
            roomStatusText.text = "Players found " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        }
    }














    public void PlayGame()
    {
        SceneManager.LoadScene("ShowdownDark");
        OnGameLevelLoaded();
    }

    public void OnGameLevelLoaded()
    {

        ActivatePanels(loadingPanel.name);
        Invoke("EnableGameHUD", 0.5f);
    }

    void EnableGameHUD()
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
    }

    public void DisplayRunes(string[] runeNames)
    {
        foreach(var listItem in runeSelectionList)
        {
            listItem.SetActive(false);
        }

        for (int i = 0; i < runeNames.Length; i++)
        {
            
            runeSelectionList[i].GetComponentInChildren<Text>().text = runeNames[i];
            runeSelectionList[i].SetActive(true);
        }
        runeSelection.SetActive(true);
    }

    public void SelectRune(int index)
    {
        Debug.LogWarning("SELECT RUNE");
        ClientManager.Instance.localPlayer.GetComponent<PlayerRuneManager>().SendSelectRuneMessage((ushort)index);
        runeSelection.SetActive(false);
    }

    public void OnPlayerDeath()
    {
        respawnButton.SetActive(true);
        respawnTimerText.gameObject.SetActive(true);
    }
    public void OnRespawnButtonClicked()
    {
        //OnGameLevelLoaded();
        respawnButton.SetActive(false);
        ClientManager.Instance.localPlayer.GetComponent<Player>().SendRespawnMessage();
    }

    public void LoadingOnRespawn()
    {
        OnGameLevelLoaded();
    }

    public void SetRespawnTimer(int _timer)
    {
        respawnTimerText.text = "Respawn in " + _timer;
        if (_timer <= 0) respawnTimerText.gameObject.SetActive(false);
    }

    public void RefreshScoreboard()
    {
        scoreboardPlayers.Clear();
        foreach(var player in ScoreManager.Instance.scoreBoard)
        {
            scoreboardPlayers.Add(player.Value);
        }

        ScorePlayer[] scoreboardSorted = scoreboardPlayers.OrderByDescending(Player => Player.score).ToArray();

        for (int i = 0; i < scoreboardList.Length; i++)
        {
            if(i < scoreboardSorted.Length)
            {
                scoreboardList[i].transform.Find("ScoreboardPlayerNameText").GetComponent<Text>().text = scoreboardSorted[i].name;
                scoreboardList[i].transform.Find("ScoreboardPlayerScoreText").GetComponent<Text>().text = scoreboardSorted[i].score.ToString();
                scoreboardList[i].SetActive(true);
            }
            else
            {
                scoreboardList[i].SetActive(false);
            }
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

    

    public void DisableDash(bool _value)
    {
        secondarySkillButton.interactable = !_value;
    }
}
