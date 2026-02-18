using UnityEngine;

public class PlayerSleep : MonoBehaviour
{
    [SerializeField] private PlayerPassOut _endDay;

    private void Awake()
    {
        if (_endDay == null)
            _endDay = GetComponent<PlayerPassOut>();
    }

    private void Update()
    {
        // Use your existing input system
        if (InputManager.InteractPressed)
        {
            // IMPORTANT: In the future, only call this when player is interacting with the BED.
            // For now this matches your test behavior without using UnityEngine.Input.
            _endDay.SleepNow();
        }
    }
}
