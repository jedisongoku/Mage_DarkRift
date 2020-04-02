using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabDataStore : MonoBehaviour
{
    public static string playFabID;
    //public static string playerName;
    //public static string playerActiveSkin;
    public static string gameMode = "Deathmatch";
    public static PlayerBaseStats playerBaseStats;
    public static PlayerProfile playerProfile;
    public static Dictionary<string, SkinModel> playerSkins = new Dictionary<string, SkinModel>();
    public static Dictionary<string, SkinModel> gameSkinCatalog = new Dictionary<string, SkinModel>();
    public static int vc_gems;
    public static int vc_coins;
    public static int vc_energy;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
