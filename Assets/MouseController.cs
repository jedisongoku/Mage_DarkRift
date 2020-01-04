using UnityEngine;

public class MouseController : MonoBehaviour
{

    AgarObject agarObject;
    // Start is called before the first frame update
    void Start()
    {
        agarObject = GetComponent<AgarObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePoint.z = 0;

        agarObject.SetMovePosition(mousePoint);
    }
}
