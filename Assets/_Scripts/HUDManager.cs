using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    List<ScorePlayer> scoreboardPlayers = new List<ScorePlayer>();

    [Header("Menu Panel")]
    [SerializeField]
    public GameObject menuPanel;

    [Header("Waiting Area Panel")]
    public GameObject waitingAreaPanel;

    [Header("Loading Panel")]
    public GameObject loadingPanel;

    [Header("Game Panel")]
    public GameObject gamePanel;
    public GameObject respawnButton;
    public GameObject runeSelection;
    public Image primarySkillCooldownImage;
    public Image secondarySkillCooldownImage;
    public GameObject[] scoreboardList;
    public GameObject[] runeSelectionList;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 200;
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("PoisonShop");
        OnGameLevelLoaded();
    }

    public void OnGameLevelLoaded()
    {
        ActivatePanels(loadingPanel.name);
        Invoke("EnableGameHUD", 2f);
    }

    void EnableGameHUD()
    {
        ActivatePanels(gamePanel.name);
    }
    void ActivatePanels(string panelToBeActivated)
    {
        menuPanel.SetActive(panelToBeActivated.Equals(menuPanel.name));
        waitingAreaPanel.SetActive(panelToBeActivated.Equals(waitingAreaPanel.name));
        loadingPanel.SetActive(panelToBeActivated.Equals(loadingPanel.name));
        gamePanel.SetActive(panelToBeActivated.Equals(gamePanel.name));
    }

    public void DisplayRunes(string[] runeNames)
    {
        for (int i = 0; i < 3; i++)
        {
            
            runeSelectionList[i].GetComponentInChildren<Text>().text = runeNames[i];
        }
        runeSelection.SetActive(true);
    }

    public void SelectRune(int index)
    {
        ClientManager.Instance.localPlayer.GetComponent<PlayerRuneManager>().SendSelectRuneMessage((ushort)index);
        runeSelection.SetActive(false);
    }

    public void OnPlayerDeath()
    {
        respawnButton.SetActive(true);
    }
    public void OnRespawnButtonClicked()
    {
        OnGameLevelLoaded();
        respawnButton.SetActive(false);
        ClientManager.Instance.localPlayer.GetComponent<Player>().SendRespawnMessage();
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
}
