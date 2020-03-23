using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProfile
{
    [SerializeField] public string playerName;
    [SerializeField] public int totalKills;
    [SerializeField] public int totalDeaths;
    [SerializeField] public int maxLevelReached;
    [SerializeField] public int killStreak;
    [SerializeField] public string skinName;

    public PlayerProfile(string _playerName, int _totalKills, int _totalDeaths, int _maxLevelReached, int _killStreak, string _skinName)
    {
        playerName = _playerName;
        totalKills = _totalKills;
        totalDeaths = _totalDeaths;
        maxLevelReached = _maxLevelReached;
        killStreak = _killStreak;
        skinName = _skinName;
    }


}
