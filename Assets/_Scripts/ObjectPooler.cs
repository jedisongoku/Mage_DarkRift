using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [SerializeField] private GameObject primarySkillPrefab;
    [SerializeField] private GameObject primarySkillExplosionPrefab;
    [SerializeField] private GameObject primarySkillEnvironmentExplosionPrefab;
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int primarySkillPrefabPooledAmount = 2;
    [SerializeField] private int primarySkillExplosionPrefabPooledAmount = 2;
    [SerializeField] private int primarySkillEnvironmentExplosionPrefabPooledAmount = 2;
    [SerializeField] private int gemPrefabPooledAmount = 10;

    List<GameObject> primarySkillPrefabList;
    List<GameObject> primarySkillExplosionPrefabList;
    List<GameObject> primarySkillEnvironmentExplosionPrefabList;
    List<GameObject> gemPrefabList;

    bool willGrow = true;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        primarySkillPrefabList = new List<GameObject>();
        primarySkillExplosionPrefabList = new List<GameObject>();
        primarySkillEnvironmentExplosionPrefabList = new List<GameObject>();
        gemPrefabList = new List<GameObject>();


        //Primary Skill Particle Instantiation 
        for (int i = 0; i < primarySkillPrefabPooledAmount * PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillPrefab);
            obj.SetActive(false);
            primarySkillPrefabList.Add(obj);
        }

        //Primary Skill OnHit Particle Instantiation 
        for (int i = 0; i < primarySkillExplosionPrefabPooledAmount * PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillExplosionPrefab);
            obj.SetActive(false);
            primarySkillExplosionPrefabList.Add(obj);
        }

        //Primary Skill OnHit FrostNova Particle Instantiation 
        for (int i = 0; i < primarySkillEnvironmentExplosionPrefabPooledAmount * PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillEnvironmentExplosionPrefab);
            obj.SetActive(false);
            primarySkillEnvironmentExplosionPrefabList.Add(obj);
        }

        //Gem Instantiation 
        for (int i = 0; i < gemPrefabPooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(gemPrefab);
            obj.SetActive(false);
            gemPrefabList.Add(obj);
        }
    }

    public GameObject GetPrimarySkillPrefab()
    {
        for (int i = 0; i < primarySkillPrefabList.Count; i++)
        {
            if (!primarySkillPrefabList[i].activeInHierarchy)
            {
                return primarySkillPrefabList[i];
            }
        }

        if(willGrow)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillPrefab);
            primarySkillPrefabList.Add(obj);
            return obj;
        }

        return null;
    }

    public GameObject GetPrimarySkillExplosionPrefab()
    {
        for (int i = 0; i < primarySkillExplosionPrefabList.Count; i++)
        {
            if (!primarySkillExplosionPrefabList[i].activeInHierarchy)
            {
                return primarySkillExplosionPrefabList[i];
            }
        }

        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillExplosionPrefab);
            primarySkillExplosionPrefabList.Add(obj);
            return obj;
        }

        return null;
    }

    public GameObject GetPrimarySkillEnvironmentExplosionPrefab()
    {
        for (int i = 0; i < primarySkillEnvironmentExplosionPrefabList.Count; i++)
        {
            if (!primarySkillEnvironmentExplosionPrefabList[i].activeInHierarchy)
            {
                return primarySkillEnvironmentExplosionPrefabList[i];
            }
        }

        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillEnvironmentExplosionPrefab);
            primarySkillEnvironmentExplosionPrefabList.Add(obj);
            return obj;
        }

        return null;
    }

    public GameObject GetGemPrefab()
    {
        for (int i = 0; i < gemPrefabList.Count; i++)
        {
            if (!gemPrefabList[i].activeInHierarchy)
            {
                return gemPrefabList[i];
            }
        }

        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(gemPrefab);
            gemPrefabList.Add(obj);
            return obj;
        }

        return null;
    }
}
