using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CartController : MonoBehaviourPunCallbacks
{

    public static CartController instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartCart());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartCart()
    {
        photonView.RPC("StartCart_RPC", RpcTarget.AllViaServer);
        GetComponent<Animator>().SetTrigger("Start");

        yield return new WaitForSeconds(82);

        StartCoroutine(StartCart());

    }

    [PunRPC]
    void StartCart_RPC()
    {
        GetComponent<Animator>().SetTrigger("Start");
    }
}
