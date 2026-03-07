using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Interaction Tile")]
    [SerializeField] private Toggle alwaysShowTileToggle;

    [Header("Audio")]
    [SerializeField] private Slider musicVolumeSlider;

    private void Start()
    {
        if (alwaysShowTileToggle != null)
        {
            alwaysShowTileToggle.isOn = InputManager.AlwaysShowInteractionTile;
            alwaysShowTileToggle.onValueChanged.AddListener(OnAlwaysShowTileToggleChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;

            float startVolume = 0.5f;
            if (MusicManager.Instance != null)
                startVolume = MusicManager.Instance.CurrentVolume;

            musicVolumeSlider.value = startVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }

    private void OnDestroy()
    {
        if (alwaysShowTileToggle != null)
            alwaysShowTileToggle.onValueChanged.RemoveListener(OnAlwaysShowTileToggleChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }

    private void OnAlwaysShowTileToggleChanged(bool isOn)
    {
        InputManager.SetAlwaysShowInteractionTile(isOn);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);
    }
}