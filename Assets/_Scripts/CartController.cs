using UnityEngine;

public class CartController : MonoBehaviour
{
    [SerializeField] GameObject gem;
    [SerializeField] Transform gemSpawnLocation;

    public static CartController instance;


    private void Start()
    {
        instance = this;
    }

    public GameObject GetGemObject
    {
        get
        {
            return gem;
        }
    }

    public Vector3 GetSpawnLocation
    {
        get
        {
            return gemSpawnLocation.position;
        }
    }

    public Vector3 GetCartLocation
    {
        get
        {
            return transform.position;
        }
    }



}
