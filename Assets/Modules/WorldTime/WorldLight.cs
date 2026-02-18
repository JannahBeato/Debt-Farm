using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class WorldLight : MonoBehaviour
{
    private Light2D _light;

    [SerializeField] private Gradient _lightGradient;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
        TimeManager.OnDateTimeChanged += OnTimeChanged;
    }

    private void OnDestroy()
    {
        TimeManager.OnDateTimeChanged -= OnTimeChanged;
    }

    private void OnTimeChanged(DateTime dateTime)
    {
        float percent = dateTime.GetMinutesOfDay() / 1440f;
        _light.color = _lightGradient.Evaluate(percent);
    }
}
