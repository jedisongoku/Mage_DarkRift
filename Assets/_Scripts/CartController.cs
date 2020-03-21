using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CartController : MonoBehaviourPunCallbacks
{

    public static CartController instance;


    private void Start()
    {
        instance = this;
    }

}
