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
    [SerializeField] private GameObject levelUpParticle;

    // Start is called before the first frame update

    void Start()
    {
        if (photonView.IsMine) levelStar.SetActive(false);   
    }
    void OnEnable()
    {
        if(photonView.IsMine)
        {
            Debug.Log("LEVEL MANAGER ENABLE");
            GameManager.OnPlayerKill += RewardXP;
        }
        
    }

    // Update is called once per frame
    void OnDisable()
    {
        if(photonView.IsMine)
        {
            Debug.Log("LEVEL MANAGER DISABLE");
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
        Debug.Log("xp earned :" + _xp + "current level "  + currentLevel);
        currentXP += _xp;
        if(currentXP >= Mathf.FloorToInt(firstLevelXP * Mathf.Pow(levelCoefficient, currentLevel)))
        {
            currentXP -= NextLevelInXP();
            currentLevel++;
            levelUpParticle.SetActive(false);
            levelUpParticle.SetActive(true);
            HUDManager.Instance.DisplayRunes();
            photonView.RPC("UpdateLevel", RpcTarget.Others, currentLevel);
            
        }

        HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
    }

    void RewardXP()
    {
        Debug.Log("REWARDS XP");
        AddXP(Mathf.FloorToInt(killXP * Mathf.Pow(killXPCoefficient, GameManager.Instance.DeadPlayerLevel)));
    }

    [PunRPC]
    void UpdateLevel(int _level)
    {
        if(_level > currentLevel)
        {
            Debug.Log(":LEVEL UP PARTICLE");
            levelUpParticle.SetActive(false);
            levelUpParticle.SetActive(true);
        }
        currentLevel = _level;
        levelText.text = _level.ToString();
    }

    public void ResetOnDeath()
    {
        currentXP = 0;
        currentLevel = 1;
        HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        photonView.RPC("UpdateLevel", RpcTarget.Others, currentLevel);    

    }
}
