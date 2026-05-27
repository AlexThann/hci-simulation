using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public TextAsset scenarioToLoad;

    public void LoadScenario()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager is missing from the scene.");
            return;
        }

        if (scenarioToLoad == null)
        {
            Debug.LogError("No JSON assigned to Load.");
            return;
        }

        // 1. Pass the data to the persistent manager
        GameManager.Instance.activeScenarioJson = scenarioToLoad;

        // 2. Route to the ICU room
        SceneManager.LoadScene("ICUroom");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}