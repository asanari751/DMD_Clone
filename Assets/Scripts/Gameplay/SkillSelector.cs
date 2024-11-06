using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Linq;

public class SkillSelector : MonoBehaviour
{
    [System.Serializable]
    public class Skilldata
    {
        public string skillName;
        public Sprite skillIcon;
        public int skillLevel;
        public string skillDescription;
    }

    [System.Serializable]
    public class SkillLevelColor
    {
        public Color color;
    }

    // [SerializeField] private GameObject darkBackground;
    [SerializeField] private float productionTime = 1f;
    [SerializeField] private float maximizeButton = 1.1f;
    [SerializeField] private float buttonEdge = 10f;
    [SerializeField] private SkillLevelColor[] skillLevelColors = new SkillLevelColor[3];

    [Header("Dices")]
    [SerializeField] private Button redDiceButton;
    [SerializeField] private Button blueDiceButton;
    [SerializeField] private TextMeshProUGUI redDiceCountText;
    [SerializeField] private TextMeshProUGUI blueDiceCountText;

    // 임시용 화면 가리기, 재가공 필수
    [SerializeField] private Image tempBlackImage;

    public GameObject skillSelectorUI;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillDescriptionTexts;
    public List<Skilldata> availableSkills;

    public Transform skillPanel;
    public Transform skillInventory;
    public List<Image> skillIconImages = new List<Image>();
    public List<Image> skillInventoryIcons = new List<Image>();
    public int currentSkillSlot = 0;

    private int maxRedDiceCount = 3;
    private int maxBlueDiceCount = 3;
    private int currentRedDiceCount;
    private int currentBlueDiceCount;
    private List<Skilldata> currentSkillSet = new List<Skilldata>();

    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private bool isProducing = false;
    private PauseController pauseController;
    private ChangeCursor cursorManager;

    private void Start()
    {
        cursorManager = FindAnyObjectByType<ChangeCursor>();
        Experience experienceComponent = FindAnyObjectByType<Experience>();
        pauseController = FindAnyObjectByType<PauseController>();
        if (experienceComponent != null)
        {
            experienceComponent.onLevelUp.AddListener(ShowSkillSelector);
        }

        skillSelectorUI.SetActive(false);
        // darkBackground.gameObject.SetActive(false);

        currentRedDiceCount = maxRedDiceCount;
        currentBlueDiceCount = maxBlueDiceCount;

        redDiceButton.onClick.AddListener(() => RollDice(true));
        blueDiceButton.onClick.AddListener(() => RollDice(false));

        UpdateDiceUI();

        for (int j = 1; j <= 4; j++)
        {
            Transform iconTransform = skillPanel.Find($"Skill Icon {j}");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    skillIconImages.Add(iconImage);
                    skillInventoryIcons.Add(iconImage);
                }
            }

            originalPositions = new Vector3[skillButtons.Length];
            originalScales = new Vector3[skillButtons.Length];

            for (int i = 0; i < skillButtons.Length; i++)
            {
                originalPositions[i] = skillButtons[i].transform.position;
                originalScales[i] = skillButtons[i].transform.localScale;
                AddHoverListeners(skillButtons[i]);
            }
        }
    }

    private void ShowSkillSelector()
    {
        ResetSkillButtons();
        skillSelectorUI.SetActive(true);
        tempBlackImage.gameObject.SetActive(true);
        pauseController.ToggleUIState();

        cursorManager.SetNormalCursor();

        currentRedDiceCount = maxRedDiceCount;
        currentBlueDiceCount = maxBlueDiceCount;

        ShuffleSkills();
        currentSkillSet = new List<Skilldata>(availableSkills.Take(3));

        UpdateSkillUI();
        UpdateDiceUI();
        StartProduction();
    }

    private void ShuffleSkills() // 순서 섞기
    {
        System.Random rng = new System.Random();
        int n = availableSkills.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Skilldata value = availableSkills[k];
            availableSkills[k] = availableSkills[n];
            availableSkills[n] = value;
        }
    }

    private void ResetSkillButtons()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.localScale = originalScales[i];
            RemoveColor(skillButtons[i].gameObject);
        }
    }

    public void StartProduction()
    {
        if (!isProducing)
        {
            StartCoroutine(ProductionCoroutine());
        }
    }
    private IEnumerator ProductionCoroutine()
    {
        isProducing = true;

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].gameObject.SetActive(false);
            skillButtons[i].transform.position = originalPositions[i] + Vector3.up * 1200f;
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].gameObject.SetActive(true);
            MoveButtonDown(skillButtons[i], i);
            yield return new WaitForSecondsRealtime(productionTime / skillButtons.Length);
        }

        yield return new WaitForSecondsRealtime(productionTime);
        isProducing = false;
    }

    // private void MoveButtonDown(Button button, int index)
    // {
    //     button.transform.DOMove(originalPositions[index], productionTime)
    //         .SetEase(Ease.OutBack).SetUpdate(true);
    // }

    private void MoveButtonDown(Button button, int index)
    {
        button.transform.DOMove(originalPositions[index], productionTime)
            .SetEase(Ease.OutQuart).SetUpdate(true);
    }

    private void KillAllButtonAnimations()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.DOKill();
        }
    }

    private void SelectSkill(int index)
    {
        Skilldata selectedSkill = availableSkills[index];
        PlayerSkills.Instance.AddOrUpgradeSkill(selectedSkill.skillName);
        UpdateSkillUI();

        bool isAlreadySelected = false;
        for (int i = 0; i < currentSkillSlot; i++)
        {
            if (skillIconImages[i].sprite == selectedSkill.skillIcon)
            {
                isAlreadySelected = true;
                break;
            }
        }

        if (!isAlreadySelected && currentSkillSlot < skillIconImages.Count)
        {
            skillIconImages[currentSkillSlot].sprite = selectedSkill.skillIcon;
            currentSkillSlot++;
        }

        pauseController.ToggleUIState();
        skillSelectorUI.SetActive(false);
        tempBlackImage.gameObject.SetActive(false);
        cursorManager.SetCombatCursor();
        // darkBackground.gameObject.SetActive(false);
    }

    private void UpdateSkillUI()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < availableSkills.Count)
            {
                skillButtons[i].gameObject.SetActive(true);

                Skilldata skill = availableSkills[i];
                int skillLevel = PlayerSkills.Instance.GetSkillLevel(skill.skillName);

                TextMeshProUGUI buttonText = skillButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = skill.skillName;
                }

                Transform skillTitleTransform = skillButtons[i].transform.Find("Skill Title - TMP");
                if (skillTitleTransform != null)
                {
                    TextMeshProUGUI skillTitleText = skillTitleTransform.GetComponent<TextMeshProUGUI>();
                    if (skillTitleText != null)
                    {
                        skillTitleText.text = skill.skillName;
                    }
                }

                Transform skillIconTransform = skillButtons[i].transform.Find("Skill Icon");
                if (skillIconTransform != null)
                {
                    Transform iconSpriteTransform = skillIconTransform.Find("Icon - Sprite");
                    if (iconSpriteTransform != null)
                    {
                        Image skillIconImage = iconSpriteTransform.GetComponent<Image>();
                        if (skillIconImage != null)
                        {
                            skillIconImage.sprite = skill.skillIcon;
                        }
                    }

                    Transform iconBackgroundTransform = skillIconTransform.Find("Icon Background");
                    if (iconBackgroundTransform != null)
                    {
                        Image iconBackgroundImage = iconBackgroundTransform.GetComponent<Image>();
                        if (iconBackgroundImage != null)
                        {
                            int colorIndex = Mathf.Clamp(skillLevel - 1, 0, skillLevelColors.Length - 1);
                            iconBackgroundImage.color = skillLevelColors[colorIndex].color;
                        }
                    }
                }

                if (skillDescriptionTexts.Length > i && skillDescriptionTexts[i] != null)
                {
                    string coloredDescription = skill.skillDescription
                        .Replace("출혈", "<color=#DC143C>출혈</color>")
                        .Replace("둔화", "<color=#4472C4>둔화</color>");
                    skillDescriptionTexts[i].text = coloredDescription;
                }

                int index = i;
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() => SelectSkill(index));
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void AddHoverListeners(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { if (!isProducing) SelectSkill(System.Array.IndexOf(skillButtons, button)); });

        button.transition = Selectable.Transition.None;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)] * maximizeButton, 0.2f).SetUpdate(true);
                AddColor(button.gameObject, buttonEdge);
            }
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)], 0.2f).SetUpdate(true);
                RemoveColor(button.gameObject);
            }
        });
        trigger.triggers.Add(exitEntry);
    }

    private void AddColor(GameObject obj, float edgeSize)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(edgeSize, -edgeSize);
        outline.useGraphicAlpha = false;
        outline.enabled = true;
    }

    private void RemoveColor(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    // ============================== Dice

    private void RollDice(bool isRed)
    {
        if ((isRed && currentRedDiceCount <= 0) || (!isRed && currentBlueDiceCount <= 0))
            return;

        if (isRed)
            currentRedDiceCount--;
        else
            currentBlueDiceCount--;

        currentSkillSet = new List<Skilldata>(availableSkills.Take(3));

        StartCoroutine(RollDiceAnimation());
    }

    private IEnumerator RollDiceAnimation()
    {
        isProducing = true;
        KillAllButtonAnimations();

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.position = originalPositions[i] + Vector3.up * 1200f;
        }

        int maxAttempts = 10;
        int currentAttempt = 0;
        do
        {
            ShuffleSkills();
            currentAttempt++;
            if (currentAttempt >= maxAttempts)
            {
                Debug.Log($"주사위 굴리기 최대 시도 횟수({maxAttempts}회) 도달");
                break;
            }
        } while (IsIdenticalSkillSet());

        UpdateSkillUI();
        UpdateDiceUI();

        for (int i = 0; i < skillButtons.Length; i++)
        {
            MoveButtonDown(skillButtons[i], i);
            yield return new WaitForSecondsRealtime(productionTime / skillButtons.Length);
        }

        yield return new WaitForSecondsRealtime(productionTime);
        isProducing = false;
    }

    private bool IsIdenticalSkillSet()
    {
        // 현재 보여지는 3개의 스킬이 이전과 동일한지 확인
        var newSkillSet = availableSkills.Take(3);
        return currentSkillSet.All(skill => newSkillSet.Any(s => s.skillName == skill.skillName))
               && newSkillSet.All(skill => currentSkillSet.Any(s => s.skillName == skill.skillName));
    }

    private void UpdateDiceUI()
    {
        redDiceCountText.text = $"{currentRedDiceCount}/{maxRedDiceCount}";
        blueDiceCountText.text = $"{currentBlueDiceCount}/{maxBlueDiceCount}";

        redDiceButton.interactable = currentRedDiceCount > 0;
        blueDiceButton.interactable = currentBlueDiceCount > 0;
    }

}