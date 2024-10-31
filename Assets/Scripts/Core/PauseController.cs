using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnToHubButton;
    [SerializeField] private Button returnToTitleButton;

    private bool isRunning = true;
    public static bool Paused { get; set; } = false;
    private bool isGameEnded;
    private bool tempRunning = true;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);

        continueButton.onClick.AddListener(TogglePauseMenu);
        returnToHubButton.onClick.AddListener(ReturnToHub);
        returnToTitleButton.onClick.AddListener(ReturnToTitle);
    }

    public void ToggleUIState()
    {
        tempRunning = !tempRunning;
        Time.timeScale = tempRunning ? 1f : 0f;
        Paused = !tempRunning;
        uiManager.SetPauseUIVisibility(!tempRunning);
    }

    public void TogglePauseMenu()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "1_Hub" && currentScene != "2_Stage_Test")
            return;

        tempRunning = !tempRunning;
        Time.timeScale = tempRunning ? 1f : 0f;
        Paused = !tempRunning;
        pauseMenuPanel.SetActive(!tempRunning);
    }

    public void PauseForGameClear()
    {
        Paused = true;
        Time.timeScale = 0f;
    }

    public void ResumeForGameClear()
    {
        Paused = false;
        Time.timeScale = 1f;
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.LoadSceneWithFade("1_Hub");
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.LoadSceneWithFade("0_Title");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tempRunning)
            {
                TogglePauseMenu();
            }
            else
            {
                TogglePauseMenu();
            }
        }
    }

    // ===================================================

    public bool IsRunning()
    {
        return isRunning;
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public bool IsGameEnded()
    {
        return isGameEnded;
    }

    public void SetGameEnded(bool ended)
    {
        isGameEnded = ended;
    }
}
