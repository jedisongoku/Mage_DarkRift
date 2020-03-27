using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinModel : MonoBehaviour
{
    [SerializeField] public string itemID;
    [SerializeField] public string displayName;
    [SerializeField] public string currencyType;
    [SerializeField] public int cost;

    public SkinModel(string _itemID, string _displayName, string _currencyType, int _cost)
    {
        itemID = _itemID;
        displayName = _displayName;
        currencyType = _currencyType;
        cost = _cost;
    }
}
