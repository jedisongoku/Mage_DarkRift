﻿using UnityEngine;
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
        instance = this;
        // Initialize the Ads listener and service:
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, true);
    }

    // Implement a function for showing a rewarded video ad:
    public void RefillEnergyAd()
    {
        PlayFabApiCalls.instance.GetVirtualCurrency_Energy();
        Photon.Pun.PhotonNetwork.KeepAliveInBackground = 60;
        myPlacementId = "RefillEnergy";
        Advertisement.Show(myPlacementId);
    }

    public void CoinRewardAd()
    {
        Photon.Pun.PhotonNetwork.KeepAliveInBackground = 60;
        myPlacementId = "CoinReward";
        Advertisement.Show(myPlacementId);
    }

    public void GemRewardAd()
    {
        Photon.Pun.PhotonNetwork.KeepAliveInBackground = 60;
        myPlacementId = "GemReward";
        Advertisement.Show(myPlacementId);
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
                PlayFabApiCalls.instance.AddVirtualCurrency(50 - PlayFabDataStore.vc_energy, "EN");
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Deathmatch") HUDManager.Instance.ShowDeathPanel();
            }
            else if(placementId == "CoinReward")
            {
                PlayFabApiCalls.instance.AddVirtualCurrency(50, "CO");
            }
            else if(placementId == "GemReward")
            {
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