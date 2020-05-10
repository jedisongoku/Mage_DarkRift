using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
public class UnityAds : MonoBehaviour, IUnityAdsListener
{
    public static UnityAds instance;

#if UNITY_IOS
        private string gameId = "3522884";
#elif UNITY_ANDROID
    private string gameId = "3522885";
#endif

    bool testMode = true;
    string myPlacementId = "rewardedVideo";

    void Start()
    {
        Application.runInBackground = true;
        instance = this;
        // Initialize the Ads listener and service:
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, true);
    }

    // Implement a function for showing a rewarded video ad:
    public void RefillEnergyAd()
    {
        if(PlayFabDataStore.vc_energy < 50)
        {
            PlayFabApiCalls.instance.GetVirtualCurrency_Energy();
            Photon.Pun.PhotonNetwork.KeepAliveInBackground = 60;
            myPlacementId = "RefillEnergy";
            Advertisement.Show(myPlacementId);
        }
        
    }

    public void CoinRewardAd()
    {
        if(PlayFabDataStore.vc_adCoin > 0)
        {
            Photon.Pun.PhotonNetwork.KeepAliveInBackground = 60;
            myPlacementId = "CoinReward";
            Advertisement.Show(myPlacementId);
            PlayFabApiCalls.instance.SubtractVirtualCurrency(1, "AC");
        }
        
    }

    public void GemRewardAd()
    {
        if(PlayFabDataStore.vc_adGem > 0)
        {
            Photon.Pun.PhotonNetwork.KeepAliveInBackground = 60;
            myPlacementId = "GemReward";
            Advertisement.Show(myPlacementId);
            PlayFabApiCalls.instance.SubtractVirtualCurrency(1, "AG");
        }
        
    }

    // Implement IUnityAdsListener interface methods:
    public void OnUnityAdsReady(string placementId)
    {
        
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            Debug.Log("Ad finished");
            // Reward the user for watching the ad to completion.
            if(placementId == "RefillEnergy")
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Deathmatch") HUDManager.Instance.ShowDeathPanel();
                else HUDManager.Instance.PlayEnergyAnimation(0, PlayFabDataStore.vc_energy, 50 - PlayFabDataStore.vc_energy);
                PlayFabApiCalls.instance.AddVirtualCurrency(50 - PlayFabDataStore.vc_energy, "EN");
                
            }
            else if(placementId == "CoinReward")
            {
                HUDManager.Instance.UpdateRemainingDailyAds();
                HUDManager.Instance.PlayCoinAnimation(0, PlayFabDataStore.vc_coins, 50);
                PlayFabApiCalls.instance.AddVirtualCurrency(50, "CO");
            }
            else if(placementId == "GemReward")
            {
                HUDManager.Instance.UpdateRemainingDailyAds();
                HUDManager.Instance.PlayGemAnimation(6, PlayFabDataStore.vc_gems, 5);
                PlayFabApiCalls.instance.AddVirtualCurrency(5, "GM");

            }
            
        }
        else if (showResult == ShowResult.Skipped)
        {
            // Do not reward the user for skipping the ad.
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.LogWarning("the ad did not finish");
        }

        Photon.Pun.PhotonNetwork.KeepAliveInBackground = 0;
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}