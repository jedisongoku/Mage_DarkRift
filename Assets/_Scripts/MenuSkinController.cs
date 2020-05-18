﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSkinController : MonoBehaviour
{
    public static MenuSkinController instance;
    public static int rewardsPlacement = 0;
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
        if(rewardsPlacement == 0)
        {
            activeSkin.GetComponent<Animator>().SetTrigger("Welcome");
        }
        else
        {
            PlayRewardsAnimation(rewardsPlacement);
        }
        
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
        activeSkin.GetComponent<Animator>().SetTrigger("Wave");
    }

    public void PlaySelectAnimation()
    {
        activeSkin.GetComponent<Animator>().SetTrigger("Welcome");
    }

    public void PlayUnlockAnimation()
    {
        activeSkin.GetComponent<Animator>().SetTrigger("Unlock");
    }

    public void PlayRewardsAnimation(int placement)
    {
        if(placement == 1)
        {
            activeSkin.GetComponent<Animator>().SetTrigger("Victory");
        }
        else if(placement < 4)
        {
            activeSkin.GetComponent<Animator>().SetTrigger("Clapping");
        }
        else
        {
            activeSkin.GetComponent<Animator>().SetTrigger("Crying");
        }
        rewardsPlacement = 0;
    }


    public GameObject GetSkin()
    {
        return activeSkin;
    }
}
