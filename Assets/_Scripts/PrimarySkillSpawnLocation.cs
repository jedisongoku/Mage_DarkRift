using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimarySkillSpawnLocation : MonoBehaviour
{
    [SerializeField] private GameObject primarySkillSpawnLocation;


    private void Start()
    {
        transform.root.GetComponent<PlayerCombatManager>().PrimarySkillSpawnLocation = primarySkillSpawnLocation;
    }
}
