using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootSteps : MonoBehaviour
{
    [SerializeField] AudioClip[] clip;
    [SerializeField] ParticleSystem dust;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Step()
    {
        if(!GetComponent<PlayerCombatManager>().isInBush)
        {
            //GetComponent<AudioSource>().PlayOneShot(clip[Random.Range(0, clip.Length - 1)]);
            dust.Play();
        }
        
    }
}
