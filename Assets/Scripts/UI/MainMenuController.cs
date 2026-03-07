using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "MainScene";

    [Header("Optional")]
    [SerializeField] private Button loadGameButton;

    private string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    private void Start()
    {
        if (loadGameButton != null)
            loadGameButton.interactable = File.Exists(SavePath);
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;

        if (File.Exists(SavePath))
            File.Delete(SavePath);

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}