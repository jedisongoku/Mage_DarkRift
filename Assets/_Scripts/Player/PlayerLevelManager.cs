using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerLevelManager : MonoBehaviourPunCallbacks
{

    private int firstLevelXP= 19;
    private float levelCoefficient = 1.35f;
    private int currentXP = 0;
    private int currentLevel = 1;
    private int killXP = 15;
    private float killXPCoefficient = 1.2f;

    public float SmartMultiplier { get; set; }

    [SerializeField] private GameObject levelStar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject levelUpParticle;
    [SerializeField] Color floatingTextColor;

    // Start is called before the first frame update

    void Start()
    {
        if (photonView.IsMine)
        {
            levelStar.SetActive(false);
            HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        }
        SmartMultiplier = 1;
        
    }
    void OnEnable()
    {
        if(photonView.IsMine)
        {
            GameManager.OnPlayerKill += RewardXP;
        }
        
    }

    // Update is called once per frame
    void OnDisable()
    {
        if(photonView.IsMine)
        {
            GameManager.OnPlayerKill -= RewardXP;
        }
        
    }

    public int GetPlayerLevel()
    {
        return currentLevel;
    }

    public int NextLevelInXP()
    {
        return Mathf.FloorToInt(firstLevelXP * Mathf.Pow(levelCoefficient, currentLevel));
    }

    public void AddXP(int _xp)
    {
        if(photonView.IsMine)
        {
            Debug.Log("xp earned :" + _xp + "current level " + currentLevel);
            ShowFloatingCombatText(Mathf.RoundToInt(_xp * SmartMultiplier));
            currentXP += Mathf.RoundToInt(_xp * SmartMultiplier);
            if (currentXP >= Mathf.FloorToInt(firstLevelXP * Mathf.Pow(levelCoefficient, currentLevel)))
            {
                currentXP -= NextLevelInXP();
                currentLevel++;
                levelUpParticle.SetActive(false);
                levelUpParticle.SetActive(true);
                HUDManager.Instance.DisplayRunes();
                photonView.RPC("UpdateLevel", RpcTarget.OthersBuffered, currentLevel);
                if (currentXP > NextLevelInXP()) currentXP = NextLevelInXP() - 5;

            }

            HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        }
        

    }

    void RewardXP()
    {
        Debug.Log("REWARDS XP");
        AddXP(Mathf.FloorToInt(killXP * Mathf.Pow(killXPCoefficient, GameManager.Instance.DeadPlayerLevel)));
    }

    [PunRPC]
    void UpdateLevel(int _level)
    {
        Debug.Log("Level " + _level);
        if(_level > currentLevel)
        {
            Debug.Log(":LEVEL UP PARTICLE");
            levelUpParticle.SetActive(false);
            levelUpParticle.SetActive(true);
        }
        currentLevel = _level;
        levelText.text = _level.ToString();
    }

    public void ResetOnRespawn()
    {
        currentXP = 0;
        currentLevel = 1;
        SmartMultiplier = 1;
        HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        photonView.RPC("UpdateLevel", RpcTarget.Others, currentLevel);    

    }

    void ShowFloatingCombatText(float amount)
    {
        GameObject obj = ObjectPooler.Instance.GetFloatingCombatTextPrefab();
        obj.GetComponent<TextMeshPro>().text = Mathf.RoundToInt(amount) + " XP";
        obj.GetComponent<TextMeshPro>().color = floatingTextColor;
        obj.transform.position = transform.position;
        obj.SetActive(true);

    }
}
