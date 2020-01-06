using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;
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
    public Image primarySkillCooldownImage;
    public Image secondarySkillCooldownImage;

    // Start is called before the first frame update
    void Start()
    {
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
