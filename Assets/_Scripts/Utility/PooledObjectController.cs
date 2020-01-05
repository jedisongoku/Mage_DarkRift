using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObjectController : MonoBehaviour
{
    [SerializeField] private float explosionDestroyTime = 1.5f;

    private void OnEnable()
    {
        Invoke("Destroy", explosionDestroyTime);
    }

    private void Destroy()
    {
        gameObject.SetActive(false);
    }
}
