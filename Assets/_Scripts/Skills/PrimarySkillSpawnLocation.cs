using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimarySkillSpawnLocation : MonoBehaviour
{
    [SerializeField] private GameObject primarySkillSpawnLocation;


    private void Start()
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "PoisonIsland" )
        {
            transform.root.GetComponent<PlayerCombatManager>().primarySkillSpawnLocation = primarySkillSpawnLocation;
        }
        
    }


}
