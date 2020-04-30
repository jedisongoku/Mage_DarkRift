using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public delegate void KillEvent();
    public static event KillEvent OnPlayerKill;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject[] spawnLocations;
    [SerializeField] GameObject botPlayerPrefab;

    private static GameObject currentPlayer;
    private List<GameObject> botPlayerList = new List<GameObject>();
    private static int currentPlayerViewID;
    public static int playerKillCount = 0;
    private int respawnCooldown = 6;
    private bool canRespawn = false;
    
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
        botPlayerCount = 0;

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
                    for(int i = 1; i < PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount; i++)
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

                GameObject bot = PhotonNetwork.Instantiate(botPlayerPrefab.name, spawnLocations[spawnLocationIndex].transform.position, Quaternion.identity);
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
            return Random.Range(0, PhotonNetwork.CurrentRoom.MaxPlayers);
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
        ScoreManager.Instance.RefreshKillFeed(PhotonNetwork.GetPhotonView(_playerViewID).Owner.NickName, _playerkilled.GetComponent<PhotonView>().Owner.NickName);

        if(_playerViewID == currentPlayerViewID)
        {
            DeadPlayerLevel = _playerkilled.GetComponent<PlayerLevelManager>().GetPlayerLevel();
            Debug.Log("Dead player level " + DeadPlayerLevel);
            playerKillCount++;

            if(OnPlayerKill != null)
            {
                OnPlayerKill();
            }
        }
    }


}
