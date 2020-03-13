using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune
{
    public static Rune Instance;

    private string displayName;
    private string functionName;
    private string description;
    private int categoryId;
    private int chargeAmount;

    public string FunctionName
    {
        get
        {
            return functionName;
        }
    }

    public string DisplayName
    {
        get
        {
            return displayName;
        }
    }

    public int ChargeAmount
    {
        get
        {
            return chargeAmount;
        }
    }

    public Rune(string _displayName, string _functionName, string _description, int _categoryId, int _chargeAmount)
    {
        displayName = _displayName;
        functionName = _functionName;
        description = _description;
        categoryId = _categoryId;
        chargeAmount = _chargeAmount;
    }

}
