using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument), typeof(GlobalUIOverlay))]
public class CursorState : MonoBehaviour
{
    private GlobalUIOverlay cursor;


    private void Awake()
        => cursor = GetComponent<GlobalUIOverlay>();

    private void OnEnable()
    {
        GameState.PauseStateChanged += UpdateCursorState;
        GameState.MainMenuActiveStateChanged += UpdateCursorState;
    }

    private void OnDisable()
    {
        GameState.PauseStateChanged -= UpdateCursorState;
        GameState.MainMenuActiveStateChanged -= UpdateCursorState;
    }


    private bool IsAnyUiOpen()
        =>  GameState.IsPaused ||
            GameState.MainMenuActive;

    private void UpdateCursorState(bool selecteMenuState)
    {
        if (selecteMenuState)
            cursor.Enable();
        if (!IsAnyUiOpen())
            cursor.Disable();
    }
}
