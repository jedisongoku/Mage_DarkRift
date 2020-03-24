﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabLoginManager : MonoBehaviour
{
    public static PlayFabLoginManager instance;

    public bool isPlayFabNextCallSuccess;
    public bool isPlayFabNextCallFailed;

    public static int callCounter = 0;


    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("1");
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        LoginPlayFab();
    }

    public void LoginPlayFab()
    {
        
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        PlayFabApiCalls.instance.AuthenticateWithCustomID();

        yield return new WaitUntil(() => (isPlayFabNextCallSuccess == true || isPlayFabNextCallFailed == true));

        if(isPlayFabNextCallSuccess)
        {
            StartCoroutine(DownloadContent());
        }
    }

    IEnumerator DownloadContent()
    {
        callCounter = 0;
        int callsToWait = 4;
        PlayFabApiCalls.instance.GetPlayerBaseStats();
        PlayFabApiCalls.instance.GetVirtualCurrency_Gems();
        PlayFabApiCalls.instance.GetVirtualCurrency_Coins();
        PlayFabApiCalls.instance.GetVirtualCurrency_Energy();
        if(PlayFabApiCalls.isNewUser)
        {
            callsToWait++;
            PlayFabApiCalls.instance.CreateNewProfile();
        }
        

        yield return new WaitUntil(() => (callCounter == callsToWait));

        PhotonNetworkManager.Instance.UpdatePlayerName();
        PhotonNetworkManager.Instance.ConnectToPhoton();




    }

    public void IncrementCallCounter()
    {
        callCounter++;
        Debug.Log("Counter :" + callCounter);
    }

    private void OnEnable()
    {

        PlayFabApiCalls.OnApiCallSuccess += OnApiCallSuccessHandler;
        PlayFabApiCalls.OnApiCallFail += OnApiCallFailHandler;
    }

    private void OnDisable()
    {
        PlayFabApiCalls.OnApiCallSuccess -= OnApiCallSuccessHandler;
        PlayFabApiCalls.OnApiCallFail -= OnApiCallFailHandler;
    }

    private void OnApiCallSuccessHandler()
    {
        isPlayFabNextCallSuccess = true;
    }

    private void OnApiCallFailHandler()
    {
        isPlayFabNextCallFailed = true;
    }
}
