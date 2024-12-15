using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;

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
    [SerializeField] private TextMeshProUGUI[] keyBindingTexts;
    [SerializeField] private Image[] keyBindingImages;

    [Header("Key Sprites")]
    [SerializeField] private Sprite normalKeySprite;
    [SerializeField] private Sprite wideKeySprite;
    [SerializeField] private Sprite longKeySprite; // 스페이스바 전용 

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
    private GameSettings gameSettings;
    private bool isLoadingScene = false;

    private void Start()
    {
        // resumeButton.SetActive(false);
        stageClearUI.SetActive(false);

        pauseController = FindAnyObjectByType<PauseController>();
        sceneTransitionManager = FindAnyObjectByType<SceneTransitionManager>();
        gameSettings = FindAnyObjectByType<GameSettings>();

        gameSettings = GameSettings.Instance;

        if (controlsUI != null)
        {
            controlsCanvasGroup = controlsUI.GetComponent<CanvasGroup>();
            if (controlsCanvasGroup == null)
            {
                controlsCanvasGroup = controlsUI.gameObject.AddComponent<CanvasGroup>();
            }

            ShowControlsUI();

        }

        UpdateKeyBindingDisplay();

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
            if (isLoadingScene) return; // 이미 로딩 중이면 무시

            isLoadingScene = true;
            Time.timeScale = 1f;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneTransitionManager.Instance.LoadSceneWithTransition(hubSceneName);
            endGameButton.interactable = false; // 버튼 비활성화
            Debug.Log("씬 변경: Hub");
        });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "1_Hub")
        {
            isLoadingScene = false; // 씬 로딩이 완료되면 플래그 초기화
            pauseController.ResumeForGameClear();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void UpdateKeyBindingDisplay()
    {
        for (int i = 0; i < keyBindingTexts.Length; i++)
        {
            InputAction action = gameSettings.GetActionByIndex(i);
            Debug.Log($"Index {i}: Action = {action}");

            int bindingIndex = i switch
            {
                0 => action.bindings.IndexOf(x => x.name == "up"),
                1 => action.bindings.IndexOf(x => x.name == "down"),
                2 => action.bindings.IndexOf(x => x.name == "left"),
                3 => action.bindings.IndexOf(x => x.name == "right"),
                _ => 0
            };
            Debug.Log($"Index {i}: BindingIndex = {bindingIndex}");

            string keyName = InputControlPath.ToHumanReadableString(
                action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
            Debug.Log($"Index {i}: KeyName = {keyName}");

            keyBindingTexts[i].text = keyName;
            keyBindingImages[i].sprite = GetKeySprite(keyName, i);
        }
    }

    private Sprite GetKeySprite(string keyName, int index)
    {
        RectTransform imageRect = keyBindingImages[index].GetComponent<RectTransform>();

        // SPACE (50x200)
        if (keyName.Equals("Space", System.StringComparison.OrdinalIgnoreCase))
        {
            imageRect.sizeDelta = new Vector2(200f, 50f);
            return longKeySprite;
        }

        // WIDE (50x100)
        string[] wideKeys = new[] {
        "Left Shift", "Right Shift",
        "Tab", "Caps Lock",
        "Left Ctrl", "Right Ctrl",
        "Left Alt", "Right Alt"
    };

        if (wideKeys.Any(k => keyName.Equals(k, System.StringComparison.OrdinalIgnoreCase)))
        {
            imageRect.sizeDelta = new Vector2(100f, 50f);
            return wideKeySprite;
        }

        // Normal (50x50)
        imageRect.sizeDelta = new Vector2(50f, 50f);
        return normalKeySprite;
    }
}