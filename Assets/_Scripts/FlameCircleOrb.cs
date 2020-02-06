using DarkRift.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameCircleOrb : MonoBehaviour
{
    [SerializeField] int rotateSpeed;

    public IClient ServerClient { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        ServerClient = transform.root.GetComponent<Player>().ServerClient;
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.parent.position, -Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            if (other.gameObject.GetComponent<Player>().IsServer)
            {
                //if collision happens on the server, notify the player to get ready for damage to the enemy player
                Debug.Log("Server Apply Damage");
                ServerManager.Instance.serverPlayersInScene[ServerClient].GetComponent<PlayerCombatManager>().ReadyForDamage(other.gameObject.GetComponent<Player>().ServerClient);
            }

        }


    }
}
