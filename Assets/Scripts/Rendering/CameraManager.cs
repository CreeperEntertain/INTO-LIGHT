using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera camera;


    private void OnEnable()
        => SettingsState.FieldOfViewChanged += UpdateFov;

    private void OnDisable()
        => SettingsState.FieldOfViewChanged -= UpdateFov;
    

    private void UpdateFov(float fov)
        => camera.fieldOfView = fov;
}
