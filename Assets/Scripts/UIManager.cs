using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private TextMeshProUGUI resumeButtonText;
    [SerializeField] private GameTimerController gameTimer;

    private void Start()
    {
        if (resumeButton == null)
        {
            Debug.LogError("Resume Button is not assigned to UIManager!");
        }
        if (resumeButtonText == null)
        {
            Debug.LogError("Resume Button Text is not assigned to UIManager!");
        }
        if (gameTimer == null)
        {
            Debug.LogError("GameTimerController is not assigned to UIManager!");
        }

        resumeButton.SetActive(false);
    }

    public void OnResumeButtonClick()
    {
        Debug.Log("Resume button clicked");
        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
            resumeButton.SetActive(false);
        }
        else
        {
            Debug.LogError("GameTimerController is null in UIManager. Cannot resume timer.");
        }
    }

    public void ShowResumeButton()
    {
        Debug.Log("ShowResumeButton called");
        if (resumeButton != null)
        {
            resumeButton.SetActive(true);
        }
        else
        {
            Debug.LogError("Resume Button is null in UIManager. Cannot show button.");
        }
    }
}