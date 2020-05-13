
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabDataStore : MonoBehaviour
{
    public static string playFabID;
    //public static string playerName;
    //public static string playerActiveSkin;
    public static string gameMode = "Deathmatch";
    public static int deathmatchFinalScore = 5;
    public static PlayerBaseStats playerBaseStats;
    public static PlayerProfile playerProfile;
    public static Dictionary<string, int> playerStatistics = new Dictionary<string, int>()
    {
        {"Games Played", 0 },
        {"Lifetime Kills", 0 },
        {"Deaths", 0 },
        {"Kill Streak", 0 },
        {"Deathmatch Win", 0 }
    };
    public static Dictionary<string, SkinModel> playerSkins = new Dictionary<string, SkinModel>();
    public static Dictionary<string, SkinModel> gameSkinCatalog = new Dictionary<string, SkinModel>();
    public static int vc_gems;
    public static int vc_coins;
    public static int vc_energy;
    public static int vc_adGem;
    public static int vc_adCoin;
    public static int adGemRechargeTime;
    public static int adCoinRechargeTime;
    public static bool gameCenterLinked = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
