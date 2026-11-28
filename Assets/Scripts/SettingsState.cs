using System;

public static class SettingsState
{
    public static event Action<float> MouseSensitivityChanged;
    public static event Action<float> ControllerSensitivityChanged;

    public static event Action<float> BrightnessChanged;
    public static event Action<float> ContrastChanged;
    public static event Action<float> FieldOfViewChanged;


    private static float mouseSensitivity = 1.0f;
    private static float controllerSensitivity = 1.0f;

    private static float brightness = 1.0f;
    private static float contrast = 1.0f;
    private static float fieldOfView = 75.0f;


    public static float MouseSensitiviy => mouseSensitivity;
    public static float ControllerSensitivity => controllerSensitivity;

    public static float Brightness => brightness;
    public static float Contrast => contrast;
    public static float FieldOfView => fieldOfView;


    public static void SetMouseSensitiviy(float newState)
    {
        if (mouseSensitivity == newState)
            return;
        mouseSensitivity = newState;
        MouseSensitivityChanged?.Invoke(mouseSensitivity);
    }
    public static void SetControllerSensitivity(float newState)
    {
        if (controllerSensitivity == newState)
            return;
        controllerSensitivity = newState;
        ControllerSensitivityChanged?.Invoke(controllerSensitivity);
    }

    public static void SetBrightness(float newState)
    {
        if (brightness == newState)
            return;
        brightness = newState;
        BrightnessChanged?.Invoke(brightness);
    }
    public static void SetContrast(float newState)
    {
        if (contrast == newState)
            return;
        contrast = newState;
        ContrastChanged?.Invoke(contrast);
    }
    public static void SetFieldOfView(float newState)
    {
        if (fieldOfView == newState)
            return;
        fieldOfView = newState;
        FieldOfViewChanged?.Invoke(fieldOfView);
    }
}
