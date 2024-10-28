using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIManagerTitle : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName = "1_Hub";

    [Header("UI Elements")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    private void Start()
    {
        InitializeButtons();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void InitializeButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClick);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClick);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClick);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(OnCloseSettingsClick);
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

    private void OnPlayButtonClick()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade(playSceneName);
    }

    private void OnSettingsButtonClick()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    private void OnCloseSettingsClick()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnExitButtonClick()
    {
        Application.Quit();
    }
}
