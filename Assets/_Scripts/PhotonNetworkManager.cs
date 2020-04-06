using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance;

    public delegate void PhotonEvents();
    public static event PhotonEvents OnJoinedRoomEvent;
    public static event PhotonEvents OnPlayerEnteredRoomEvent;

    private bool isGamePaused { get; set; }

    #region Unity Mono Calls
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    public void ConnectToPhoton()
    {
        PhotonNetwork.UseAlternativeUdpPorts = true;

        /*
        if (PlayFabDataStore.playerName != null)
        {
            PhotonNetwork.LocalPlayer.NickName = PlayFabDataStore.playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player Nickname is invalid!");
        }*/

        PhotonNetwork.ConnectUsingSettings();
        
        PhotonNetwork.AutomaticallySyncScene = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #endregion

    #region PUN Callbacks

    //OnConnected is called when connected to Internet
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
        
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
#if (!UNITY_EDITOR)
        // your code here
        Debug.Log("Disconnected");
        HUDManager.Instance.StartAppLaunch();
        SceneManager.LoadScene(0);
#endif

    }

    //OnConnectedToMaster is called once user connects to Photon Server
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Server");

    }

    //OnJoinedRoom is called when user joins to a room
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.KeepAliveInBackground = 0;

        if (OnJoinedRoomEvent != null)
        {
            OnJoinedRoomEvent();

        }
    }

    //OnJoinRandomFailed is called when user fails to join a room and creates a room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
        roomPropterties.Add("Level", PlayFabDataStore.gameMode);
        string[] lobbyProperties = { "Level", PlayFabDataStore.gameMode };
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = lobbyProperties;
        string roomName = PlayFabDataStore.gameMode + Random.Range(1000, 10000);
        roomOptions.CustomRoomProperties = roomPropterties;
        if(PlayFabDataStore.gameMode == "Deathmatch") roomOptions.MaxPlayers = 8;
        else roomOptions.MaxPlayers = 2;



        PhotonNetwork.CreateRoom(roomName, roomOptions);

        
    }
    private void OnApplicationPause(bool pause)
    {
        isGamePaused = pause;
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus && isGamePaused && !PhotonNetwork.IsConnected)
        {
            /*
            Debug.Log("Not Connected");
            HUDManager.Instance.StartAppLaunch();
            SceneManager.LoadScene(0);
            ConnectToPhoton();*/
        }
    }


    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(PlayFabDataStore.gameMode);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (OnPlayerEnteredRoomEvent != null)
        {
            OnPlayerEnteredRoomEvent();
        }
        /*
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
        {
            
            //ActivatePanels(loadingPanel.name);
            //Invoke("ActivateGamePanel", 2f);
            PhotonNetwork.LoadLevel(PersistData.instance.GameData.GameMode);
        }*/
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        PhotonNetwork.KeepAliveInBackground = 60;
#if (!UNITY_EDITOR)
        SceneManager.LoadScene(0);
#endif

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 0)
        {
            if (!PhotonNetwork.IsConnected) ConnectToPhoton();
            HUDManager.Instance.GetComponent<Canvas>().worldCamera = Camera.main;
            HUDManager.Instance.characterLocation.SetActive(true);
            PlayFabApiCalls.instance.GetPlayerInventory();
        }
        
    }

    public void UpdatePlayerName()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayFabDataStore.playerProfile.playerName;
        HUDManager.Instance.isContentDownloaded = true;
    }

    #endregion
}
