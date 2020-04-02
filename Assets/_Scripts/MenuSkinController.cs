using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSkinController : MonoBehaviour
{
    public static MenuSkinController instance;
    [SerializeField] RuntimeAnimatorController controller;
    [SerializeField] Avatar avatar;

    public GameObject activeSkin;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //SwitchSkin();
    }

    private void OnEnable()
    {

        SwitchSkin();
    }

    public void SwitchSkin()
    {
        if (activeSkin != null)
        {
            if (activeSkin.name != PlayFabDataStore.playerProfile.skinName)
            {
                Destroy(activeSkin);
                Debug.Log("Skin name " + PlayFabDataStore.playerProfile.skinName);
                activeSkin = Instantiate(Resources.Load("Skins/" + PlayFabDataStore.playerProfile.skinName) as GameObject, transform);
            }

            //Instantiate(skin, transform);  
        }
        else
        {
            activeSkin = Instantiate(Resources.Load("Skins/" + PlayFabDataStore.playerProfile.skinName) as GameObject, transform);
            
        }
        activeSkin.AddComponent<Animator>();
        activeSkin.GetComponent<Animator>().runtimeAnimatorController = controller;
        activeSkin.GetComponent<Animator>().avatar = avatar;
        //activeSkin.GetComponent<Animator>().Rebind();
    }

    public void ShowcaseSkin(string name)
    {
        if (activeSkin != null)
        {
            if (activeSkin.name != name)
            {
                Destroy(activeSkin);

                activeSkin = Instantiate(Resources.Load("Skins/" + name) as GameObject, transform);
            }

            //Instantiate(skin, transform);  
        }
        else
        {
            activeSkin = Instantiate(Resources.Load("Skins/" + name) as GameObject, transform);
        }
        activeSkin.name = name;
        activeSkin.AddComponent<Animator>();
        activeSkin.GetComponent<Animator>().runtimeAnimatorController = controller;
        activeSkin.GetComponent<Animator>().avatar = avatar;
    }


    public GameObject GetSkin()
    {
        return activeSkin;
    }
}
