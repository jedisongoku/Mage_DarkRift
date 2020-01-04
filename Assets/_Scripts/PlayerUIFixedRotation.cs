using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIFixedRotation : MonoBehaviour
{

    private GameObject parent;
    private Vector3 position = new Vector3(0, 2.5f, 0);
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.SetParent(null);
        transform.rotation = new Quaternion(0, 0, 0, 0);
        transform.SetParent(parent.transform);
        transform.localPosition = position;
    }
}
