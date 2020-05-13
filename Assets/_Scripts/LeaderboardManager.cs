using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    public static List<PlayerLeaderboardEntry> killLeaderboardList;
    public static PlayerLeaderboardEntry killLeaderboardLocalPlayer;
    public static List<PlayerLeaderboardEntry> winLeaderboardList;
    public static PlayerLeaderboardEntry winLeaderboardLocalPlayer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private int leaderboardEntryCount = 100;
    [SerializeField] private GameObject leaderboardContent;
    [SerializeField] private GameObject playerEntry;
    [SerializeField] private Sprite swordImage;
    [SerializeField] private Sprite crownImage;

    List<GameObject> leaderboardEntryList;

    bool refreshLeaderboard = true;
    // Start is called before the first frame update

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GetLeaderboard();
    }

    void GetLeaderboard()
    {
        if (refreshLeaderboard)
        {
            refreshLeaderboard = false;
            Invoke("RefreshActivate", 60f);
            PlayFabApiCalls.instance.GetLeaderboard("Lifetime Kills");
            PlayFabApiCalls.instance.GetLeaderboardAroundPlayer("Lifetime Kills");
            PlayFabApiCalls.instance.GetLeaderboard("Deathmatch Win");
            PlayFabApiCalls.instance.GetLeaderboardAroundPlayer("Deathmatch Win");
        }
    }

    public void RefreshLeaderboard(string statisticName)
    {
        for (int i = 0; i < leaderboardEntryList.Count; i++)
        {
            if (leaderboardEntryList[i].activeInHierarchy)
            {
                leaderboardEntryList[i].SetActive(false);
            }
        }
        if (statisticName == "Lifetime Kills")
        {
            foreach (var entry in killLeaderboardList)
            {
                if (entry.DisplayName != null)
                {
                    GameObject prefab = GetLeaderboardEntryPrefab();
                    prefab.transform.Find("Placement").GetComponent<Text>().text = (entry.Position + 1).ToString();
                    prefab.transform.Find("PlayerName").GetComponent<Text>().text = entry.DisplayName.ToString();
                    prefab.transform.Find("Score/Text").GetComponent<Text>().text = entry.StatValue.ToString();
                    prefab.transform.Find("Score/Image").GetComponent<Image>().sprite = swordImage;
                    prefab.transform.parent = leaderboardContent.transform;
                    prefab.SetActive(true);
                }

            }
        }
        else
        {
            foreach (var entry in winLeaderboardList)
            {
                if (entry.DisplayName != null)
                {
                    GameObject prefab = GetLeaderboardEntryPrefab();
                    prefab.transform.Find("Placement").GetComponent<Text>().text = (entry.Position + 1).ToString();
                    prefab.transform.Find("PlayerName").GetComponent<Text>().text = entry.DisplayName.ToString();
                    prefab.transform.Find("Score/Text").GetComponent<Text>().text = entry.StatValue.ToString();
                    prefab.transform.Find("Score/Image").GetComponent<Image>().sprite = crownImage;
                    prefab.transform.parent = leaderboardContent.transform;
                    prefab.SetActive(true);
                }

            }
        }
        

    }

    public void RefreshPlayerRank(string statisticName)
    {
        PlayerLeaderboardEntry player;
        if (statisticName == "Lifetime Kills")
        {
            player = killLeaderboardLocalPlayer;
            playerEntry.transform.Find("Score/Image").GetComponent<Image>().sprite = swordImage;
        }
        else
        {
            player = winLeaderboardLocalPlayer;
            playerEntry.transform.Find("Score/Image").GetComponent<Image>().sprite = crownImage;
        }

        playerEntry.transform.Find("Placement").GetComponent<Text>().text = (player.Position + 1).ToString();
        playerEntry.transform.Find("PlayerName").GetComponent<Text>().text = player.DisplayName.ToString();
        playerEntry.transform.Find("Score/Text").GetComponent<Text>().text = player.StatValue.ToString();

    }

    public GameObject GetLeaderboardEntryPrefab()
    {
        for (int i = 0; i < leaderboardEntryList.Count; i++)
        {
            if (!leaderboardEntryList[i].activeInHierarchy)
            {
                return leaderboardEntryList[i];
            }
        }
        return null;
    }



    void Start()
    {
        leaderboardEntryList = new List<GameObject>();


        //Primary Skill Particle Instantiation 
        for (int i = 0; i < leaderboardEntryCount; i++)
        {
            GameObject obj = (GameObject)Instantiate(leaderboardEntryPrefab, leaderboardContent.transform);
            obj.SetActive(false);
            leaderboardEntryList.Add(obj);
        }
    }

    void RefreshActivate()
    {
        foreach(var item in leaderboardEntryList)
        {
            item.SetActive(false);
        }
        refreshLeaderboard = true;
    }

}
