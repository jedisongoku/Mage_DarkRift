using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.LocalPlayer.NickName = PersistData.instance.GameData.PlayerName;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

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

    public override void OnJoinedRoom()
    {
        //this is handled in the HudManager
        Debug.Log("Joined Room");

    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        ExitGames.Client.Photon.Hashtable roomPropterties = new ExitGames.Client.Photon.Hashtable();
        Debug.Log("GAME MODE " + PersistData.instance.GameData.GameMode);
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
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }


    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
    }

    #endregion


}
