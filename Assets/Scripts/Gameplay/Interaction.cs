using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class Interaction : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public Image illustration;
        public string characterName;
        [TextArea(3, 10)]
        public string dialogueText;
    }

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionField;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private float typingSpeed;

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactionAction;
    [SerializeField] private InputActionReference cancleAction;

    [Header("Character Data")]
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Button Animation")]
    [SerializeField] private Button[] godChooseButtons;
    [SerializeField] private float godButtonAnimationDuration = 0.5f;
    [SerializeField] private float godButtonSpacing = 100f;

    [SerializeField] private bool isInRange = false;
    private Tween textTween;
    private bool isDialogueComplete = false;
    private Vector3[] originalButtonPositions;
    private bool areButtonsVisible = false;

    private void Start()
    {
        SetInteractionUI(false);
        InitializeGodChooseButtons();
    }

    private void InitializeGodChooseButtons()
    {
        originalButtonPositions = new Vector3[godChooseButtons.Length];
        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            originalButtonPositions[i] = godChooseButtons[i].transform.position;
            godChooseButtons[i].gameObject.SetActive(false);
            int index = i;
            godChooseButtons[i].onClick.AddListener(() => OnGodChooseButtonClicked(index));
        }
    }

    private void OnEnable()
    {
        interactionAction.action.Enable();
        interactionAction.action.performed += OnInteract;
        cancleAction.action.Enable();
        cancleAction.action.performed += OnCancel;
    }

    private void OnDisable()
    {
        interactionAction.action.Disable();
        interactionAction.action.performed -= OnInteract;
        cancleAction.action.Disable();
        cancleAction.action.performed -= OnCancel;

        if (textTween != null && textTween.IsActive()) // 강제 취소
        {
            textTween.Kill();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        isInRange = false;
        if (isInRange == false)
        {
            SetInteractionUI(false);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isInRange && !areButtonsVisible)
        {
            ShowGodChooseButtons();
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

        if (backgroundOverlay != null && backgroundOverlay.gameObject != null)
        {
            backgroundOverlay.gameObject.SetActive(enable);
        }
    }


    private void DisplayCharacterData(CharacterData characterToDisplay)
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

        dialogueText.text = characterToDisplay.dialogueText;
        dialogueText.maxVisibleCharacters = 0;
        isDialogueComplete = false;

        float duration = characterToDisplay.dialogueText.Length / typingSpeed;

        textTween = DOTween.To(() => dialogueText.maxVisibleCharacters, x => dialogueText.maxVisibleCharacters = x, characterToDisplay.dialogueText.Length, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => isDialogueComplete = true);
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
        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            godChooseButtons[i].gameObject.SetActive(true);
            Vector3 startPosition = originalButtonPositions[i] + Vector3.down * godButtonSpacing;
            godChooseButtons[i].transform.position = startPosition;

            godChooseButtons[i].transform.DOMove(originalButtonPositions[i], godButtonAnimationDuration)
                .SetEase(Ease.OutBack);
        }
        areButtonsVisible = true;
    }

    private void HideGodChooseButtons()
    {
        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            Vector3 endPosition = originalButtonPositions[i] + Vector3.down * godButtonSpacing;
            godChooseButtons[i].transform.DOMove(endPosition, godButtonAnimationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => godChooseButtons[i].gameObject.SetActive(false));
        }
        areButtonsVisible = false;
    }

    private void OnGodChooseButtonClicked(int index)
    {
        if (index < characters.Length)
        {
            HideGodChooseButtons();
            DisplayCharacterData(characters[index]);
            SetInteractionUI(true);
        }
    }
}