using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [SerializeField] private GameObject primarySkillPrefab;
    [SerializeField] private GameObject primarySkillExplosionPrefab;
    [SerializeField] private GameObject primarySkillFrostNovaPrefab;
    [SerializeField] private int primarySkillPrefabPooledAmount = 2;
    [SerializeField] private int primarySkillExplosionPrefabPooledAmount = 2;
    [SerializeField] private int primarySkillFrostNovaPrefabPooledAmount = 1;

    List<GameObject> primarySkillPrefabList;
    List<GameObject> primarySkillExplosionPrefabList;
    List<GameObject> primarySkillFrostNovaPrefabList;

    bool willGrow = true;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    public void GenerateParticles()
    {
        Debug.Log("Count " + ClientManager.Instance.TotalPlayer);
        primarySkillPrefabList = new List<GameObject>();
        primarySkillExplosionPrefabList = new List<GameObject>();
        primarySkillFrostNovaPrefabList = new List<GameObject>();

        //Primary Skill Particle Instantiation 
        for (int i = 0; i < primarySkillPrefabPooledAmount * ClientManager.Instance.TotalPlayer; i++)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillPrefab);
            obj.SetActive(false);
            primarySkillPrefabList.Add(obj);
        }

        //Primary Skill OnHit Particle Instantiation 
        for (int i = 0; i < primarySkillExplosionPrefabPooledAmount * ClientManager.Instance.TotalPlayer; i++)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillExplosionPrefab);
            obj.SetActive(false);
            primarySkillExplosionPrefabList.Add(obj);
        }

        //Primary Skill OnHit FrostNova Particle Instantiation 
        for (int i = 0; i < primarySkillFrostNovaPrefabPooledAmount * ClientManager.Instance.TotalPlayer; i++)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillFrostNovaPrefab);
            obj.SetActive(false);
            primarySkillFrostNovaPrefabList.Add(obj);
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

    public GameObject GetPrimarySkillFrostNovaPrefab()
    {
        for (int i = 0; i < primarySkillFrostNovaPrefabList.Count; i++)
        {
            if (!primarySkillFrostNovaPrefabList[i].activeInHierarchy)
            {
                return primarySkillFrostNovaPrefabList[i];
            }
        }

        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(primarySkillFrostNovaPrefab);
            primarySkillFrostNovaPrefabList.Add(obj);
            return obj;
        }

        return null;
    }
}
