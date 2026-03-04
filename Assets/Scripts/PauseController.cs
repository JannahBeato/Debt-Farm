using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    // Supports multiple systems pausing at once (shop + sleep prompt + menu, etc.)
    private static int pauseRequests = 0;

    public static void SetPause(bool pause)
    {
        if (pause) pauseRequests++;
        else pauseRequests = Mathf.Max(0, pauseRequests - 1);

        bool shouldPause = pauseRequests > 0;
        if (IsPaused == shouldPause) return;

        IsPaused = shouldPause;
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    // safety reset when entering play mode / scene loads
    private void OnDisable()
    {
        pauseRequests = 0;
        IsPaused = false;
        Time.timeScale = 1f;
    }
}