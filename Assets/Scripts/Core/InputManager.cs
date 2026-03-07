using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;
    public static bool InteractPressed;

    public static bool ShowInteractionTile { get; private set; }
    public static bool AlwaysShowInteractionTile { get; private set; }

    private const string AlwaysShowTilePrefKey = "AlwaysShowInteractionTile";

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _interactAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _interactAction = _playerInput.actions["Action"];

        AlwaysShowInteractionTile = PlayerPrefs.GetInt(AlwaysShowTilePrefKey, 0) == 1;
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        InteractPressed = _interactAction.WasPressedThisFrame();

        if (Keyboard.current != null)
        {
            ShowInteractionTile =
                Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed;
        }
        else
        {
            ShowInteractionTile = false;
        }
    }

    public static bool ShouldShowInteractionTile()
    {
        return AlwaysShowInteractionTile || ShowInteractionTile;
    }

    public static void SetAlwaysShowInteractionTile(bool enabled)
    {
        AlwaysShowInteractionTile = enabled;
        PlayerPrefs.SetInt(AlwaysShowTilePrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}