using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationRendererUpdate : MonoBehaviour
{
    public bool visible;
    private Animator m_Animator;
    // Start is called before the first frame update

    private void Awake()
    {
        if(transform.parent.parent.GetComponent<Animator>() != null)
        {
            m_Animator = transform.parent.parent.GetComponent<Animator>();
        }
        
    }

    private void OnBecameVisible()
    {
        visible = true;
        if(m_Animator != null)
            m_Animator.enabled = true;
    }

    private void OnBecameInvisible()
    {
        visible = false;
        if (m_Animator != null)
            m_Animator.enabled = false;
    }
}
