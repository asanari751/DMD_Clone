using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class Interaction : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public Image illustration;
        public string characterName;
        [TextArea(3, 10)]
        public string[] dialogueTexts;
    }

    [System.Serializable]
    public class ButtonFrame
    {
        public Image normalFrame;
        public Image selectedFrame;
    }

    [SerializeField] private AudioManager audioManager;

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionField;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private float typingSpeed;
    [SerializeField] private GameObject titleImage;
    [SerializeField] private CanvasGroup titleImageCanvasGroup;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Vector3 promptOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private float promptDelay;
    [SerializeField] private CanvasGroup interactionPromptCanvasGroup;

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactionAction;
    [SerializeField] private InputActionReference cancleAction;

    [Header("Character Data")]
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float repeatWeight; // 대사 재선택 확률

    [Header("Button Animation")]
    [SerializeField] private GameObject godChoosePanel;
    [SerializeField] private Button[] godChooseButtons;
    [SerializeField] private float animationDuration;
    [SerializeField] private ButtonFrame[] buttonFrames;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private bool isInRange = false;
    private Tween textTween;
    private bool isDialogueComplete = false;
    private Vector3[] originalButtonPositions;
    private bool areButtonsVisible = false;
    private bool nowInteract = false;
    private Image interactionPromptImage;
    private Dictionary<int, int> lastDialogueIndices = new Dictionary<int, int>();

    private void Start()
    {
        SetInteractionUI(false);
        titleImage.SetActive(false);
        godChoosePanel.SetActive(false);
        interactionPrompt.SetActive(false);
        backgroundOverlay.gameObject.SetActive(false);

        if (interactionPromptCanvasGroup != null)
        {
            interactionPromptCanvasGroup.alpha = 0f;
        }

        InitializeGodChooseButtons();
        InitializeCloseButton();
    }

    private void Update()
    {
        if (isInRange && interactionPrompt.activeSelf)
        {
            UpdatePromptPosition();
        }
    }

    private void InitializeGodChooseButtons()
    {
        originalButtonPositions = new Vector3[godChooseButtons.Length];
        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            originalButtonPositions[i] = godChooseButtons[i].transform.position;
            godChooseButtons[i].gameObject.SetActive(false);

            // 프레임 초기 상태 설정
            if (buttonFrames[i] != null)
            {
                buttonFrames[i].normalFrame.gameObject.SetActive(true);
                buttonFrames[i].selectedFrame.gameObject.SetActive(false);
            }

            int index = i;
            godChooseButtons[i].onClick.AddListener(() => OnGodChooseButtonClicked(index));
        }
    }

    private void InitializeCloseButton()
    {
        closeButton.onClick.AddListener(() =>
        {
            EndInteraction();
        });
    }

    private void OnEnable()
    {
        interactionAction.action.Enable();
        interactionAction.action.performed += OnInteract;
        cancleAction.action.Enable();
        cancleAction.action.performed += OnCancel;
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        interactionAction.action.Disable();
        interactionAction.action.performed -= OnInteract;
        cancleAction.action.Disable();
        cancleAction.action.performed -= OnCancel;
        InputSystem.onActionChange -= OnActionChange;

        if (textTween != null && textTween.IsActive()) // 강제 취소
        {
            textTween.Kill();
        }
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.BoundControlsChanged)
        {
            UpdatePromptText();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;

            if (interactionPromptCanvasGroup != null)
            {
                interactionPromptCanvasGroup.DOKill();
            }

            interactionPrompt.SetActive(true);
            interactionPromptCanvasGroup.alpha = 0f;
            interactionPromptCanvasGroup.DOFade(1f, 0.5f);

            UpdatePromptText();
            UpdatePromptPosition();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        isInRange = false;
        if (isInRange == false)
        {
            if (interactionPromptCanvasGroup != null)
            {
                interactionPromptCanvasGroup.DOKill();
            }

            interactionPromptCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                interactionPrompt.SetActive(false);
            });

            EndInteraction();
            SetInteractionUI(false);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isInRange && !nowInteract)
        {
            nowInteract = true;

            if (!areButtonsVisible)
            {
                ShowGodChooseButtons();
            }

            else if (!isDialogueComplete)
            {
                CompleteDialogue();
            }

            else
            {
                EndInteraction();
            }
        }
    }

    // private void OnInteract(InputAction.CallbackContext context)
    // {
    //     if (isInRange)
    //     {
    //         if (!areButtonsVisible)
    //         {
    //             ShowInteractionButtons();
    //         }
    //         else if (!isDialogueComplete)
    //         {
    //             if (textTween != null && textTween.IsActive())
    //             {
    //                 textTween.Kill();
    //             }
    //             dialogueText.maxVisibleCharacters = dialogueText.text.Length;
    //             isDialogueComplete = true;
    //         }
    //         else
    //         {
    //             HideInteractionButtons();
    //             SetInteractionUI(false);
    //             isDialogueComplete = false;
    //         }
    //     }
    // }

    private void SetInteractionUI(bool enable)
    {
        if (interactionField != null)
        {
            interactionField.SetActive(enable);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(enable);
        }
    }

    private void DisplayCharacterData(CharacterData characterToDisplay, string selectedDialogue)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].illustration.gameObject.SetActive(characters[i] == characterToDisplay);
        }

        characterNameText.text = characterToDisplay.characterName;

        if (textTween != null && textTween.IsActive())
        {
            textTween.Kill();
        }

        dialogueText.text = selectedDialogue;
        dialogueText.maxVisibleCharacters = 0;
        isDialogueComplete = false;

        float duration = selectedDialogue.Length / typingSpeed;

        textTween = DOTween.To(() => dialogueText.maxVisibleCharacters,
        x => dialogueText.maxVisibleCharacters = x,
        selectedDialogue.Length,
        duration)
        .SetEase(Ease.Linear)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            isDialogueComplete = true;
        });
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (textTween != null && textTween.IsActive())
        {
            textTween.Kill();
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            isDialogueComplete = true;
        }
        else
        {
            SetInteractionUI(false);
        }
    }

    private void ShowGodChooseButtons()
    {
        audioManager.PlayAmbient("A4");
        PauseController.Paused = true;
        Time.timeScale = 0f;
        godChoosePanel.SetActive(true);
        backgroundOverlay.gameObject.SetActive(true);

        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            godChooseButtons[i].gameObject.SetActive(true);
            godChooseButtons[i].transform.position = originalButtonPositions[i];
        }
        areButtonsVisible = true;
        titleImage.SetActive(true);

        titleImage.SetActive(true);
        if (titleImageCanvasGroup != null)
        {
            titleImageCanvasGroup.alpha = 1f;
        }
    }

    private void OnGodChooseButtonClicked(int index)
    {
        if (index < characters.Length)
        {
            DisableAllButtonComponents();
            AnimateButtonSelection(index);

            CharacterData selectedCharacter = characters[index];

            int randomDialogueIndex = Random.Range(0, 3);
            string selectedDialogue = selectedCharacter.dialogueTexts[randomDialogueIndex];

            DisplayCharacterData(selectedCharacter, selectedDialogue);
        }
    }

    private void AnimateButtonSelection(int selectedIndex)
    {
        if (titleImageCanvasGroup != null)
        {
            titleImageCanvasGroup.DOFade(0f, animationDuration)
                .SetUpdate(true)
                .OnComplete(() => titleImage.SetActive(false));
        }

        // 정렬 순서 변경
        godChooseButtons[selectedIndex].transform.SetAsLastSibling();

        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            Button button = godChooseButtons[i];
            ButtonFrame frame = buttonFrames[i];

            if (i == selectedIndex)
            {
                // 선택된 버튼의 프레임 변경
                if (frame != null)
                {
                    frame.normalFrame.gameObject.SetActive(false);
                    frame.selectedFrame.gameObject.SetActive(true);
                }

                button.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetUpdate(true);
                button.transform.DOMove(originalButtonPositions[i], 0.3f).SetUpdate(true);
                button.image.DOColor(Color.white, animationDuration).SetUpdate(true).OnComplete(() =>
                {
                    button.gameObject.SetActive(false);

                    int randomDialogueIndex = Random.Range(0, 3);
                    string selectedDialogue = GetWeightedRandomDialogue(selectedIndex);
                    DisplayCharacterData(characters[selectedIndex], selectedDialogue);
                    SetInteractionUI(true);
                });
            }
            else
            {
                // 선택되지 않은 버튼들은 기본 프레임 유지
                if (frame != null)
                {
                    frame.normalFrame.gameObject.SetActive(true);
                    frame.selectedFrame.gameObject.SetActive(false);
                }

                Color targetColor = new Color(0x44 / 255f, 0x44 / 255f, 0x44 / 255f);
                button.image.DOColor(targetColor, animationDuration)
                    .OnComplete(() => button.gameObject.SetActive(false))
                    .SetUpdate(true);
            }
        }

        nowInteract = false;
        DOVirtual.DelayedCall(animationDuration, () => { EnableAllButtonComponents(); }).SetUpdate(true);
    }

    private void CompleteDialogue()
    {
        if (textTween != null && textTween.IsActive())
        {
            textTween.Kill();
        }
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        isDialogueComplete = true;
        nowInteract = false;
    }

    private void EndInteraction()
    {
        PauseController.Paused = false;
        Time.timeScale = 1f;

        if (godChoosePanel != null)
            godChoosePanel.SetActive(false);

        foreach (var button in godChooseButtons)
        {
            if (button != null && button.gameObject != null)
            {
                DOTween.Kill(button.transform);
                DOTween.Kill(button.image);

                Image[] childImages = button.GetComponentsInChildren<Image>();
                foreach (Image img in childImages)
                {
                    if (img != null)
                    {
                        DOTween.Kill(img);
                        img.color = Color.white;
                    }
                }

                button.transform.localScale = Vector3.one;
                button.transform.position = originalButtonPositions[System.Array.IndexOf(godChooseButtons, button)];
            }
        }

        // 프레임 초기화
        for (int i = 0; i < buttonFrames.Length; i++)
        {
            if (buttonFrames[i] != null)
            {
                buttonFrames[i].normalFrame.gameObject.SetActive(true);
                buttonFrames[i].selectedFrame.gameObject.SetActive(false);
            }
        }

        if (backgroundOverlay != null && backgroundOverlay.gameObject != null)
            backgroundOverlay.gameObject.SetActive(false);

        SetInteractionUI(false);
        isDialogueComplete = false;
        areButtonsVisible = false;
        nowInteract = false;
    }

    private string GetWeightedRandomDialogue(int characterIndex)
    {
        float[] weights = new float[3];
        int lastIndex;

        // 이전 대사 인덱스 확인
        if (!lastDialogueIndices.TryGetValue(characterIndex, out lastIndex))
        {
            lastIndex = -1;
        }

        // 가중치 설정
        for (int i = 0; i < 3; i++)
        {
            weights[i] = (i == lastIndex) ? repeatWeight : 1f;
        }

        // 가중치 기반 랜덤 선택
        float totalWeight = weights.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < 3; i++)
        {
            currentSum += weights[i];
            if (randomValue <= currentSum)
            {
                lastDialogueIndices[characterIndex] = i; // 선택된 인덱스 저장
                return characters[characterIndex].dialogueTexts[i];
            }
        }

        return characters[characterIndex].dialogueTexts[0];
    }

    private void DisableAllButtonComponents()
    {
        foreach (var button in godChooseButtons)
        {
            button.enabled = false;
        }
    }

    private void EnableAllButtonComponents()
    {
        foreach (var button in godChooseButtons)
        {
            button.enabled = true;
        }
    }

    // interactionPrompt

    private void UpdatePromptPosition()
    {
        if (interactionPrompt != null)
        {
            Vector3 worldPosition = transform.position + promptOffset;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            interactionPrompt.transform.DOKill();

            interactionPrompt.transform.DOMove(screenPosition, promptDelay)
            .SetEase(Ease.OutQuad);
        }
    }

    private void UpdatePromptText()
    {
        for (int i = 0; i < interactionAction.action.bindings.Count; i++)
        {
            if (interactionAction.action.bindings[i].path.Contains("Keyboard"))
            {
                string keyName = interactionAction.action.GetBindingDisplayString(i);
                promptText.text = $"{keyName}    상호작용";
                break;
            }
        }
    }
}