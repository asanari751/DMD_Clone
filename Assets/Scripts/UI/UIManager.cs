using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [Header("Scripts")]
    [SerializeField] private GameTimerController gameTimerController;

    // [Header("Button")]
    // [SerializeField] private GameObject resumeButton;

    [Header("Key Binding")]
    [SerializeField] private RectTransform controlsUI;
    [SerializeField] private float bindingTime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Image")]
    [SerializeField] private GameObject pauseOverlay;
    [SerializeField] private TextMeshProUGUI pausedText;

    [Header("Stage Clear")]
    [SerializeField] private string hubSceneName = "1_Hub";
    [SerializeField] private GameObject stageClearUI;
    [SerializeField] private Button endGameButton;
    [SerializeField] private Scene hubScene;

    private CanvasGroup controlsCanvasGroup;
    private PauseController pauseController;
    private SceneTransitionManager sceneTransitionManager;

    private void Start()
    {
        // resumeButton.SetActive(false);
        stageClearUI.SetActive(false);

        pauseController = FindAnyObjectByType<PauseController>();
        sceneTransitionManager = FindAnyObjectByType<SceneTransitionManager>();

        if (controlsUI != null)
        {
            controlsCanvasGroup = controlsUI.GetComponent<CanvasGroup>();
            if (controlsCanvasGroup == null)
            {
                controlsCanvasGroup = controlsUI.gameObject.AddComponent<CanvasGroup>();
            }
            ShowControlsUI();
        }

        // if (resumeButton != null)
        // {
        //     resumeButton.GetComponent<Button>().onClick.AddListener(OnResumeButtonClick);
        // }
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         CheckButtonClick();
    //     }
    // }

    // private void CheckButtonClick()
    // {
    //     PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
    //     pointerEventData.position = Input.mousePosition;

    //     List<RaycastResult> results = new List<RaycastResult>();
    //     EventSystem.current.RaycastAll(pointerEventData, results);

    //     foreach (RaycastResult result in results)
    //     {
    //         if (result.gameObject == resumeButton)
    //         {
    //             OnResumeButtonClick();
    //             break;
    //         }
    //     }
    // }

    private void ShowControlsUI()
    {
        controlsCanvasGroup.alpha = 0f;

        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(controlsCanvasGroup.DOFade(1, fadeDuration));

        showSequence.AppendInterval(bindingTime);
        showSequence.Append(controlsCanvasGroup.DOFade(0, fadeDuration));

        showSequence.Play();
    }

    // public void OnResumeButtonClick()
    // {
    //     if (gameTimerController != null)
    //     {
    //         gameTimerController.ResumeTimer();
    //         gameTimerController.RemoveCombatAreaLimits();
    //         resumeButton.SetActive(false);
    //     }
    // }

    // public void ShowResumeButton()
    // {
    //     if (resumeButton != null)
    //     {
    //         resumeButton.SetActive(true);
    //     }
    // }

    public void SetPauseUIVisibility(bool isVisible)
    {
        pauseOverlay.SetActive(isVisible);
        pausedText.gameObject.SetActive(isVisible);
    }

    public void ShowStageClearUI()
    {
        stageClearUI.SetActive(true);
        stageClearUI.transform.SetAsLastSibling();
        
        endGameButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneTransitionManager.Instance.LoadSceneWithFade(hubSceneName);
            Debug.Log("씬 변경: Hub");
        });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "1_Hub")
        {
            pauseController.ResumeForGameClear();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}