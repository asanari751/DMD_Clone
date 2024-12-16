using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private Sprite[] tutorialSprites;
    [SerializeField] private TMP_Text pageNumberText;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference interactionAction;
    [SerializeField] private InputActionReference movementAction;
    // [SerializeField] private InputActionReference mouseClickAction;


    [Header("UI")]
    [SerializeField] private Button closeButton;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text fillerText;
    [SerializeField] private Vector3 promptOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private float promptDelay;
    [SerializeField] private CanvasGroup interactionPromptCanvasGroup;

    private int currentPage = 0;
    private bool isTutorialActive = false;
    private const string TUTORIAL_COMPLETE_KEY = "TutorialComplete";

    private void Start()
    {
        // 최초 실행 체크
        if (PlayerPrefs.GetInt(TUTORIAL_COMPLETE_KEY, 0) == 0)
        {
            ShowTutorial();
            PlayerPrefs.SetInt(TUTORIAL_COMPLETE_KEY, 1);
            PlayerPrefs.Save();
        }

        closeButton.onClick.AddListener(CloseTutorial);
    }

    private void OnEnable()
    {
        interactionAction.action.Enable();
        movementAction.action.Enable();
        // mouseClickAction.action.Enable();

        movementAction.action.performed += OnMovementInput;
        // mouseClickAction.action.performed += OnMouseClick;
        interactionAction.action.performed += OnInteractionInput;
    }

    private void OnDisable()
    {
        interactionAction.action.Disable();
        movementAction.action.Disable();
        // mouseClickAction.action.Disable();

        movementAction.action.performed -= OnMovementInput;
        // mouseClickAction.action.performed -= OnMouseClick;
        interactionAction.action.performed -= OnInteractionInput;
    }

    private void ShowTutorial()
    {
        currentPage = 0;
        isTutorialActive = true;
        tutorialPanel.SetActive(true);
        PauseController.Paused = true;
        interactionAction.action.performed -= OnInteractionPerformed; // 튜토리얼 시작시 이벤트 해제
        UpdateTutorialImage();
    }

    // private void OnMouseClick(InputAction.CallbackContext context)
    // {
    //     if (!isTutorialActive) return;
    //     OnNextPage();
    // }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        if (!isTutorialActive) return;

        Vector2 movement = context.ReadValue<Vector2>();

        if (movement.x > 0)
        {
            audioManager.PlayUI("U1");
            OnNextPage();
        }
        else if (movement.x < 0)
        {
            audioManager.PlayUI("U1");
            OnPrevPage();
        }
    }

    private void OnInteractionInput(InputAction.CallbackContext context)
    {
        if (!isTutorialActive) return;
        audioManager.PlayUI("U1");
        OnNextPage();
    }

    private void OnNextPage()
    {
        currentPage++;
        if (currentPage >= tutorialSprites.Length)
        {
            CloseTutorial();
            return;
        }
        UpdateTutorialImage();
    }

    private void OnPrevPage()
    {
        currentPage--;
        if (currentPage < 0) currentPage = 0;
        UpdateTutorialImage();
    }

    private void UpdateTutorialImage()
    {
        tutorialImage.sprite = tutorialSprites[currentPage];

        pageNumberText.text = $"{currentPage + 1}/{tutorialSprites.Length}";
    }

    private void CloseTutorial()
    {
        currentPage = 0;
        isTutorialActive = false;
        PauseController.Paused = false;
        tutorialPanel.SetActive(false);
        interactionAction.action.performed += OnInteractionPerformed; // 튜토리얼 종료시 이벤트 다시 등록
    }

    // Interaction 트리거 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactionAction.action.performed += OnInteractionPerformed;

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
        if (other.CompareTag("Player"))
        {
            interactionAction.action.performed -= OnInteractionPerformed;

            if (interactionPromptCanvasGroup != null)
            {
                interactionPromptCanvasGroup.DOKill();
            }

            interactionPromptCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                interactionPrompt.SetActive(false);
            });
        }
    }

    private void OnInteractionPerformed(InputAction.CallbackContext context)
    {
        if (!isTutorialActive)
        {
            ShowTutorial();
        }
    }

    // ==================== 프롬프트

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
        var interactAction = interactionAction.action;
        for (int i = 0; i < interactAction.bindings.Count; i++)
        {
            if (interactAction.bindings[i].path.Contains("Keyboard"))
            {
                string keyName = interactAction.GetBindingDisplayString(i);
                fillerText.text = keyName;
                promptText.text = "튜토리얼";
                break;
            }
        }
    }
}
