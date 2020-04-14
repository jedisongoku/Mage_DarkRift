using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatistics
{
    [SerializeField] public int gamesPlayed;
    [SerializeField] public int totalKills;
    [SerializeField] public int totalDeaths;
    [SerializeField] public int maxLevel;
    [SerializeField] public int killStreak;

    public PlayerStatistics(int _gamesPlayed, int _totalKills, int _totalDeaths, int _maxLevel, int _killStreak)
    {
        gamesPlayed = _gamesPlayed;
        totalKills = _totalKills;
        totalDeaths = _totalDeaths;
        maxLevel = _maxLevel;
        killStreak = _killStreak;
    }


}
