using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles behavior for rating star buttons
/// 
/// Ruben Sanchez
/// 12/21/18
/// </summary>
public class RatingStarButtonManager : MonoBehaviour
{

    private int index;

    private void Awake()
    {
        index = transform.GetSiblingIndex();

        GetComponent<Button>().onClick.AddListener(ActivateUpToIndex);
    }

    private void OnEnable()
    {
        RatingManager.Instance.SubmitButton.gameObject.SetActive(false);
        DeactivateUpToIndex(RatingManager.Instance.GoldStarParent.childCount - 1);
    }

    public void ActivateUpToIndex()
    {
        if (RatingManager.Instance.GoldStarParent.childCount == 0 || index >= RatingManager.Instance.GoldStarParent.childCount)
            return;

        RatingManager.Instance.SubmitButton.gameObject.SetActive(true);

        // deactivate all the stars first
        DeactivateUpToIndex(RatingManager.Instance.GoldStarParent.childCount - 1);

        // activate up to this index
        for (int i = 0; i <= index; i++)
        {
            RatingManager.Instance.GoldStarParent.GetChild(i).gameObject.SetActive(true);
        }

        // update the listener on the submit button
        UpdateSubmitButton();
    }

    private void DeactivateUpToIndex(int index)
    {
        if (RatingManager.Instance.GoldStarParent.childCount == 0 || index >= RatingManager.Instance.GoldStarParent.childCount)
            return;

        for (int i = 0; i <= index; i++)
        {
            RatingManager.Instance.GoldStarParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void UpdateSubmitButton()
    {
        // update the current star amount on the Rating Manager
        RatingManager.Instance.StarAmount = index + 1;

        // update the button listener
        RatingManager.Instance.SubmitButton.onClick.RemoveAllListeners();
        RatingManager.Instance.SubmitButton.onClick.AddListener(RatingManager.Instance.HandleRating);
    }
}
