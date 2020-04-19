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
            CustomId = PlayFabSettings.DeviceUniqueIdentifier
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

    public void LinkGameAccount()
    {
#if UNITY_ANDROID
        LinkGooglePlay();
#else
        LinkGameCenter();
#endif

    }

    public void LinkGameCenter()
    {
        var request = new LinkGameCenterAccountRequest()
        {
            GameCenterId = Social.localUser.id
        };
        PlayFabClientAPI.LinkGameCenterAccount(request, (result) =>
        {
            PlayFabDataStore.playerProfile.playerName = Social.localUser.userName;
            UpdateProfile();
            Debug.Log("Game center linked");

        }, (error) =>
        {
            OnPlayFabError(error);
        });
    }
#if UNITY_ANDROID
    public void LinkGooglePlay()
    {
        var request = new LinkGoogleAccountRequest()
        {
            ServerAuthCode = GooglePlayGames.PlayGamesPlatform.Instance.GetServerAuthCode()
        };
        PlayFabClientAPI.LinkGoogleAccount(request, (result) =>
        {
            PlayFabDataStore.playerProfile.playerName = Social.localUser.userName;
            UpdateProfile();
            Debug.Log("Google Account linked");

        }, (error) =>
        {
            OnPlayFabError(error);
        });
    }
#endif

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

    public void AddVirtualCurrency(int currencyAmount, string currencyCode)
    {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddVirtualCurrency",
            FunctionParameter = new { currencyAmount = currencyAmount, currencyCode = currencyCode}
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            //Result
            GetPlayerInventory();

        }, (error) =>
        {
            ApiCallFail();
            OnPlayFabError(error);

        });
    }

    public void SubtractVirtualCurrency(int currencyAmount, string currencyCode)
    {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "SubtractVirtualCurrency",
            FunctionParameter = new { currencyAmount = currencyAmount, currencyCode = currencyCode}
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            if (currencyCode == "EN") PlayFabDataStore.vc_energy -= currencyAmount;
            else if (currencyCode == "CO") PlayFabDataStore.vc_coins -= currencyAmount;
            else if (currencyCode == "GM") PlayFabDataStore.vc_gems -= currencyAmount;
            //Result
        }, (error) =>
        {
            ApiCallFail();
            OnPlayFabError(error);

        });
    }

    public void CreateNewProfile()
    {
        string username;
        if(Social.localUser.authenticated)
        {
            username = Social.localUser.userName;
        }
        else
        {
            username = "Mage" + Random.Range(1000, 9999);
        }

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "CreateNewProfile",
            FunctionParameter = new { playerName = username }
            
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            //Result
            Debug.Log("NEW USER CREATED");
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
            UpdateUserDisplayName(PlayFabDataStore.playerProfile.playerName + "#");

            ApiCallSuccess();

        }, (error) =>
        {
            ApiCallFail();
            OnPlayFabError(error);

        });
    }

    public void GetPlayerInventory()
    {
        var request = new GetUserInventoryRequest()
        {

        };
        PlayFabClientAPI.GetUserInventory(request, (result) =>
        {
            //Result
            //PlayFabDataStore.playerProfile = JsonUtility.FromJson<PlayerProfile>(result.Data["PlayerProfile"].Value);
            PlayFabDataStore.playerSkins = new Dictionary<string, SkinModel>();
            foreach(var item in result.Inventory)
            {
                if(item.ItemClass == "Skin" && !PlayFabDataStore.playerSkins.ContainsKey(item.ItemId))
                {
                    PlayFabDataStore.playerSkins.Add(item.ItemId, new SkinModel(item.ItemId, item.DisplayName, item.UnitCurrency, (int)item.UnitPrice));
                }
            }

            PlayFabDataStore.vc_coins = result.VirtualCurrency["CO"];
            PlayFabDataStore.vc_gems = result.VirtualCurrency["GM"];
            PlayFabDataStore.vc_energy = result.VirtualCurrency["EN"];


            PlayFabLoginManager.instance.IncrementCallCounter();
            HUDManager.Instance.UpdateCurrencies();

        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }

    public void GetCatalogItems()
    {
        var request = new GetCatalogItemsRequest()
        {

        };
        PlayFabClientAPI.GetCatalogItems(request, (result) =>
        {
            foreach (var item in result.Catalog)
            {
                if (item.ItemClass == "Skin")
                {
                    string currencyUnity = item.VirtualCurrencyPrices.ContainsKey("CO") ? "CO" : "GM";
                    PlayFabDataStore.gameSkinCatalog.Add(item.ItemId, new SkinModel(item.ItemId, item.DisplayName, currencyUnity, (int)item.VirtualCurrencyPrices[currencyUnity]));
                }
            }
            PlayFabLoginManager.instance.IncrementCallCounter();

        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }

    public void PurchaseItem(string ItemID, int price, string currency)
    {
        Debug.Log("Attempting to buy skin");

        var request = new PurchaseItemRequest()
        {
            ItemId = ItemID,
            Price = price,
            VirtualCurrency = currency
            
        };
        PlayFabClientAPI.PurchaseItem(request, (result) =>
        {
            if(result.Items != null)
            {
                if(!PlayFabDataStore.playerSkins.ContainsKey(result.Items[0].ItemId))
                {
                    PlayFabDataStore.playerSkins.Add(result.Items[0].ItemId, new SkinModel(result.Items[0].ItemId, result.Items[0].DisplayName, result.Items[0].UnitCurrency, (int)result.Items[0].UnitPrice));
                }
                
            }
            HUDManager.Instance.OnSkinPreviewed(true, result.Items[0].ItemId);

        }, (error) =>
        {

            if (error.Error == PlayFabErrorCode.InsufficientFunds) HUDManager.Instance.InsufficientFunds_VC();
            OnPlayFabError(error);

        });
    }

    public void UpdateStatistics(string _name, int _kills)
    {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdateStatistics",
            FunctionParameter = new { name = _name, kills = _kills }

        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            //Result
            Debug.Log("Statistics Updated");
        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }

    public void UpdateProfile()
    {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdateProfile",
            FunctionParameter = new { 
                playerName = PlayFabDataStore.playerProfile.playerName,
                totalKills = PlayFabDataStore.playerProfile.totalKills,
                totalDeaths = PlayFabDataStore.playerProfile.totalDeaths,
                maxLevelReached = PlayFabDataStore.playerProfile.maxLevelReached,
                skinName = PlayFabDataStore.playerProfile.skinName
                }

    };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            //Result
            UpdateUserDisplayName(PlayFabDataStore.playerProfile.playerName + "#");
            HUDManager.Instance.UpdatePlayerName();
            Debug.Log("Profile Updated");
        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }

    public void UpdateUserDisplayName(string name)
    {
        if(name != null)
        {
            var request = new UpdateUserTitleDisplayNameRequest()
            {
                DisplayName = name
            };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result) =>
            {
                //Result
                Debug.Log("Display Name Updated");
            }, (error) =>
            {
                OnPlayFabError(error);

            });
        }
        
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest()
        {
            MaxResultsCount = 100,
            StatisticName = "Lifetime Kills",
            
        };
        PlayFabClientAPI.GetLeaderboard(request, (result) =>
        {
            //Result
            LeaderboardManager.Instance.RefreshLeaderboard(result.Leaderboard);
            Debug.Log("leaderboard retrieved");
        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }

    public void GetPlayerStatistics()
    {
        var request = new GetPlayerStatisticsRequest()
        {
            
        };
        PlayFabClientAPI.GetPlayerStatistics(request, (result) =>
        {
            foreach(var stat in result.Statistics)
            {
                if(!PlayFabDataStore.playerStatistics.ContainsKey(stat.StatisticName))
                {
                    PlayFabDataStore.playerStatistics.Add(stat.StatisticName, stat.Value);
                }
                else
                {
                    PlayFabDataStore.playerStatistics[stat.StatisticName] = stat.Value;
                }
            }

            HUDManager.Instance.RefreshProfileStatistics();

        }, (error) =>
        {
            OnPlayFabError(error);

        });
    }
    public void GetLeaderboardAroundPlayer()
    {
        var request = new GetLeaderboardAroundPlayerRequest()
        {
            StatisticName = "Lifetime Kills"
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, (result) =>
        {
            //Result
            foreach(var entry in result.Leaderboard)
            {
                if(entry.PlayFabId == PlayFabDataStore.playFabID)
                {
                    LeaderboardManager.Instance.RefreshPlayerRank(entry);
                }
                    
            }
            //LeaderboardManager.Instance.RefreshPlayerRank(result.Leaderboard.);
            Debug.Log("leaderboard retrieved");
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
