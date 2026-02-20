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
        // TEST INPUT (replace with your bed interaction):
        // Example later: when interacting with Bed tile/object -> _endDay.SleepNow();
        if (Input.GetKeyDown(KeyCode.E))
        {
            _endDay.SleepNow();
        }
    }
}
