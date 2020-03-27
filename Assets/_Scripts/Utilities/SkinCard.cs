using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinCard : MonoBehaviour
{
    [SerializeField] public Image skinImage;
    
    string skinName;
    int cost;
    bool isOwned = false;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnSkinClicked);
        skinImage = transform.Find("Image/Image").GetComponent<Image>();
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        skinName = skinImage.sprite.name;
        if(PlayFabDataStore.playerSkins.ContainsKey(skinName))
        {
            transform.Find("Unlocked").gameObject.SetActive(false);
            isOwned = true;
            cost = PlayFabDataStore.playerSkins[skinName].cost;
        }


        
    }

    public void OnSkinClicked()
    {
        HUDManager.Instance.OnSkinPreviewed(isOwned, skinName);
    }
}
