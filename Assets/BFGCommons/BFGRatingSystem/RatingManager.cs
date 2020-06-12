using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles game rating, pops Rating and response panels, fires off event on rating
/// 
/// Ruben Sanchez
/// 12/19/18
/// </summary>
public class RatingManager : MonoBehaviour
{
    public static RatingManager Instance;

    public Button SubmitButton;
    public Transform GoldStarParent;

    public delegate void Rating(int starAmount);
    public event Rating OnRating;

    [HideInInspector]
    public int StarAmount;

    [SerializeField] private GameObject ratePanel;
    [SerializeField] private GameObject lowRatingResponsePanel;
    [SerializeField] private GameObject highRatingResponsePanel;
    [SerializeField] private int highRatingThreshold = 4;

    [SerializeField] private GameObject[] objectsToDeactivate;

    private string storeLink;
    private const string ratingPref = "CanShowRating";

    private void Awake()
    {
        storeLink = "https://apps.apple.com/us/app/mage-io-magic-battle-arena/id1505443765";
#if UNITY_ANDROID
        storeLink = "https://play.google.com/store/apps/details?id=" + Application.identifier;
#endif


        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        else
            Destroy(gameObject);
    }

    public void ShowRatingPanel()
    {
        /*if (PlayerPrefs.GetString(ratingPref, "true") == "false")
            return;*/

        foreach (var o in objectsToDeactivate)
            o.SetActive(false);

        ratePanel.SetActive(true);
    }

    public void HandleRating()
    {
        if (StarAmount >= highRatingThreshold)
            highRatingResponsePanel.SetActive(true);

        else
            lowRatingResponsePanel.SetActive(true);

        if (OnRating != null)
            OnRating(StarAmount);

        PlayerPrefs.SetInt("Rating", StarAmount);
    }

    public void OpenStoreLink()
    {
        Application.OpenURL(storeLink);
    }
    /*
    public void ShowRatingBasedOnHighScore(int minHighScoreToRate)
    {
        if (ScoreCurrencyManager.Instance.BestScore >= minHighScoreToRate)
            ShowRatingPanel();
    }*/

    public void DisableRating()
    {
        PlayerPrefs.SetString(ratingPref, "false");
    }
}
