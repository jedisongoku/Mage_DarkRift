using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonShopManager : MonoBehaviour
{
    public static PoisonShopManager Instance;

    public GameObject[] spawnLocations;

    private void Awake()
    {
        Instance = this;
    }

}
