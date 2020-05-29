using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    public float speed = 0;
    public Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void OnDisable()
    {
        transform.position = initialPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + new Vector3(Time.deltaTime * speed, Time.deltaTime * speed, 0);
        if (transform.position.x < -initialPosition.x) transform.position = initialPosition;
    }
}
