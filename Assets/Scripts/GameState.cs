using System;
using UnityEngine;

public static class GameState
{
    public static event Action<bool> PauseStateChanged;

    private static bool isPaused;

    public static bool IsPaused => isPaused;

    public static void SetPaused(bool paused)
    {
        if (isPaused == paused)
            return;
        isPaused = paused;
        PauseStateChanged?.Invoke(isPaused);
    }
}
