using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header ("Button")]
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private TextMeshProUGUI resumeButtonText;
    [SerializeField] private GameTimerController gameTimer;

    [Header ("Key Binding")]
    [SerializeField] private RectTransform controlsUI; // RectTransform으로 변경
    [SerializeField] private float bindingTime = 3f;
    [SerializeField] private float fadeDuration = 0.5f; // 페이드 효과 지속 시간

    private CanvasGroup controlsCanvasGroup;

    private void Start()
    {
        if (gameTimer == null)
        {
            Debug.LogError("GameTimerController is not assigned to UIManager!");
        }

        resumeButton.SetActive(false);
        
        if (controlsUI != null)
        {
            controlsCanvasGroup = controlsUI.GetComponent<CanvasGroup>();
            if (controlsCanvasGroup == null)
            {
                controlsCanvasGroup = controlsUI.gameObject.AddComponent<CanvasGroup>();
            }
            ShowControlsUI();
        }
        else
        {
            Debug.LogError("Controls UI is not assigned to UIManager!");
        }
    }

    private void ShowControlsUI()
    {
        // 초기 설정
        controlsCanvasGroup.alpha = 0f;

        // UI 등장 애니메이션
        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(controlsCanvasGroup.DOFade(1, fadeDuration));

        // 대기 후 사라짐 애니메이션
        showSequence.AppendInterval(bindingTime);
        showSequence.Append(controlsCanvasGroup.DOFade(0, fadeDuration));

        // 애니메이션 실행
        showSequence.Play();
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