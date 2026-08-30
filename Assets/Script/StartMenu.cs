using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("How to Play panel")]
    public GameObject howToPlayPanel;

    void Start()
    {
        // Hide the How to Play canvas by default.
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("FinalScene");
    }

    public void ToggleHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(!howToPlayPanel.activeSelf);
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
