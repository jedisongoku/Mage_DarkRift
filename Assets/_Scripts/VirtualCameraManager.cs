using Cinemachine;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera camera;
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
            if (GameManager.Instance.GetCurrentPlayer.transform.position.x > 32 || GameManager.Instance.GetCurrentPlayer.transform.position.x < -22)
                camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 1;
            else camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0.15f;

            if (GameManager.Instance.GetCurrentPlayer.transform.position.z > 27 || GameManager.Instance.GetCurrentPlayer.transform.position.z < -22)
                camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 1;
            else camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0.15f;
        }
        

    }
}
