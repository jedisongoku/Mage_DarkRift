using Cinemachine;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera camera;
    public float left = -22;
    public float right = 32;
    public float top = 27;
    public float bottom = -22;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.GetCurrentPlayer != null)
        {
            if (GameManager.Instance.GetCurrentPlayer.transform.position.x > right || GameManager.Instance.GetCurrentPlayer.transform.position.x < left)
                camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 1;
            else camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0.15f;

            if (GameManager.Instance.GetCurrentPlayer.transform.position.z > top || GameManager.Instance.GetCurrentPlayer.transform.position.z < bottom)
                camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 1;
            else camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0.15f;
        }
        

    }
}
