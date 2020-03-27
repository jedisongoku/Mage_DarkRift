using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSkinController : MonoBehaviour
{
    public static MenuSkinController instance;
    [SerializeField] RuntimeAnimatorController controller;
    [SerializeField] Avatar avatar;

    GameObject activeSkin;
    GameObject showcasedSkin;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        SwitchSkin();
    }

    private void OnEnable()
    {
        

    }

    public void SwitchSkin()
    {
        if (activeSkin != null)
        {
            if (activeSkin.name != PlayFabDataStore.playerActiveSkin)
            {
                Destroy(activeSkin);
                Debug.Log("Skin name " + PlayFabDataStore.playerActiveSkin);
                activeSkin = Instantiate(Resources.Load("Skins/" + PlayFabDataStore.playerActiveSkin) as GameObject, transform);
            }

            //Instantiate(skin, transform);  
        }
        else
        {
            activeSkin = Instantiate(Resources.Load("Skins/" + PlayFabDataStore.playerActiveSkin) as GameObject, transform);
            
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

        activeSkin.AddComponent<Animator>();
        activeSkin.GetComponent<Animator>().runtimeAnimatorController = controller;
        activeSkin.GetComponent<Animator>().avatar = avatar;
    }


    public GameObject GetSkin()
    {
        return activeSkin;
    }
}
