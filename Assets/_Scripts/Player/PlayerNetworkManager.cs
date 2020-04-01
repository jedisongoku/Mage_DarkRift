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

    // Start is called before the first frame update
    void Start()
    {
        SetPlayerUI();
        if(photonView.IsMine)
        {
            photonView.RPC("SelectSkin", RpcTarget.All, PlayFabDataStore.playerActiveSkin, false);
            GetComponent<AudioListener>().enabled = true;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Spawn GEM 1");
            StartCoroutine(SpawnGems());
            previousLocation = CartController.instance.GetCartLocation;
        }
    }

    void SetPlayerUI()
    {
        playerNameText.text = photonView.Owner.NickName;
    }

    [PunRPC]
    void SelectSkin(string _skinName, bool _isDead)
    {
        GameObject skin = Instantiate(Resources.Load("Skins/" + _skinName) as GameObject, playerSkinParent.transform);
        GetComponent<PlayerCombatManager>().PlayerModel = skin;
        GetComponent<PlayerHealthManager>().PlayerModel = skin;
        playerSkinParent.GetComponent<Animator>().Rebind();
        if(_isDead)
        {
            GetComponent<PlayerCombatManager>().DisablePlayer();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(photonView.IsMine)
        {
            photonView.RPC("SelectSkin", newPlayer, PlayFabDataStore.playerActiveSkin, GetComponent<PlayerCombatManager>().IsDead);
            photonView.RPC("StartCartAnimation", newPlayer, CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Forward"),
                CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime, CartController.instance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length,
                PhotonNetwork.ServerTimestamp);
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
        Debug.Log("Spawn GEM 2");

        gemSpawnTime = Random.Range(3f, 5f);

        yield return new WaitForSeconds(gemSpawnTime);

        int random = Random.Range(-100, 100);
        random = random >= 0 ? 1 : -1;
        Vector3 randomVector = new Vector3(0, 0, random);

        //GameObject obj = PhotonNetwork.Instantiate(gem.name, gemSpawnLocation.position + randomVector, Quaternion.identity);

        int power = Random.Range(7, 12);
        //obj.GetComponent<Rigidbody>().AddExplosionForce(power, gemSpawnLocation.position, radius, upforce, ForceMode.Impulse);
        if ((previousLocation - CartController.instance.GetCartLocation).magnitude > 1)
        {
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
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnGems());
            previousLocation = transform.position;
        }
    }

    [PunRPC]
    void SpawnGem(int power, Vector3 spawnLocation, Vector3 randomVector)
    {
        Debug.Log("Spawn GEM 3");
        //GameObject obj = Instantiate(CartController.instance.GetGemObject, spawnLocation + randomVector, Quaternion.identity);
        GameObject obj = ObjectPooler.Instance.GetGemPrefab();
        obj.transform.position = spawnLocation + randomVector;
        obj.SetActive(true);
        obj.GetComponent<GemPickup>().AddToList();

        obj.GetComponent<Rigidbody>().AddExplosionForce(power, spawnLocation, radius, upforce, ForceMode.Impulse);
    }


}
