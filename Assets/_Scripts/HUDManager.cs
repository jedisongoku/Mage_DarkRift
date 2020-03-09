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
    public Text respawnTimerText;
    public GameObject runeSelection;
    public Image primarySkillCooldownImage;
    public Image secondarySkillCooldownImage;
    public Button secondarySkillButton;
    public GameObject[] scoreboardList;
    public GameObject[] runeSelectionList;

    [Header("Poison Shop")]
    public Text nextPoisonTimerText;
    public Image posionPickupProgress;



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

    public void EnablePoisonPickupProgress()
    {
        posionPickupProgress.transform.parent.gameObject.SetActive(true);
    }

    public void SetPoisonPickupProgress(float _amount)
    {
        posionPickupProgress.fillAmount = _amount;
    }

    public void DisablePoisonPickupProgress()
    {
        SetPoisonPickupProgress(0);
        posionPickupProgress.transform.parent.gameObject.SetActive(false);
    }

    public void EnablePoisonTimer()
    {
        nextPoisonTimerText.gameObject.SetActive(true);
    }

    //timer that counts for the next poison
    public void UpdatePoisonTimer(int _timer)
    {
        nextPoisonTimerText.text = "Next Poison in " + _timer;
    }

    //disable the timer when time runs out
    public void DisablePoisonTimer()
    {
        nextPoisonTimerText.gameObject.SetActive(false);
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
