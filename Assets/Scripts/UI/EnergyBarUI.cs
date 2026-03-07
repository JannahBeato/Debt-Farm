using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerEnergy _energy;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fillImage;

    private void Awake()
    {
        if (_energy == null)
            _energy = FindFirstObjectByType<PlayerEnergy>();

        if (_slider == null)
            _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (_energy != null)
            _energy.OnEnergyChanged += HandleEnergyChanged;

        // Force an initial update
        if (_energy != null)
            HandleEnergyChanged(_energy.CurrentEnergy, _energy.MaxEnergy);
    }

    private void OnDisable()
    {
        if (_energy != null)
            _energy.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void HandleEnergyChanged(int current, int max)
    {
        if (_slider == null) return;
        if (max <= 0) return;

        _slider.value = (float)current / max;
    }
}
