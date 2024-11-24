using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

public class UIManagerTitle : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName = "1_Hub";

    [Header("UI Elements")]
    [SerializeField] private Button playButton;
    [SerializeField] private RawImage playBackground;
    [SerializeField] private Button settingsButton;
    [SerializeField] private RawImage settingsBackground;
    [SerializeField] private Button exitButton;
    [SerializeField] private RawImage exitBackground;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private float colorChangeDuration;
    [SerializeField] private Ease colorChangeEase = Ease.OutQuad;
    [SerializeField] private Color startColor = new Color(0, 0, 0, 0.2f);
    [SerializeField] private Color endColor = new Color(0, 0, 0, 1f);

    [Header("Game Settings")]
    [SerializeField] private GameSettings gameSettings;

    private void Start()
    {
        InitializeButtons();
        InitializeBackgrounds();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void InitializeButtons()
    {
        SetupButtonHoverEffects(playButton, playBackground);
        SetupButtonHoverEffects(settingsButton, settingsBackground);
        SetupButtonHoverEffects(exitButton, exitBackground);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClick);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClick);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClick);
    }

    private void InitializeBackgrounds()
    {
        SetupBackground(playBackground);
        SetupBackground(settingsBackground);
        SetupBackground(exitBackground);
    }

    private void SetupBackground(RawImage background)
    {
        if (background != null)
        {
            background.color = startColor; // 초기 상태는 투명
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckButtonClick();
        }
    }

    private void CheckButtonClick()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null)
            {
                break;
            }
        }
    }

    private void SetupButtonHoverEffects(Button button, RawImage background)
    {
        if (button != null && background != null)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((data) => {
                background.DOColor(endColor, colorChangeDuration).SetEase(colorChangeEase);
            });
            
            EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((data) => {
                background.DOColor(startColor, colorChangeDuration).SetEase(colorChangeEase);
            });

            trigger.triggers.Add(enterEntry);
            trigger.triggers.Add(exitEntry);
        }
    }

    private void OnPlayButtonClick()
    {
        SceneTransitionManager.Instance.LoadSceneWithTransition(playSceneName);
    }

    private void OnSettingsButtonClick()
{
    if (gameSettings != null)
    {
        gameSettings.OpenSettings();
    }
}

    private void OnCloseSettingsClick()
    {
        if (gameSettings != null)
        {
            gameSettings.OnSettingsExit();
        }
    }

    private void OnExitButtonClick()
    {
        Application.Quit();
    }
}
