using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerEnergy _playerEnergy;

    [Tooltip("If you have a Slider component on this object, assign it (or leave empty to auto-find).")]
    [SerializeField] private Slider _slider;

    [Tooltip("If you're NOT using Slider, assign the Fill Image here (Image Type must be Filled).")]
    [SerializeField] private Image _fillImage;

    private void Awake()
    {
        if (_playerEnergy == null)
            _playerEnergy = FindFirstObjectByType<PlayerEnergy>();

        if (_slider == null)
            _slider = GetComponent<Slider>(); // auto-grab if present
    }

    private void OnEnable()
    {
        if (_playerEnergy != null)
            _playerEnergy.OnEnergyChanged += HandleEnergyChanged;

        // Initial refresh
        if (_playerEnergy != null)
            HandleEnergyChanged(_playerEnergy.CurrentEnergy, _playerEnergy.MaxEnergy);
    }

    private void OnDisable()
    {
        if (_playerEnergy != null)
            _playerEnergy.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void HandleEnergyChanged(int current, int max)
    {
        if (max <= 0) return;

        // If a Slider exists, use it (best when Slider controls Fill)
        if (_slider != null)
        {
            _slider.maxValue = max;
            _slider.value = current;
            return;
        }

        // Otherwise fallback to Fill Image
        if (_fillImage != null)
        {
            _fillImage.fillAmount = (float)current / max;
            return;
        }

        Debug.LogWarning("EnergyBarUI: No Slider or Fill Image assigned.");
    }
}
