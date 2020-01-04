using UnityEngine;

public class AgarObject : MonoBehaviour
{

    public float speed = 1f;
    float scale = 1f;
    Vector3 movePosition;

    private void Awake()
    {
        movePosition = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (speed != 0f)
            transform.position = Vector3.MoveTowards(transform.position, movePosition, speed * Time.deltaTime);
    }

    internal void SetColor(Color32 color)
    {
        Renderer renderer = GetComponent<Renderer>();

        renderer.material.color = color;
    }

    internal void SetRadius(float radius)
    {
        transform.localScale = new Vector3(radius * scale, radius * scale, 1);
    }

    internal void SetMovePosition(Vector3 newPosition)
    {
        movePosition = newPosition;
    }
}
