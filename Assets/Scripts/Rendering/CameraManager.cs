using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;


    private void OnEnable()
        => SettingsState.FieldOfViewChanged += UpdateFov;

    private void OnDisable()
        => SettingsState.FieldOfViewChanged -= UpdateFov;
    

    private void UpdateFov(float fov)
    {
        if (targetCamera != null)
            targetCamera.fieldOfView = fov;
    }
}
