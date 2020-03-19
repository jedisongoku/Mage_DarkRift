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

    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
    private List<ScorePlayer> playersList;

    private List<KillFeed> killFeed;
    private int score;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        killFeed = new List<KillFeed>();



        playersList = new List<ScorePlayer>();
    }

    public void StartScoreboard()
    {

        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
        {
            score = 0;
            playerProperties.Add("Score", score);
        }
        //StartCoroutine(AutoUpdate());
    }

    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            Invoke("UpdateScore", 1f);
        }
    }

    IEnumerator AutoUpdate()
    {
        UpdateScore();
        yield return new WaitForSeconds(1);
        StartCoroutine(AutoUpdate());


    }
    public void UpdateScore()
    {
        playersList.Clear();
        Debug.Log("Updating Score");
        playerProperties["Score"] = score;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        RefreshScoreboard();

    }

    void RefreshScoreboard()
    {
        /*
        var tempList = PhotonNetwork.PlayerList;
        foreach(var player in tempList)
        {
            playersList.Add(new ScorePlayer(player.NickName, (int)player.CustomProperties["Score"]));
        }

        playersList = playersList.OrderByDescending(ScorePlayer => ScorePlayer.score).ToList();
        */
        Player[] scoreList = PhotonNetwork.PlayerList.OrderByDescending(Player => Player.CustomProperties["Score"]).ToArray();

        for (int i = 0; i < HUDManager.Instance.ScoreboardItems.Length; i++)
        {
            if (scoreList.Length > i && scoreList[i].CustomProperties.ContainsKey("Score"))
            {
                HUDManager.Instance.ScoreboardItems[i].SetActive(true);
                HUDManager.Instance.ScoreboardItems[i].transform.Find("ScoreboardPlayerNameText").GetComponent<Text>().text = scoreList[i].NickName;
                HUDManager.Instance.ScoreboardItems[i].transform.Find("ScoreboardPlayerScoreText").GetComponent<Text>().text = scoreList[i].CustomProperties["Score"].ToString();

            }
            else
            {
                HUDManager.Instance.ScoreboardItems[i].SetActive(false);
            }
        }
    }

    public void RefreshKillFeed(string _killer, string _killed)
    {
        killFeed.Add(new KillFeed(_killer, _killed));

        List<KillFeed> reversedList = killFeed;
        reversedList.Reverse();

        int count = reversedList.Count <= HUDManager.Instance.killFeed.Length ? reversedList.Count : HUDManager.Instance.killFeed.Length;

        for (int i = 0; i < count; i++)
        {
            HUDManager.Instance.killFeed[i].GetComponent<Text>().text = reversedList[i].killer + " killed " + reversedList[i].killed;
            HUDManager.Instance.killFeed[i].SetActive(true);

        }
    }

    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        RefreshScoreboard();
        //Redo client score here
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