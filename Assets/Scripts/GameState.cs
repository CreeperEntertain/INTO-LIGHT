using System;

public static class GameState
{
    public static event Action<bool> MainMenuActiveStateChanged;
    public static event Action<bool> PauseStateChanged;


    private static bool mainMenuActive;
    private static bool isPaused;


    public static bool MainMenuActive => mainMenuActive;
    public static bool IsPaused => isPaused;


    public static void SetMainMenuActive(bool newState)
    {
        if (mainMenuActive == newState)
            return;
        mainMenuActive = newState;
        MainMenuActiveStateChanged?.Invoke(mainMenuActive);
    }
    public static void SetPaused(bool newState)
    {
        if (isPaused == newState)
            return;
        isPaused = newState;
        PauseStateChanged?.Invoke(isPaused);
    }
}
