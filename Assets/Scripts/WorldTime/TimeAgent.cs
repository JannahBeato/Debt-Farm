using UnityEngine;

public class TimeAgent : MonoBehaviour
{
   

    private void onEnable()
    {
        TimeManager.OnDateTimeChanged += HandleTimeChanged;
    }

    public void Invoke()
    {
    
    }

    private void OnDisable()
    {
        TimeManager.OnDateTimeChanged -= HandleTimeChanged;
    }

    private void HandleTimeChanged(DateTime dateTime)
    {

    }
}
