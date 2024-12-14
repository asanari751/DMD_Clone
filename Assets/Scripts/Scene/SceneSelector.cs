using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class SceneSelector : MonoBehaviour
{
    [System.Serializable]
    public class StageData
    {
        public string sceneName;
        public string stageName;
        public Sprite thumbnail;
    }

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Image capsuleArt;
    [SerializeField] private TextMeshProUGUI stageTitle;
    [SerializeField] private Button[] navigationButtons;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private List<StageData> stages;

    private int currentStageIndex = 0;

    private void Start()
    {
        uiPanel.SetActive(false);
        SetupButtons();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckButtonClick();
        }
    }

    public void ShowUI()
    {
        uiPanel.SetActive(true);
        backgroundOverlay.gameObject.SetActive(true);
        UpdateStageInfo(stages[currentStageIndex]);
        audioManager.PlayAmbient("A2");
    }

    private void SetupButtons()
    {
        navigationButtons[0].onClick.AddListener(PreviousStage);
        navigationButtons[1].onClick.AddListener(NextStage);
        navigationButtons[2].onClick.AddListener(StartGame);
        navigationButtons[3].onClick.AddListener(Cancle);
    }

    private void CheckButtonClick()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == navigationButtons[0].gameObject)
            {
                PreviousStage();
                break;
            }
            else if (result.gameObject == navigationButtons[1].gameObject)
            {
                NextStage();
                break;
            }
            else if (result.gameObject == navigationButtons[2].gameObject)
            {
                StartGame();
                break;
            }
            else if (result.gameObject == navigationButtons[3].gameObject)
            {
                Cancle();
                break;
            }
        }
    }

    private void PreviousStage()
    {
        Debug.Log("Previous");
        if (currentStageIndex > 0)
        {
            currentStageIndex--;
            UpdateStageInfo(stages[currentStageIndex]);
        }
    }

    private void NextStage()
    {
        Debug.Log("Next");
        if (currentStageIndex < stages.Count - 1)
        {
            currentStageIndex++;
            UpdateStageInfo(stages[currentStageIndex]);
        }
    }

    private void StartGame()
    {
        audioManager.PlayAmbient("A3");
        SelectStage(currentStageIndex);
    }

    public void Cancle()
    {
        audioManager.PlayAmbient("A3");
        backgroundOverlay.gameObject.SetActive(false);
        uiPanel.SetActive(false);
    }

    private void SelectStage(int stageIndex)
    {
        string sceneName = stages[stageIndex].sceneName;
        sceneTransition.LoadScene(sceneName);
        uiPanel.SetActive(false);
    }

    private void UpdateStageInfo(StageData stageData)
    {
        capsuleArt.sprite = stageData.thumbnail;
        stageTitle.text = stageData.stageName;
    }
}
