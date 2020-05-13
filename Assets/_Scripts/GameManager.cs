using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public delegate void KillEvent();
    public static event KillEvent OnPlayerKill;

    public delegate void GameOverEvent();
    public static event GameOverEvent OnGameOver;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject[] spawnLocations;
    [SerializeField] GameObject botPlayerPrefab;

    private static GameObject currentPlayer;
    private List<GameObject> botPlayerList = new List<GameObject>();
    private static int currentPlayerViewID;
    public static int playerKillCount = 0;
    public static int playerTotalKillCount = 0;
    public static int playerTotalDeathCount = 0;
    private int respawnCooldown = 6;
    private bool canRespawn = false;
    private int lastSpawnLocation;
    public static bool isGameOver = false;
    public static bool isWinner = false;
    
    public int botPlayerCount { get; set; }

    public int DeadPlayerLevel { get; set; }

    public int RespawnCooldown
    {
        get
        {
            return respawnCooldown;
        }
        set
        {
            respawnCooldown = value;
        }
    }

    public bool CanRespawn
    {
        get
        {
            return canRespawn;
        }
        set
        {
            canRespawn = value;
        }
    }


    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        HUDManager.Instance.OnGameLevelLoaded();
        playerKillCount = 0;
        playerTotalKillCount = 0;
        botPlayerCount = 0;
        OnPlayerKill = null;
        OnPlayerKill = null;
        HUDManager.Instance.UpdateTotalKillsScoreText(playerTotalKillCount);

    }


    public void InitializePlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null)
            {
                int spawnLocationIndex;
                if (PhotonNetwork.LocalPlayer.ActorNumber <= PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    spawnLocationIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                }
                else
                {
                    spawnLocationIndex = SpawnLocationIndex;
                }

                currentPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocations[spawnLocationIndex].transform.position, Quaternion.identity);

                currentPlayerViewID = currentPlayer.GetComponent<PhotonView>().ViewID;
                if(PlayFabDataStore.gameMode == "Deathmatch")
                    PlayerRuneManager.Instance.Initialize();

                if(PhotonNetwork.IsMasterClient)
                {
                    for (int i = 1; i < PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount + 1; i++)
                    {
                        InitializeBotPlayer(i);
                    }
                }
                    
            }
            else
            {
                Debug.Log("Missin PlayerPrefab");
            }

        }

        
    }

    public void InitializeBotPlayer(int index)
    {
        Debug.Log("Bot Init");
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (botPlayerPrefab != null)
            {
                int spawnLocationIndex = index;

                GameObject bot = PhotonNetwork.InstantiateSceneObject(botPlayerPrefab.name, spawnLocations[spawnLocationIndex].transform.position, Quaternion.identity);
                bot.GetComponent<PlayerAIController>().spawnLocation = spawnLocationIndex;
                botPlayerCount++;

            }
            else
            {
                Debug.Log("Missin PlayerPrefab");
            }

        }

        HUDManager.Instance.UpdateTotalPlayerCount();
        
    }


    public void RespawnPlayer()
    {
        currentPlayer.GetComponent<PlayerCombatManager>().RespawnPlayer();
        playerKillCount = 0;
        //Invoke("SetSpawnLocation",0.1f);

    }

    public int GetRewardAmount()
    {
        return Mathf.RoundToInt(playerKillCount * 10 * Mathf.Pow(1.15f, currentPlayer.GetComponent<PlayerLevelManager>().GetPlayerLevel()));
    }

    public int SpawnLocationIndex
    {
        get
        {
            if(PhotonNetwork.IsMasterClient)
            {
                int index;
                do
                {
                    index = Random.Range(0, PhotonNetwork.CurrentRoom.MaxPlayers);
                } while (index == lastSpawnLocation);
                lastSpawnLocation = index;
                return lastSpawnLocation;
            }
            else
            {
                return Random.Range(0, PhotonNetwork.CurrentRoom.MaxPlayers);
            }
            
        }
    }

    public Vector3 SpawnLocation(int index)
    {
        return spawnLocations[index].transform.position;
    }

    public GameObject GetCurrentPlayer
    {
        get
        {
            return currentPlayer;
        }
    }

    public int GetCurrentPlayerViewID
    {
        get
        {
            return currentPlayerViewID;
        }
    }

    public void KillFeed(int _playerViewID, GameObject _playerkilled)
    {
        string killerName = PhotonNetwork.GetPhotonView(_playerViewID).GetComponent<PlayerCombatManager>().killFeedName;
        string victimName = _playerkilled.GetComponent<PlayerCombatManager>().killFeedName;
        

        ScoreManager.Instance.RefreshKillFeed(killerName, victimName);

        ScoreManager.Instance.UpdateScore(killerName);

        if(!PhotonNetwork.GetPhotonView(_playerViewID).GetComponent<PlayerLevelManager>().isPlayer)
        {
            PhotonNetwork.GetPhotonView(_playerViewID).GetComponent<PlayerLevelManager>().RewardXP();
        }

        

        if (_playerViewID == currentPlayerViewID)
        {
            
            DeadPlayerLevel = _playerkilled.GetComponent<PlayerLevelManager>().GetPlayerLevel();
            playerKillCount++;
            playerTotalKillCount++;
            HUDManager.Instance.UpdateTotalKillsScoreText(playerTotalKillCount);

            if (playerTotalKillCount == PlayFabDataStore.deathmatchFinalScore) isWinner = true;


            if (OnPlayerKill != null)
            {
                OnPlayerKill();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if(PhotonNetwork.IsMasterClient)
            SwitchMaster();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && PhotonNetwork.IsMasterClient)
        {
            SwitchMaster();
        }
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused && PhotonNetwork.IsMasterClient)
        {
            SwitchMaster();
        }
    }

    void SwitchMaster()
    {
        if (PhotonNetwork.IsMasterClient)
            if (PhotonNetwork.MasterClient.GetNext() != null)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
                PhotonNetwork.SendAllOutgoingCommands();
            }
                
    }

    public void GameOver()
    {
        if (isWinner)
            PlayFabApiCalls.instance.UpdateStatistics("Deathmatch Win", 1);
        PlayFabApiCalls.instance.UpdateStatistics("Deaths", playerTotalDeathCount);
        PlayFabApiCalls.instance.UpdateStatistics("Lifetime Kills", playerTotalKillCount);

        if (OnGameOver != null)
        {
            OnGameOver();
            isGameOver = true;
        }
    }

}
