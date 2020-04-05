using System.Collections;
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

        
    }

    void Start()
    {
        Social.localUser.Authenticate(success => {
            if (success)
                Debug.Log("Social success");
            else
                Debug.Log("Failed to social authenticate");
        });

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
            isPlayFabNextCallSuccess = false;
            StartCoroutine(DownloadContent());
        }
    }

    IEnumerator DownloadContent()
    {
        
        if (PlayFabApiCalls.isNewUser)
        {
            PlayFabApiCalls.instance.CreateNewProfile();
            if(Social.localUser.authenticated) PlayFabApiCalls.instance.LinkGameAccount();

        }
        else
        {
            PlayFabApiCalls.instance.GetUserData();
        }

        yield return new WaitUntil(() => (isPlayFabNextCallSuccess || isPlayFabNextCallFailed));
        HUDManager.Instance.isLoginSuccess = true;
        callCounter = 0;
        int callsToWait = 3;
        PlayFabApiCalls.instance.GetCatalogItems();
        PlayFabApiCalls.instance.GetPlayerBaseStats();
        PlayFabApiCalls.instance.GetPlayerInventory();
        
        

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
