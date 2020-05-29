#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabLoginManager : MonoBehaviour
{
    public static PlayFabLoginManager instance;

    public bool isPlayFabNextCallSuccess;
    public bool isPlayFabNextCallFailed;

    public static int callCounter = 0;

    bool customLogin;


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
#if UNITY_ANDROID
        if(Application.platform == RuntimePlatform.Android)
        {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .AddOauthScope("profile")
        .RequestServerAuthCode(false)
        .Build();
            PlayGamesPlatform.InitializeInstance(config);

            // recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;

            // Activate the Google Play Games platform
            PlayGamesPlatform.Activate();
            
        }       
#endif
        
        Social.localUser.Authenticate(success => {
            if (success)
            {
                Debug.Log("Social success");
                customLogin = false;
                LoginPlayFab();
                //if (PlayFabApiCalls.isNewUser) PlayFabApiCalls.instance.LinkGameAccount();
            }   
            else
            {
                Debug.Log("Failed to social authenticate");
                customLogin = true;
                LoginPlayFab();
            }
                
                
        });
    }

    

    public void LoginPlayFab()
    {
        
        StartCoroutine(Login(customLogin));
    }

    IEnumerator Login(bool custom)
    {
        if(custom)
        {
            isPlayFabNextCallFailed = false;
            isPlayFabNextCallSuccess = false;
            Debug.Log("Authentication with Custom ID");
            PlayFabApiCalls.instance.AuthenticateWithCustomID();
        }
        else
        {
#if UNITY_ANDROID
            PlayFabApiCalls.instance.AuthenticateWithGoogle();
#else
        PlayFabApiCalls.instance.AuthenticateWithAppleGameCenter();
#endif 
        }


        yield return new WaitUntil(() => (isPlayFabNextCallSuccess == true || isPlayFabNextCallFailed == true));

        
        if(isPlayFabNextCallSuccess)
        {
            isPlayFabNextCallSuccess = false;
            StartCoroutine(DownloadContent());
        }
        else
        {
            StartCoroutine(Login(true));
        }
    }

    IEnumerator DownloadContent()
    {
        
        if (PlayFabApiCalls.isNewUser)
        {
            PlayFabApiCalls.instance.CreateNewProfile();
            if(!customLogin)
            {
                PlayFabApiCalls.instance.LinkCustomId();
            }
            /*
            PlayerPrefs.SetInt("Music", 1);
            PlayerPrefs.SetInt("SoundEffect", 1);*/
            //if(Social.localUser.authenticated) PlayFabApiCalls.instance.LinkGameAccount();

        }
        else
        {
            PlayFabApiCalls.instance.GetUserData();
            /*
            if(!PlayerPrefs.HasKey("Music"))
            {
                PlayerPrefs.SetInt("Music", 1);
                PlayerPrefs.SetInt("SoundEffect", 1);
            }*/
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
        HUDManager.Instance.UpdateCurrencies();




    }

    public void IncrementCallCounter()
    {
        callCounter++;
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
