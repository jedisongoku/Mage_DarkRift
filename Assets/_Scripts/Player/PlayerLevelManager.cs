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
    public bool isPlayer = true;

    [SerializeField] private GameObject levelStar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject levelUpParticle;
    [SerializeField] Color floatingTextColor;

    // Start is called before the first frame update

    void Start()
    {
        if (photonView.IsMine && isPlayer)
        {
            levelStar.SetActive(false);
            HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        }
        SmartMultiplier = 1;
        
    }
    void OnEnable()
    {
        if(photonView.IsMine && isPlayer)
        {
            GameManager.OnPlayerKill += RewardXP;
        }
        
    }

    // Update is called once per frame
    void OnDisable()
    {
        if(photonView.IsMine && isPlayer)
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
            if(isPlayer) ShowFloatingCombatText(Mathf.RoundToInt(_xp * SmartMultiplier));
            currentXP += Mathf.RoundToInt(_xp * SmartMultiplier);
            if (currentXP >= Mathf.FloorToInt(firstLevelXP * Mathf.Pow(levelCoefficient, currentLevel)))
            {
                currentXP -= NextLevelInXP();
                currentLevel++;
                levelText.text = currentLevel.ToString();
                levelUpParticle.SetActive(false);
                levelUpParticle.SetActive(true);
                if (isPlayer) HUDManager.Instance.DisplayRunes();
                else GetComponent<PlayerAIRuneManager>().SelectRune(currentLevel - 2);
                photonView.RPC("UpdateLevel", RpcTarget.OthersBuffered, currentLevel);
                if (currentXP > NextLevelInXP()) currentXP = NextLevelInXP() - 5;

            }

            if(isPlayer) HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        }
        

    }

    public void RewardXP()
    {
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
        levelText.text = currentLevel.ToString();
        if (isPlayer) HUDManager.Instance.UpdatePlayerLevel(currentLevel, currentXP, NextLevelInXP());
        else GetComponent<PlayerAIRuneManager>().RestartPlayerRunes();
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
