using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class PlayerAIController : MonoBehaviour
{
    enum botState {search, attack, defense};
    botState botPlayerState;

    NavMeshAgent botController;
    GameObject targetPlayer;
    // Start is called before the first frame update
    void Start()
    {
        botController = GetComponent<NavMeshAgent>();
        botPlayerState = botState.search;
        StartCoroutine(SearchPlayer());

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SearchPlayer()
    {
        yield return new WaitForSeconds(1);

        FindPlayer(GameManager.Instance.GetCurrentPlayer.transform.position);

        StartCoroutine(SearchPlayer());
    }

    void FindPlayer(Vector3 target)
    {
        botController.SetDestination(target);
    }
}
