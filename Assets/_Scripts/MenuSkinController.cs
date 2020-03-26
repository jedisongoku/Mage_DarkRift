using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSkinController : MonoBehaviour
{
    public static MenuSkinController instance;
    [SerializeField] GameObject skin;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Instantiate(skin, transform);
        
        GetComponent<Animator>().Rebind();
    }

    public GameObject GetSkin()
    {
        return skin;
    }
}
