using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIFixedRotation : MonoBehaviour
{
    public int x;
    public int y;
    public int z;
    public int t;

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
        /*
        transform.SetParent(null);
        transform.rotation = new Quaternion(x, y, z, t);
        transform.SetParent(parent.transform);
        transform.localPosition = position;*/
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
}
