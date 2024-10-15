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
    [SerializeField] private GameObject titleImage;

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactionAction;
    [SerializeField] private InputActionReference cancleAction;

    [Header("Character Data")]
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Button Animation")]
    [SerializeField] private Button[] godChooseButtons;
    [SerializeField] private float animationDuration;

    [SerializeField] private bool isInRange = false;
    private Tween textTween;
    private bool isDialogueComplete = false;
    private Vector3[] originalButtonPositions;
    private bool areButtonsVisible = false;
    private bool nowInteract = false;
    private bool maximized = false;

    private void Start()
    {
        SetInteractionUI(false);
        InitializeGodChooseButtons();
        titleImage.SetActive(false);
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
            godChooseButtons[i].transform.position = originalButtonPositions[i];
        }
        areButtonsVisible = true;
        titleImage.SetActive(true);

        if (backgroundOverlay != null && backgroundOverlay.gameObject != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
        }
    }

    private void OnGodChooseButtonClicked(int index)
    {
        if (index < characters.Length)
        {
            AnimateButtonSelection(index);
            titleImage.SetActive(false);
        }
    }

    private void AnimateButtonSelection(int selectedIndex)
    {
        for (int i = 0; i < godChooseButtons.Length; i++)
        {
            Button button = godChooseButtons[i];
            if (i == selectedIndex && !maximized)
            {
                button.transform.DOScale(Vector3.one * 1.2f, 0.3f);
                button.transform.DOMove(originalButtonPositions[i], 0.3f);
                button.image.DOColor(Color.white, animationDuration).OnComplete(() =>
                {
                    button.gameObject.SetActive(false);
                    DisplayCharacterData(characters[selectedIndex]);
                    SetInteractionUI(true);
                });
            }
            else
            {
                Color targetColor = new Color(0x44 / 255f, 0x44 / 255f, 0x44 / 255f);
                button.image.DOColor(targetColor, animationDuration).OnComplete(() => button.gameObject.SetActive(false));

                Image[] childImages = button.GetComponentsInChildren<Image>();
                foreach (Image img in childImages)
                {
                    img.DOColor(targetColor, animationDuration);
                }
            }
        }
        nowInteract = false;
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
        Debug.Log("End interaction");

        foreach (var button in godChooseButtons)
        {
            DOTween.Kill(button.transform);
            DOTween.Kill(button.image);

            Image[] childImages = button.GetComponentsInChildren<Image>();
            foreach (Image img in childImages)
            {
                DOTween.Kill(img);
                img.color = Color.white;
            }

            button.transform.localScale = Vector3.one;
            button.transform.position = originalButtonPositions[System.Array.IndexOf(godChooseButtons, button)];
        }

        SetInteractionUI(false);
        backgroundOverlay.gameObject.SetActive(false);
        isDialogueComplete = false;
        areButtonsVisible = false;
        nowInteract = false;
    }
}