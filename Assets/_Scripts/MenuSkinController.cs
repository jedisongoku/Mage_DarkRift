using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSkinController : MonoBehaviour
{
    [SerializeField] GameObject skin;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(skin, transform);
        GetComponent<Animator>().Rebind();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
