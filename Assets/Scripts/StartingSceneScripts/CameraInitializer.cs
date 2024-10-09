using UnityEngine;
using Cinemachine;

public class CameraInitializer : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        // Ensure the camera starts looking straight ahead
        if (virtualCamera != null)
        {
            CinemachinePOV pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                pov.m_HorizontalAxis.Value = 0f; // Adjust as needed for initial horizontal view
                pov.m_VerticalAxis.Value = 0f;   // Adjust as needed for initial vertical view
            }
        }
    }
}