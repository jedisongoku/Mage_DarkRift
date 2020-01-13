using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public Dictionary<int, ScorePlayer> scoreBoard = new Dictionary<int, ScorePlayer>();

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }


    public void AddPlayer(int playerID, ScorePlayer player, bool isServer)
    {
        if (!scoreBoard.ContainsKey(playerID))
        {
            scoreBoard.Add(playerID, player);
        }
        if(!isServer) HUDManager.Instance.RefreshScoreboard();

    }

    public void RemovePlayer(int playerID, bool isServer)
    {
        if (scoreBoard.ContainsKey(playerID))
        {
            scoreBoard.Remove(playerID);
        }
        if (!isServer) HUDManager.Instance.RefreshScoreboard();
    }

    public void UpdateScoreboard(int _killerID, int _victimID)
    {
        //Increment the killer's score
        if (scoreBoard.ContainsKey(_killerID))
        {
            scoreBoard[_killerID].score = (ushort)(scoreBoard[_killerID].score + 1);
        }
        //reset the victim's score
        if (scoreBoard.ContainsKey(_victimID))
        {
            scoreBoard[_victimID].score = 0;
        }

        SendScoreboardMessage(_killerID, (ushort)scoreBoard[_killerID].score, _victimID);
    }

    public void UpdateScoreboard(ushort _killerID, ushort score, ushort _victimID)
    {
        if (scoreBoard.ContainsKey(_killerID))
        {
            scoreBoard[_killerID].score = score;
        }
        if (scoreBoard.ContainsKey(_victimID))
        {
            scoreBoard[_victimID].score = 0;
        }
        HUDManager.Instance.RefreshScoreboard();
    }

    void SendScoreboardMessage(int _killerID, ushort score, int _victimID)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            ScoreboardMessageModel newMessage = new ScoreboardMessageModel()
            {
                KillerID = (ushort)_killerID,
                Score = score,
                VictimID = (ushort)_victimID

            };
            using (Message message = Message.Create(NetworkTags.ScoreboardTag, newMessage))
                foreach (IClient c in ServerManager.Instance.gameServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(message, SendMode.Reliable);
        }
    }
}

public class ScorePlayer
{
    public int id;
    public string name;
    public int score;

    public ScorePlayer(int _id, string _name, int _score)
    {
        id = _id;
        name = _name;
        score = _score;
    }
}
