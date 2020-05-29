using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private GameObject killFeedPrefab;
    [SerializeField] private int killFeedPrefabPooledAmount = 5;
    
    List<GameObject> killFeedPrefabList;
    GameObject killFeedParent;

    public Dictionary<string, int> playerScoreList = new Dictionary<string, int>();
    public string localPlayerName;

    //public List<ScorePlayer> playersList;    

    private List<KillFeed> killFeed;
    private int score;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        killFeed = new List<KillFeed>();
        //playersList = new List<ScorePlayer>();

        killFeedPrefabList = new List<GameObject>();

        killFeedParent = HUDManager.Instance.scoreboardContent;

        for (int i = 0; i < killFeedPrefabPooledAmount; i++)
        {
            GameObject obj = Instantiate(killFeedPrefab, killFeedParent.transform);
            //obj.transform.SetParent(killFeedParent.transform);
            obj.SetActive(false);
            killFeedPrefabList.Add(obj);
        }
    }

    private void OnDisable()
    {
        foreach(var feed in killFeedPrefabList)
        {
            Destroy(feed);
        }
    }

    #region scoreboard

    public void StartScoreboard(string name)
    {
        //playersList.Add(new ScorePlayer(name, 0));
        if (!playerScoreList.ContainsKey(name))
            playerScoreList.Add(name, 0);

        RefreshScoreboard();
    }

    public void RemoveScoreboard(string name)
    {
        //playersList.Add(new ScorePlayer(name, 0));
        if (!playerScoreList.ContainsKey(name))
            playerScoreList.Remove(name);
    }


    public void UpdateScore(string name)
    {
        if (playerScoreList.ContainsKey(name))
            playerScoreList[name]++;

        RefreshScoreboard();

    }

    public void RefreshScoreboard()
    {
        var scoreList = playerScoreList.OrderByDescending(p => p.Value).ToArray();

        if(scoreList.Length > 2)
        {
            HUDManager.Instance.ScoreboardItems[0].transform.Find("KillFeed").GetComponent<Text>().text = scoreList[0].Key + " - " + scoreList[0].Value + " kills";
            HUDManager.Instance.ScoreboardItems[1].transform.Find("KillFeed").GetComponent<Text>().text = scoreList[1].Key + " - " + scoreList[1].Value + " kills";
            HUDManager.Instance.ScoreboardItems[2].transform.Find("KillFeed").GetComponent<Text>().text = scoreList[2].Key + " - " + scoreList[2].Value + " kills";
        }

        if(scoreList[0].Value == PlayFabDataStore.deathmatchFinalScore)
        {
            GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerNetworkManager>().WinGame(scoreList[0].Key.ToString());
        }
        


    }

    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        RefreshScoreboard();
        //Redo client score here
    }

    void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Custom room property changed");
    }
    #endregion


    public GameObject GetKillFeedPrefab()
    {
        for (int i = 0; i < killFeedPrefabList.Count; i++)
        {
            if (!killFeedPrefabList[i].activeInHierarchy)
            {
                return killFeedPrefabList[i];
            }
        }

        return null;
    }



    public void RefreshKillFeed(string _killer, string _killed)
    {
        killFeed.Add(new KillFeed(_killer, _killed));

        GameObject feed = GetKillFeedPrefab();
        feed.transform.Find("KillFeed").GetComponent<Text>().text = _killer + " killed " + _killed;
        //feed.transform.SetParent(killFeedParent.transform);
        feed.SetActive(true);
    }


}


public class ScorePlayer
{
    public string name;
    public int score;

    public ScorePlayer(string _name, int _score)
    {
        name = _name;
        score = _score;
    }
}

public class KillFeed
{
    public string killer;
    public string killed;

    public KillFeed(string _killer, string _killed)
    {
        killer = _killer;
        killed = _killed;
    }
}