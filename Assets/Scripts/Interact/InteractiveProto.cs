using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InteractiveProto : MonoBehaviour
{
    public GameObject interactiveSpritePrefab; // 상호작용 가능 표시 UI 프리팹
    public GameObject commonUIPanel; // 상호작용 시 표시될 UI 패널 프리팵
    public float yOffset = 30f; // y축 오프셋 (픽셀 단위)
    public string customText = "상호작용"; // 인스펙터에서 설정할 커스텀 텍스트
    public float horizontalPadding = 10f; // 텍스트 좌우 여백 (픽셀 단위)
    public Vector2 panelBackgroundSize = new Vector2(800, 600); // 기본값 설정, 인스펙터에서 조정 가능

    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private GameTimerController gameTimerController;

    private GameObject spawnedSprite; // 생성된 상호작용 가능 표시 UI의 참조
    private GameObject spawnedCommonPanel; // 생성된 공통 UI 패널의 참조
    private GameObject darkOverlay; // 어두운 오버레이 게임 오브젝트
    private Canvas overlayCanvas; // 오버레이 캔버스
    private bool isPlayerInRange = false;
    private bool isInteracting = false;

    private void Start()
    {
        CreateOverlayCanvas();
        CreateDarkOverlay();
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
        interactAction.action.performed -= OnInteract;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            SpawnInteractiveSprite();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            DestroyInteractiveSprite();
            if (isInteracting)
            {
                EndInteraction();
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isPlayerInRange)
        {
            if (!isInteracting)
            {
                StartInteraction();
            }
            else
            {
                EndInteraction();
            }
        }
    }

    private void StartInteraction()
    {
        DestroyInteractiveSprite(); // 상호작용 가능 표시 UI 제거
        SpawnCommonUIPanel(); // 상호작용 UI 패널 생성
        ShowDarkOverlay(); // 어두운 오버레이 표시
        if (gameTimerController != null)
        {
            gameTimerController.PauseGame();
        }
        else
        {
            Debug.LogError("GameTimerController is not assigned to InteractiveProto!");
        }
        isInteracting = true;
    }

    private void EndInteraction()
    {
        DestroyCommonUIPanel(); // 상호작용 UI 패널 제거
        HideDarkOverlay(); // 어두운 오버레이 숨기기
        if (gameTimerController != null)
        {
            gameTimerController.ResumeGame();
        }
        isInteracting = false;
        if (isPlayerInRange)
        {
            SpawnInteractiveSprite(); // 상호작용 가능 표시 UI 다시 생성
        }
    }

    private void CreateOverlayCanvas()
    {
        GameObject overlayCanvasObject = new GameObject("OverlayCanvas");
        overlayCanvas = overlayCanvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvasObject.AddComponent<CanvasScaler>();
        overlayCanvasObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateDarkOverlay()
    {
        darkOverlay = new GameObject("DarkOverlay");
        darkOverlay.transform.SetParent(overlayCanvas.transform, false);
        Image image = darkOverlay.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.5f); // 50% 투명도의 검은색
        RectTransform rectTransform = darkOverlay.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        darkOverlay.SetActive(false);
    }

    private void ShowDarkOverlay()
    {
        if (darkOverlay != null)
        {
            darkOverlay.SetActive(true);
        }
    }

    private void HideDarkOverlay()
    {
        if (darkOverlay != null)
        {
            darkOverlay.SetActive(false);
        }
    }

    private void SpawnInteractiveSprite()
    {
        if (interactiveSpritePrefab != null && spawnedSprite == null && overlayCanvas != null)
        {
            spawnedSprite = Instantiate(interactiveSpritePrefab, overlayCanvas.transform);
            
            RectTransform rectTransform = spawnedSprite.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3 worldPosition = transform.position + new Vector3(0, yOffset / 100f, 0);
                rectTransform.position = worldPosition;

                float originalHeight = rectTransform.rect.height;

                TextMeshProUGUI tmpComponent = spawnedSprite.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpComponent != null)
                {
                    tmpComponent.text = customText;
                    
                    tmpComponent.ForceMeshUpdate();
                    float textWidth = tmpComponent.preferredWidth;

                    float width = textWidth + (horizontalPadding * 2);

                    rectTransform.sizeDelta = new Vector2(width, originalHeight);

                    RectTransform textRectTransform = tmpComponent.GetComponent<RectTransform>();
                    textRectTransform.sizeDelta = new Vector2(textWidth, textRectTransform.sizeDelta.y);
                }

                Image backgroundImage = spawnedSprite.GetComponent<Image>();
                if (backgroundImage != null)
                {
                    backgroundImage.rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, backgroundImage.rectTransform.sizeDelta.y);
                }
            }
        }
    }

    private void SpawnCommonUIPanel()
    {
        if (commonUIPanel != null && spawnedCommonPanel == null && overlayCanvas != null)
        {
            spawnedCommonPanel = Instantiate(commonUIPanel, overlayCanvas.transform);
            
            RectTransform rectTransform = spawnedCommonPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero; // 화면 중앙에 위치
            }

            // Panel Background의 크기 조정
            Transform panelBackgroundTransform = spawnedCommonPanel.transform.Find("Panel Background");
            if (panelBackgroundTransform != null)
            {
                RectTransform panelBackgroundRect = panelBackgroundTransform.GetComponent<RectTransform>();
                if (panelBackgroundRect != null)
                {
                    panelBackgroundRect.sizeDelta = panelBackgroundSize;
                }
                else
                {
                    Debug.LogWarning("RectTransform component not found on Panel Background");
                }
            }
            else
            {
                Debug.LogWarning("Panel Background not found in commonUIPanel prefab");
            }

            // Panel Textbox의 텍스트 변경 (기존 코드)
            Transform panelTextboxTransform = spawnedCommonPanel.transform.Find("Panel Background/Panel Textbox");
            if (panelTextboxTransform != null)
            {
                TextMeshProUGUI panelTextbox = panelTextboxTransform.GetComponent<TextMeshProUGUI>();
                if (panelTextbox != null)
                {
                    panelTextbox.text = customText;
                }
                else
                {
                    Debug.LogWarning("TextMeshProUGUI component not found on Panel Textbox");
                }
            }
            else
            {
                Debug.LogWarning("Panel Textbox not found in commonUIPanel prefab");
            }
        }
    }

    private void DestroyInteractiveSprite()
    {
        if (spawnedSprite != null)
        {
            Destroy(spawnedSprite);
            spawnedSprite = null;
        }
    }

    private void DestroyCommonUIPanel()
    {
        if (spawnedCommonPanel != null)
        {
            Destroy(spawnedCommonPanel);
            spawnedCommonPanel = null;
        }
    }

    private void OnDestroy()
    {
        if (overlayCanvas != null)
        {
            Destroy(overlayCanvas.gameObject);
        }
    }
}
