using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingCombatText : MonoBehaviour
{

    public float DisableTime = 3f;
    public Color defaultColor;
    public Vector3 Offset = new Vector3(-2, 2, 0);
    public Vector3 RandomizeIntensity = new Vector3(0.5f, 0, 0);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }

    void OnEnable()
    {
        Invoke("DisableObject", DisableTime);

        transform.position += Offset;
        int pos = Random.Range(-1, 1) < 0 ? -1 : 1;
        transform.position += new Vector3(pos, 0, 0);
        GetComponent<Animator>().SetTrigger("Text");
    }

    void DisableObject()
    {
        gameObject.SetActive(false);
        GetComponent<TextMeshPro>().color = defaultColor;
    }
}
