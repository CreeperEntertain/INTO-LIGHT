using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-1000)]
public class PopupManagerBootstrap : MonoBehaviour
{
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private StyleSheet popupStyleSheet;


    private void Awake()
        => PopupManager.Initialize(panelSettings, popupStyleSheet);
}
