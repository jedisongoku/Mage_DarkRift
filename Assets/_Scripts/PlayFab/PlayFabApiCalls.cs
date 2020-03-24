using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PlayFabApiCalls : MonoBehaviour
{
    public static PlayFabApiCalls instance;
    public static bool isNewUser;

    public delegate void OnApiCallSuccessEvent();
    public static event OnApiCallSuccessEvent OnApiCallSuccess;

    public delegate void OnApiCallFailEvent();
    public static event OnApiCallFailEvent OnApiCallFail;


    // Start is called before the first frame update
    void Awake()
    {
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
    #region Player Login
    public void AuthenticateWithCustomID()
    {
        var request = new LoginWithCustomIDRequest()
        {
            CreateAccount = true,
            CustomId = PlayFabSettings.DeviceUniqueIdentifier,
        };

        PlayFabClientAPI.LoginWithCustomID(request, (result) =>
        {
            isNewUser = result.NewlyCreated;
            PlayFabDataStore.playFabID = result.PlayFabId;
            RequestPhotonToken(result);
        }, (error) =>
         {
             //Error
             ApiCallFail();
             OnPlayFabError(error);
         });
    }

    private void RequestPhotonToken(LoginResult obj)
    {
        LogMessage("PlayFab authenticated. Requesting photon token...");
        //We can player PlayFabId. This will come in handy during next step
        PlayFabDataStore.playFabID = obj.PlayFabId;

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
        }, AuthenticateWithPhoton, OnPlayFabError);
    }

    private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        LogMessage("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

        //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
        var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
        //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
        customAuth.AddAuthParameter("username", PlayFabDataStore.playFabID);    // expected by PlayFab custom auth service

        //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
        customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

        //We finally tell Photon to use this authentication parameters throughout the entire application.
        PhotonNetwork.AuthValues = customAuth;
        ApiCallSuccess();
    }


    #endregion

    #region GameContent
    public void GetPlayerBaseStats()
    {
        var request = new GetTitleDataRequest()
        {
            Keys = new List<string>{ "PlayerBaseStats" }
        };
        PlayFabClientAPI.GetTitleData(request, (result) =>
        {
            PlayFabDataStore.playerBaseStats = JsonUtility.FromJson<PlayerBaseStats>(result.Data["PlayerBaseStats"]);
            PlayFabLoginManager.instance.IncrementCallCounter();

        }, (error) =>
        {
            OnPlayFabError(error);
            ApiCallFail();
        });
    }
    #endregion

    #region Virtual Currency
    public void GetVirtualCurrency_Gems()
    {
        var request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "GM",
            Amount = 0

        };
        PlayFabClientAPI.AddUserVirtualCurrency(request, (result) =>
        {
            PlayFabDataStore.vc_gems = result.Balance;
            PlayFabLoginManager.instance.IncrementCallCounter();

        }, (error) =>
        {
            ApiCallFail();
            OnPlayFabError(error);
        });
    }

    public void GetVirtualCurrency_Coins()
    {
        var request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "CO",
            Amount = 0

        };
        PlayFabClientAPI.AddUserVirtualCurrency(request, (result) =>
        {
            PlayFabDataStore.vc_coins = result.Balance;
            PlayFabLoginManager.instance.IncrementCallCounter();

        }, (error) =>
        {
            //Error
            ApiCallFail();
            OnPlayFabError(error);
        });
    }

    public void GetVirtualCurrency_Energy()
    {
        var request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "EN",
            Amount = 0

        };
        PlayFabClientAPI.AddUserVirtualCurrency(request, (result) =>
        {
            PlayFabDataStore.vc_energy = result.Balance;
            PlayFabLoginManager.instance.IncrementCallCounter();


        }, (error) =>
        {
            ApiCallFail();
            OnPlayFabError(error);
        });
    }

    #endregion

    public void GetVirtualCurrency(string currencyCode)
    {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddVirtualCurrency",
            FunctionParameter = new { code = currencyCode}
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            //Result
        }, (error) =>
        {
            ApiCallFail();
            OnPlayFabError(error);

        });
    }

    public void CreateNewProfile()
    {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "CreateNewProfile",
            FunctionParameter = false
            
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            //Result
            GetUserData();
        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }

    public void GetUserData()
    {
        var request = new GetUserDataRequest()
        {
            Keys = new List<string> { "PlayerProfile" }

        };
        PlayFabClientAPI.GetUserData(request, (result) =>
        {
            //Result
            PlayFabDataStore.playerProfile = JsonUtility.FromJson<PlayerProfile>(result.Data["PlayerProfile"].Value);
            PlayFabDataStore.playerName = PlayFabDataStore.playerProfile.playerName;
            PlayFabLoginManager.instance.IncrementCallCounter();

        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }



    private void OnPlayFabError(PlayFabError obj)
    {
        LogMessage(obj.GenerateErrorReport());
    }

    public void LogMessage(string message)
    {
        Debug.Log("PlayFab + Photon Example: " + message);
    }

    private void ApiCallSuccess()
    {
        OnApiCallSuccess?.Invoke();
    }

    private void ApiCallFail()
    {
        OnApiCallFail?.Invoke();
    }
}
