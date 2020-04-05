using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private int leaderboardEntryCount = 100;
    [SerializeField] private GameObject leaderboardContent;
    [SerializeField] private GameObject playerEntry;

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
            PlayFabApiCalls.instance.GetLeaderboard();
            PlayFabApiCalls.instance.GetLeaderboardAroundPlayer();
        }
    }

    public void RefreshLeaderboard(List<PlayerLeaderboardEntry> leaderboard)
    {
        foreach(var entry in leaderboard)
        {
            Debug.Log("Name " + entry.DisplayName);
            GameObject prefab = GetLeaderboardEntryPrefab();
            prefab.transform.Find("Placement").GetComponent<Text>().text = (entry.Position + 1).ToString();
            prefab.transform.Find("PlayerName").GetComponent<Text>().text = entry.DisplayName.ToString();
            prefab.transform.Find("Score/Text").GetComponent<Text>().text = entry.StatValue.ToString();
            prefab.transform.parent = leaderboardContent.transform;
            prefab.SetActive(true);
        }

    }

    public void RefreshPlayerRank(PlayerLeaderboardEntry player)
    {
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
        refreshLeaderboard = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
