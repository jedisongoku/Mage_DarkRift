using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerNetworkManager : MonoBehaviourPun
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
            photonView.RPC("SelectSkin", RpcTarget.AllBuffered, Random.Range(0, playerSkins.Length));

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
}
