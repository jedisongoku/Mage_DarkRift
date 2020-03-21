using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerNetworkManager : MonoBehaviourPunCallbacks
{
    
    [SerializeField] private TextMeshProUGUI playerNameText;
    public GameObject[] playerSkins;
    public GameObject playerSkinParent;


    ExitGames.Client.Photon.Hashtable poisonShopRoomProperties = new ExitGames.Client.Photon.Hashtable();
    RoomOptions poisonShopRoomOptions = new RoomOptions();

    // Start is called before the first frame update
    void Start()
    {
        SetPlayerUI();
        if(photonView.IsMine)
        {
            photonView.RPC("SelectSkin", RpcTarget.All, Random.Range(0, playerSkins.Length));

        }
    }

    void SetPlayerUI()
    {
        playerNameText.text = photonView.Owner.NickName;
    }

    [PunRPC]
    void SelectSkin(int _skinIndex)
    {
        GameObject skin = Instantiate(playerSkins[_skinIndex], playerSkinParent.transform) as GameObject;
        GetComponent<PlayerCombatManager>().PlayerModel = skin;
        GetComponent<PlayerHealthManager>().PlayerModel = skin;
        playerSkinParent.GetComponent<Animator>().Rebind();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        photonView.RPC("SelectSkin", newPlayer, Random.Range(0, playerSkins.Length));
        photonView.RPC("StartCartAnimation", newPlayer, CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Forward"), 
            CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime, CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length,
            PhotonNetwork.ServerTimestamp);
        //GetComponent<PlayerHealthManager>().InformNewUser(newPlayer);

    }

    [PunRPC]
    void StartCartAnimation(bool _animationState, float _time, float _length, int _timeStamp)
    {
        float lag = Mathf.Abs((float)(PhotonNetwork.ServerTimestamp - _timeStamp)) / 1000;
        _time += lag / _length;
        if(_animationState) CartController.instance.GetComponent<Animator>().Play("Forward", 0, _time);
        else CartController.instance.GetComponent<Animator>().Play("Backward", 0, _time);
    }
}
