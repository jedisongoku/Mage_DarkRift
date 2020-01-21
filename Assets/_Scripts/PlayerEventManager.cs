using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventManager : MonoBehaviour
{

    //When a player kills an enemy player
    public delegate void KillAction();
    public static event KillAction OnPlayerKilled;

    //When a player is respawned
    public delegate void RespawnAction();
    public static event RespawnAction OnPlayerRespawned;

    //When a player is dead
    public delegate void DeathAction();
    public static event DeathAction OnPlayerDeath;
    /*
    public static void Deaths()
    {
        if(OnPlayerDeath != null)
        {
            OnPlayerDeath();
        }
        
    }

    public static void Kill()
    {
        if(OnPlayerKilled != null)
        {
            OnPlayerKilled();
        }
        
    }*/
}
