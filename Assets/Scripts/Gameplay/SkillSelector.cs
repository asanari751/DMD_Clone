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
        public SkillData skillDataSO; // SkillData 스크립터블 오브젝트 참조

        // 스크립터블 오브젝트에서 데이터 가져오기
        public string skillName => skillDataSO.skillName;
        public Sprite skillIcon => skillDataSO.skillIcon;
        public int skillLevel => skillDataSO.skillLevel;
        public string skillDescription => skillDataSO.skillDescription;
    }

    [System.Serializable]
    public class SkillLevelColor
    {
        public Color color;
    }

    [System.Serializable]
    public class GodData
    {
        public int godId;
        public Image godImage;
        public string godName;
        public Sprite skillButtonImage;
        public Sprite selectedButtonImage;
        [TextArea(3, 10)]
        public string[] dialogueTexts;
        public List<Skilldata> availableSkills;
    }

    public static SkillSelector Instance { get; private set; }

    // [SerializeField] private GameObject darkBackground;
    [SerializeField] private float productionTime = 1f;
    [SerializeField] private float maximizeButton = 1.1f;
    [SerializeField] private float buttonEdge = 10f;
    [SerializeField] private SkillLevelColor[] skillLevelColors = new SkillLevelColor[3];
    [SerializeField] private TMP_Text selectorTitle;
    [SerializeField] private GameObject cooldownPrefab;
    [SerializeField] private Sprite clawSkillIcon;
    [SerializeField] private Sprite swordSkillIcon;
    [SerializeField] private Sprite arrowSkillIcon;
    [SerializeField] private GameObject spinnerPrefab;
    [SerializeField] private AudioManager audioManager;

    [Header("Dices")]
    [SerializeField] private Button redDiceButton;
    [SerializeField] private Button blueDiceButton;
    [SerializeField] private TextMeshProUGUI redDiceCountText;
    [SerializeField] private TextMeshProUGUI blueDiceCountText;

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionField;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text godNameText;
    [SerializeField] private GodData[] gods;

    // 임시용 화면 가리기, 재가공 필수
    [SerializeField] private Image tempBlackImage;
    [Header("Status Effect Colors")]
    [SerializeField] private Color bleedColor = new Color(0.86f, 0.08f, 0.24f);
    [SerializeField] private Color slowColor = new Color(0.27f, 0.45f, 0.77f);
    [SerializeField] private Color debilitateColor = new Color(0.08f, 0.63f, 0.08f);
    [SerializeField] private Color VenomColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color fearColor = new Color(0.86f, 0.08f, 0.24f);
    [SerializeField] private Color stiffColor = new Color(0.27f, 0.45f, 0.77f);

    public GameObject skillSelectorUI;
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Sprite selectedButtonSprite;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillLevelTexts;
    public TextMeshProUGUI[] skillDescriptionTexts;

    public Transform skillPanel;
    public Transform skillInventory;
    private List<Image> skillIconImages = new List<Image>();
    private Image basicAttackCooldown;
    private Dictionary<int, Image> skillCooldownImages = new Dictionary<int, Image>();
    private List<Image> skillInventoryIcons = new List<Image>();
    public int currentSkillSlot = 0;

    private int maxRedDiceCount = 99;
    private int maxBlueDiceCount = 99;
    private int currentRedDiceCount;
    private int currentBlueDiceCount;
    private int previousGodIndex = -1;
    private List<Skilldata> currentSkillSet = new List<Skilldata>();

    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private bool isProducing = false;
    private PauseController pauseController;
    private ChangeCursor cursorManager;

    private Tween textTween;
    private bool isDialogueComplete = false;
    private Dictionary<int, int> lastDialogueIndices = new Dictionary<int, int>();
    private float repeatWeight = 0.5f;
    private int currentGodIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        cursorManager = FindAnyObjectByType<ChangeCursor>();
        Experience experienceComponent = FindAnyObjectByType<Experience>();
        if (experienceComponent != null)
        {
            experienceComponent.onSkillSelectLevel.AddListener(StartInteraction);
        }

        pauseController = FindAnyObjectByType<PauseController>();

        skillSelectorUI.SetActive(false);
        // darkBackground.gameObject.SetActive(false);

        currentRedDiceCount = maxRedDiceCount;
        currentBlueDiceCount = maxBlueDiceCount;

        redDiceButton.onClick.AddListener(() => RollDice(true));
        blueDiceButton.onClick.AddListener(() => RollDice(false));

        UpdateDiceUI();

        PlayerStateManager playerStateManager = FindAnyObjectByType<PlayerStateManager>();

        Transform firstIconTransform = skillPanel.Find("Skill Icon 1");
        if (firstIconTransform != null)
        {
            Image firstIconImage = firstIconTransform.GetComponent<Image>();
            if (firstIconImage != null)
            {
                skillIconImages.Add(firstIconImage);
                skillInventoryIcons.Add(firstIconImage);

                GameObject cooldown = Instantiate(cooldownPrefab, firstIconImage.transform);
                basicAttackCooldown = cooldown.GetComponent<Image>();

                if (playerStateManager != null)
                {
                    switch (playerStateManager.currentAttackType)
                    {
                        case PlayerStateManager.AttackType.Claw:
                            firstIconImage.sprite = GetClawSkillIcon();
                            break;
                        case PlayerStateManager.AttackType.Sword:
                            firstIconImage.sprite = GetSwordSkillIcon();
                            break;
                        case PlayerStateManager.AttackType.Arrow:
                            firstIconImage.sprite = GetArrowSkillIcon();
                            break;
                    }
                    currentSkillSlot = 1;  // 다음 스킬은 1번부터 시작
                }
            }
        }

        for (int j = 2; j <= 4; j++)
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

    // 기본공격 아이콘 반환

    private Sprite GetClawSkillIcon()
    {
        return clawSkillIcon;
    }

    private Sprite GetSwordSkillIcon()
    {
        return swordSkillIcon;
    }

    private Sprite GetArrowSkillIcon()
    {
        return arrowSkillIcon;
    }

    // 인터랙션 영역

    private void StartInteraction()
    {
        isDialogueComplete = false;

        if (!interactionField.activeSelf)
        {
            pauseController.ToggleUIState();
        }

        int randomGodIndex;
        do
        {
            randomGodIndex = Random.Range(0, gods.Length);
        } while (randomGodIndex == previousGodIndex);

        previousGodIndex = randomGodIndex;
        GodData selectedGod = gods[randomGodIndex];

        currentGodIndex = randomGodIndex;

        string selectedDialogue = GetWeightedRandomDialogue(randomGodIndex);

        interactionField.SetActive(true);
        audioManager.PlayAmbient("A9");
        backgroundOverlay.gameObject.SetActive(true);

        DisplayGodDialogue(selectedGod, selectedDialogue);
    }

    private string GetWeightedRandomDialogue(int godIndex)
    {
        float[] weights = new float[3];
        int lastIndex;

        if (!lastDialogueIndices.TryGetValue(godIndex, out lastIndex))
        {
            lastIndex = -1;
        }

        for (int i = 0; i < 3; i++)
        {
            weights[i] = (i == lastIndex) ? repeatWeight : 1f;
        }

        float totalWeight = weights.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < 3; i++)
        {
            currentSum += weights[i];
            if (randomValue <= currentSum)
            {
                lastDialogueIndices[godIndex] = i;
                return gods[godIndex].dialogueTexts[i];
            }
        }

        return gods[godIndex].dialogueTexts[0];
    }

    private void DisplayGodDialogue(GodData selectedGod, string dialogue)
    {
        // 모든 신의 이미지를 비활성화
        foreach (var god in gods)
        {
            god.godImage.gameObject.SetActive(false);
        }

        // 선택된 신만 활성화
        selectedGod.godImage.gameObject.SetActive(true);

        // 선택된 신의 이름 표시
        godNameText.text = selectedGod.godName;
        selectorTitle.text = selectedGod.godName;

        dialogueText.text = dialogue;
        dialogueText.maxVisibleCharacters = 0;

        float duration = dialogue.Length * typingSpeed;

        textTween = DOTween.To(() => dialogueText.maxVisibleCharacters,
            x => dialogueText.maxVisibleCharacters = x,
            dialogue.Length,
            duration)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() => isDialogueComplete = true);
    }

    private void Update()
    {
        if (interactionField.activeSelf && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            if (isDialogueComplete)
            {
                interactionField.SetActive(false);
                backgroundOverlay.gameObject.SetActive(false);
                ShowSkillSelector();
            }
            else
            {
                if (textTween != null && textTween.IsActive())
                {
                    textTween.Complete();
                    isDialogueComplete = true;
                }
            }
        }
    }

    // 스킬 선택 영역

    private void ShowSkillSelector()
    {
        ResetSkillButtons();
        skillSelectorUI.SetActive(true);
        tempBlackImage.gameObject.SetActive(true);

        cursorManager.SetNormalCursor();

        GodData currentGod = gods[currentGodIndex];
        List<Skilldata> selectedGodSkills = currentGod.availableSkills;

        foreach (Button button in skillButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = currentGod.skillButtonImage;
            }
        }

        ShuffleSkills(selectedGodSkills);
        currentSkillSet = new List<Skilldata>(selectedGodSkills.Take(3));

        UpdateSkillUI();
        UpdateDiceUI();
        StartProduction();
    }

    private void ShuffleSkills(List<Skilldata> skillsToShuffle)
    {
        // 레벨 3 이하인 스킬만 필터링
        skillsToShuffle.RemoveAll(skill => PlayerSkills.Instance.GetSkillLevel(skill.skillName) >= 3);

        System.Random rng = new System.Random();
        int n = skillsToShuffle.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Skilldata value = skillsToShuffle[k];
            skillsToShuffle[k] = skillsToShuffle[n];
            skillsToShuffle[n] = value;
        }
    }

    private void ResetSkillButtons()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.localScale = originalScales[i];
            // RemoveColor(skillButtons[i].gameObject);
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
        Skilldata selectedSkill = currentSkillSet[index];

        PlayerSkills.Instance.AddOrUpgradeSkill(selectedSkill.skillDataSO);
        UpdateSkillUI();
        audioManager.PlayAmbient("A8");

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

            if (selectedSkill.skillDataSO.skillRangeType != SkillData.SkillRangeType.Self)
            {
                GameObject cooldown = Instantiate(cooldownPrefab, skillIconImages[currentSkillSlot].transform);
                skillCooldownImages[currentSkillSlot - 1] = cooldown.GetComponent<Image>();
            }

            currentSkillSlot++;
        }

        pauseController.ToggleUIState();
        skillSelectorUI.SetActive(false);
        tempBlackImage.gameObject.SetActive(false);
        cursorManager.SetCombatCursor();
    }

    private void UpdateSkillUI()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < currentSkillSet.Count)
            {
                skillButtons[i].gameObject.SetActive(true);

                Skilldata skill = currentSkillSet[i];
                int skillLevel = PlayerSkills.Instance.GetSkillLevel(skill.skillName);

                if (skillLevelTexts.Length > i && skillLevelTexts[i] != null)
                {
                    skillLevelTexts[i].text = $"레벨 {skillLevel + 1}";
                }

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
    .Replace("출혈", $"<color=#{ColorUtility.ToHtmlStringRGB(bleedColor)}>출혈</color>")
    .Replace("둔화", $"<color=#{ColorUtility.ToHtmlStringRGB(slowColor)}>둔화</color>")
    .Replace("쇠약", $"<color=#{ColorUtility.ToHtmlStringRGB(debilitateColor)}>쇠약</color>")
    .Replace("중독", $"<color=#{ColorUtility.ToHtmlStringRGB(VenomColor)}>중독</color>")
    .Replace("공포", $"<color=#{ColorUtility.ToHtmlStringRGB(fearColor)}>공포</color>")
    .Replace("경직", $"<color=#{ColorUtility.ToHtmlStringRGB(stiffColor)}>경직</color>");
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

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)] * maximizeButton, 0.2f).SetUpdate(true);
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = gods[currentGodIndex].selectedButtonImage;
                }
            }
        });

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)], 0.2f).SetUpdate(true);
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = gods[currentGodIndex].skillButtonImage;
                }
            }
        });
        trigger.triggers.Clear();
        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    // private void AddColor(GameObject obj, float edgeSize)
    // {
    //     Outline outline = obj.GetComponent<Outline>();
    //     if (outline == null)
    //     {
    //         outline = obj.AddComponent<Outline>();
    //     }
    //     // 반투명 효과를 준 색상 사용
    //     outline.effectColor = new Color(1f, 1f, 1f, 0.7f);
    //     outline.effectDistance = new Vector2(edgeSize, -edgeSize);
    //     outline.useGraphicAlpha = true;
    //     outline.enabled = true;
    // }

    // private void RemoveColor(GameObject obj)
    // {
    //     Outline outline = obj.GetComponent<Outline>();
    //     if (outline != null)
    //     {
    //         outline.enabled = false;
    //     }
    // }

    // ============================== Spinner

    public void ToggleAutoAttackIndicator(bool isActive)
    {
        Transform firstIconTransform = skillPanel.Find("Skill Icon 1");
        if (firstIconTransform != null)
        {
            Transform existingSpinner = firstIconTransform.Find("AutoAttackSpinner");
            if (existingSpinner != null)
                Destroy(existingSpinner.gameObject);

            if (isActive)
            {
                GameObject spinner = Instantiate(spinnerPrefab, firstIconTransform);
                spinner.name = "AutoAttackSpinner";
                spinner.transform.SetAsLastSibling();
            }
        }
    }

    // ============================== Skill Hover

    public SkillData GetSkillDataByIndex(int index)
    {
        // 스킬 (인덱스 1-3)
        foreach (var skill in currentSkillSet)
        {
            if (skillIconImages[index].sprite == skill.skillIcon)
            {
                return skill.skillDataSO;
            }
        }

        return null;
    }
    // ============================== Cooldown

    // 기본 공격 쿨다운 업데이트
    public void UpdateBasicAttackCooldown(float currentCooldown, float maxCooldown)
    {
        if (basicAttackCooldown != null)
        {
            float fillAmount = Mathf.Clamp01(1 - (currentCooldown / maxCooldown));
            basicAttackCooldown.fillAmount = fillAmount;
        }
    }

    // 스킬 쿨다운 업데이트
    public void UpdateSkillCooldown(int skillIndex, float currentCooldown, float maxCooldown)
    {
        if (skillCooldownImages.ContainsKey(skillIndex))
        {
            float fillAmount = Mathf.Clamp01(1 - (currentCooldown / maxCooldown));
            skillCooldownImages[skillIndex].fillAmount = fillAmount;
        }
    }

    // ============================== Dice

    private void RollDice(bool isRed)
    {
        if ((isRed && currentRedDiceCount <= 0) || (!isRed && currentBlueDiceCount <= 0))
            return;

        if (isRed)
        {
            currentRedDiceCount--;
            audioManager.PlayAmbient("A10");
            currentSkillSet = new List<Skilldata>(gods[currentGodIndex].availableSkills.Take(3));
            StartCoroutine(RollDiceAnimation());
            UpdateDiceUI();
        }

        else
        {
            currentBlueDiceCount--;
            audioManager.PlayAmbient("A11");
            skillSelectorUI.SetActive(false);
            tempBlackImage.gameObject.SetActive(false);

            interactionField.SetActive(true);
            backgroundOverlay.gameObject.SetActive(true);
            StartInteraction();
            UpdateDiceUI();
        }
    }

    private IEnumerator RollDiceAnimation()
    {
        isProducing = true;
        KillAllButtonAnimations();

        float moveDistance = 100f;
        float animationDuration = 0.5f;

        // 현재 진행 중인 모든 트윈 즉시 완료
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.DOKill(true);
        }

        // 모든 버튼을 원래 위치로 즉시 이동
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.position = originalPositions[i];
        }

        // 새로운 애니메이션 시작
        for (int i = 0; i < skillButtons.Length; i++)
        {
            Button button = skillButtons[i];
            button.transform.DOMoveY(originalPositions[i].y + moveDistance, animationDuration / 2)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    button.transform.DOMoveY(originalPositions[System.Array.IndexOf(skillButtons, button)].y, animationDuration / 2)
                        .SetEase(Ease.InQuad)
                        .SetUpdate(true);
                });
        }

        int maxAttempts = 10;
        int currentAttempt = 0;

        List<Skilldata> selectedGodSkills = gods[currentGodIndex].availableSkills;

        do
        {
            ShuffleSkills(selectedGodSkills);
            currentAttempt++;
            if (currentAttempt >= maxAttempts)
            {
                Debug.Log($"주사위 굴리기 최대 시도 횟수({maxAttempts}회) 도달");
                break;
            }
        } while (IsIdenticalSkillSet());

        currentSkillSet = new List<Skilldata>(selectedGodSkills.Take(3));

        UpdateSkillUI();
        UpdateDiceUI();

        yield return new WaitForSecondsRealtime(animationDuration);

        isProducing = false;
    }

    private bool IsIdenticalSkillSet()
    {
        // 현재 보여지는 3개의 스킬이 이전과 동일한지 확인
        var newSkillSet = currentSkillSet;
        return currentSkillSet.All(skill => newSkillSet.Any(s => s.skillName == skill.skillName))
               && newSkillSet.All(skill => currentSkillSet.Any(s => s.skillName == skill.skillName));
    }

    private void UpdateDiceUI()
    {
        redDiceCountText.text = $"{currentRedDiceCount}";
        blueDiceCountText.text = $"{currentBlueDiceCount}";

        redDiceButton.interactable = currentRedDiceCount > 0;
        blueDiceButton.interactable = currentBlueDiceCount > 0;
    }

}