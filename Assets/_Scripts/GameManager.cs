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
    [SerializeField] GameObject poisonShopManager;
    [SerializeField] GameObject[] spawnLocations;

    private static GameObject currentPlayer;
    private static int currentPlayerViewID;
    private static int playerKillCount = 0;
    private int respawnCooldown = 6;
    private bool canRespawn = false;

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
  
    }


    public void InitializePlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null)
            {
                int spawnLocationIndex;
                if (PhotonNetwork.LocalPlayer.ActorNumber <= 8)
                {
                    spawnLocationIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                }
                else
                {
                    spawnLocationIndex = SpawnLocationIndex;
                }

                currentPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocations[spawnLocationIndex].transform.position, Quaternion.identity);

                currentPlayerViewID = currentPlayer.GetComponent<PhotonView>().ViewID;
            }
            else
            {
                Debug.Log("Missin PlayerPrefab");
            }

        }
        
    }


    public void RespawnPlayer()
    {
        currentPlayer.GetComponent<PlayerCombatManager>().RespawnPlayer();
        //Invoke("SetSpawnLocation",0.1f);

    }

    public int SpawnLocationIndex
    {
        get
        {
            return Random.Range(0, 8);
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

    public void KillFeed(int playerViewID)
    {
        if(playerViewID == currentPlayerViewID)
        {
            ScoreManager.Instance.Score = ScoreManager.Instance.Score + 1;
            playerKillCount++;
            HUDManager.Instance.DisplayRunes();

            if(OnPlayerKill != null)
            {
                OnPlayerKill();
            }
        }
    }


}
