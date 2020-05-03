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
        m_Animator = transform.parent.parent.GetComponent<Animator>();
    }

    private void OnBecameVisible()
    {
        visible = true;
        m_Animator.enabled = true;
    }

    private void OnBecameInvisible()
    {
        visible = false;
        m_Animator.enabled = false;
    }
}
