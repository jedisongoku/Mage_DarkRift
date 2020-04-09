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

    

    [SerializeField] float power = 10f;
    [SerializeField] float radius = 5f;
    [SerializeField] float upforce = 2f;

    private Vector3 previousLocation;
    private float gemSpawnTime;
    private bool hasSkin = false;

    // Start is called before the first frame update
    void Start()
    {
        SetPlayerUI();
        if(photonView.IsMine)
        {
            photonView.RPC("SelectSkin", RpcTarget.All, PlayFabDataStore.playerProfile.skinName, false);
            GetComponent<AudioListener>().enabled = true;
        }

        if (PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            StartCoroutine(SpawnGems());
            previousLocation = CartController.instance.GetCartLocation;
            //GameManager.Instance.InitializeBotPlayer();
        }
    }

    void SetPlayerUI()
    {
        playerNameText.text = photonView.Owner.NickName;
    }

    [PunRPC]
    void SelectSkin(string _skinName, bool _isDead)
    {
        if(!hasSkin)
        {
            hasSkin = true;
            GameObject skin = Instantiate(Resources.Load("Skins/" + _skinName) as GameObject, playerSkinParent.transform);
            GetComponent<PlayerCombatManager>().PlayerModel = skin;
            GetComponent<PlayerHealthManager>().PlayerModel = skin;
            playerSkinParent.GetComponent<Animator>().Rebind();
            if (_isDead)
            {
                GetComponent<PlayerCombatManager>().DisablePlayer();
            }
        }
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(photonView.IsMine)
        {
            photonView.RPC("SelectSkin", newPlayer, PlayFabDataStore.playerProfile.skinName, GetComponent<PlayerCombatManager>().IsDead);
            HUDManager.Instance.UpdateTotalPlayerCount();

        }

        if(photonView.IsMine && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCartAnimation", RpcTarget.Others, CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Forward"),
                CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime, CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length,
                PhotonNetwork.ServerTimestamp);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (photonView.IsMine)
        {
            HUDManager.Instance.UpdateTotalPlayerCount();

        }
    }

    [PunRPC]
    void StartCartAnimation(bool _animationState, float _time, float _length, int _timeStamp)
    {
        float lag = Mathf.Abs((float)(PhotonNetwork.ServerTimestamp - _timeStamp)) / 1000;
        _time += lag / _length;
        if(_animationState) CartController.instance.GetComponent<Animator>().Play("Forward", 0, _time);
        else CartController.instance.GetComponent<Animator>().Play("Backward", 0, _time);
    }

    IEnumerator SpawnGems()
    {

        gemSpawnTime = Random.Range(3f, 5f);

        yield return new WaitForSeconds(gemSpawnTime);

        int random = Random.Range(-100, 100);
        random = random >= 0 ? 1 : -1;
        Vector3 randomVector = new Vector3(0, 0, random);

        

        int power = Random.Range(7, 12);
        if ((previousLocation - CartController.instance.GetCartLocation).magnitude > 1)
        {
            Debug.Log("SpawnGem");
            photonView.RPC("SpawnGem", RpcTarget.AllViaServer, power, CartController.instance.GetSpawnLocation, randomVector);
            previousLocation = CartController.instance.GetCartLocation;
        }


        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnGems());
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            StartCoroutine(SpawnGems());
            previousLocation = transform.position;
        }
    }

    [PunRPC]
    void SpawnGem(int power, Vector3 spawnLocation, Vector3 randomVector)
    {

        GameObject obj = ObjectPooler.Instance.GetGemPrefab();
        obj.transform.position = spawnLocation + randomVector;
        obj.SetActive(true);

        obj.GetComponent<Rigidbody>().AddExplosionForce(power, spawnLocation, radius, upforce, ForceMode.Impulse);
    }


}
