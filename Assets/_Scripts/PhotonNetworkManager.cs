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

        if (PersistData.instance.GameData.PlayerName != null)
        {
            PhotonNetwork.LocalPlayer.NickName = PersistData.instance.GameData.PlayerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player Nickname is invalid!");
        }
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

    //OnConnectedToMaster is called once user connects to Photon Server
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Connected to Photon Server");
    }

    //OnJoinedRoom is called when user joins to a room
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);

        if(OnJoinedRoomEvent != null)
        {
            OnJoinedRoomEvent();
        }
    }

    //OnJoinRandomFailed is called when user fails to join a room and creates a room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
        roomPropterties.Add("Level", PersistData.instance.GameData.GameMode);
        string[] lobbyProperties = { "Level", PersistData.instance.GameData.GameMode };
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = lobbyProperties;
        string roomName = PersistData.instance.GameData.GameMode + Random.Range(1000, 10000);
        roomOptions.CustomRoomProperties = roomPropterties;
        roomOptions.MaxPlayers = 8;

        PhotonNetwork.CreateRoom(roomName, roomOptions);

        
    }


    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(PersistData.instance.GameData.GameMode);
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
        SceneManager.LoadScene(0);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HUDManager.Instance.GetComponent<Canvas>().worldCamera = Camera.main;
        HUDManager.Instance.characterLocation.SetActive(true);
    }

    #endregion
}
