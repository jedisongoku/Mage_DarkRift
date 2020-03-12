using System;
using System.Collections.Generic;


[Serializable]
public class GameDataModel
{
    public string PlayerName { get; set; }
    public int GemBalance { get; set; }
    public int CoinBalance { get; set; }
    public int EnergyBalance { get; set; }

    public string GameMode { get; set; }
}
