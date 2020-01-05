using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public int ID { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        if(Equals(ServerManager.Instance, null) && ID == 0)
        {
            Destroy(gameObject);
        }
        else if(!Equals(ServerManager.Instance, null))
        {
            ID = GetInstanceID();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
