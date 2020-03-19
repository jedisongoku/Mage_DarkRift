using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerLevelManager : MonoBehaviourPunCallbacks
{

    private int firstLevelXP= 16;
    private float levelCoefficient = 1.6f;
    private int currentXP = 0;
    private int currentLevel = 1;
    private int killXP = 15;
    private float killXPCoefficient = 1.2f;

    [SerializeField] private GameObject levelStar;
    [SerializeField] private TextMeshProUGUI levelText;

    // Start is called before the first frame update

    void Start()
    {
        if (photonView.IsMine) levelStar.SetActive(false);   
    }
    void OnEnable()
    {
        GameManager.OnPlayerKill += RewardXP;
    }

    // Update is called once per frame
    void OnDisable()
    {
        GameManager.OnPlayerKill -= RewardXP;
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
        currentXP += _xp;
        if(currentXP > firstLevelXP * Mathf.Pow(levelCoefficient, currentLevel))
        {
            currentXP -= NextLevelInXP();
            currentLevel++;
            HUDManager.Instance.DisplayRunes();
            photonView.RPC("UpdateLevel", RpcTarget.OthersBuffered, currentLevel);
            
        }

        HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
    }

    void RewardXP()
    {
        Debug.Log("REWARDS XP");
        AddXP(killXP * Mathf.FloorToInt(Mathf.Pow(killXPCoefficient, GameManager.Instance.DeadPlayerLevel)));
    }

    [PunRPC]
    void UpdateLevel(int _level)
    {
        currentLevel = _level;
        levelText.text = _level.ToString();
    }
}
