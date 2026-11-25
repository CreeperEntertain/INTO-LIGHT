using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-32000)]
public class DisableAutomaticUIToolkitInput : MonoBehaviour
{
    private void Awake()
    {
        PanelInputConfiguration config = FindFirstObjectByType<PanelInputConfiguration>();
        if (config == null)
        {
            GameObject configObject = new("UI Toolkit Input Configuration");
            configObject.SetActive(false);
            config = configObject.AddComponent<PanelInputConfiguration>();
            Configure(config);
            configObject.SetActive(true);
            DontDestroyOnLoad(configObject);
            return;
        }
        Configure(config);
    }

    private static void Configure(PanelInputConfiguration config)
    {
        config.panelInputRedirection = PanelInputConfiguration.PanelInputRedirection.Always;
        config.autoCreatePanelComponents = false;
    }
}
