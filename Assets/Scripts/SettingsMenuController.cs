using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Toggle alwaysShowTileToggle;

    private void Start()
    {
        if (alwaysShowTileToggle == null)
        {
            Debug.LogError("SettingsMenuController: alwaysShowTileToggle is not assigned.");
            return;
        }

        alwaysShowTileToggle.isOn = InputManager.AlwaysShowInteractionTile;
        alwaysShowTileToggle.onValueChanged.AddListener(OnAlwaysShowTileToggleChanged);
    }

    private void OnDestroy()
    {
        if (alwaysShowTileToggle != null)
            alwaysShowTileToggle.onValueChanged.RemoveListener(OnAlwaysShowTileToggleChanged);
    }

    private void OnAlwaysShowTileToggleChanged(bool isOn)
    {
        InputManager.SetAlwaysShowInteractionTile(isOn);
    }
}