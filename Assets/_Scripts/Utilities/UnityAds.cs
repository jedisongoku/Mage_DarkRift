using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
public class UnityAds : MonoBehaviour
{
    public static UnityAds instance;

#if UNITY_IOS
        private string gameId = "3522884";
#elif UNITY_ANDROID
    private string gameId = "3522885";
#endif

    bool testMode = true;

    void Start()
    {
        instance = this;
        //watchAdsForSpeed = GetComponent<Button>();

        // Set interactivity to be dependent on the Placement’s status:
        //watchAdsForSpeed.interactable = Advertisement.IsReady(myPlacementId);

        // Map the ShowRewardedVideo function to the button’s click listener:
        //if (watchAdsForSpeed) watchAdsForSpeed.onClick.AddListener(ShowRewardedVideo);

        Advertisement.Initialize(gameId, true);
    }


    public void RefillEnergyAds(string placementId)
    {
        // If the ready Placement is rewarded, activate the button: 
        if (Advertisement.IsReady())
        {
            Advertisement.Show(placementId);
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            // Reward the user for watching the ad to completion.
        }
        else if (showResult == ShowResult.Skipped)
        {
            // Do not reward the user for skipping the ad.
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.LogWarning("The ad did not finish due to an error.");
        }
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