using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterInfo : MonoBehaviour
{
    public enum GodType
    {
        Bathory = 0,
        Strigoi = 1,
        Vlad = 2,
    }

    public struct PlayerStatData
    {
        // 1. Attack
        public float damageMultiplier;
        public float criticalDamage;
        public float criticalChance;
        public float range;

        // 2. Defense
        public float maxHealth;
        public float armor;
        public float reduction;
        public float dodge;

        // 3. Default Attack
        public float defaultDamage;
        public float defaultAttackSpeed;
        public float defaultRange;
        public float defaultCriticalChance;

        // 4. Utility
        public float moveSpeed;
        public float lootRange;
        public float dashRange;
        public float dashCount;

    }

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject InfoUI;
    [SerializeField] private InputActionReference toggleInfoAction;
    [SerializeField] private bool InfoVisible = false;
    [SerializeField] private PauseController pauseController;
    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private PlayerStatData currentStats;

    [Header("Icon")]
    [SerializeField] private Sprite[] GodIcons;
    [SerializeField] private Image IconFiller;
    [SerializeField] private TMP_Text godName;

    [Header("Skill Inventory")]
    [SerializeField] private GameObject skillInventory;
    [SerializeField] private GameObject bathoryContainer;
    [SerializeField] private GameObject strigoiContainer;
    [SerializeField] private GameObject vladContainer;
    [SerializeField] private Button undoButton;

    // =======================================

    [Header("Attack")]
    [SerializeField] private TextMeshProUGUI attackDamageLabel;
    [SerializeField] private TextMeshProUGUI attackDamageValue;
    [SerializeField] private TextMeshProUGUI criticalChanceLabel;
    [SerializeField] private TextMeshProUGUI criticalChanceValue;
    [SerializeField] private TextMeshProUGUI criticalDamageLabel;
    [SerializeField] private TextMeshProUGUI criticalDamageValue;
    [SerializeField] private TextMeshProUGUI rangeLabel;
    [SerializeField] private TextMeshProUGUI rangeValue;

    [Header("Defense")]
    [SerializeField] private TextMeshProUGUI maxHealthLabel;
    [SerializeField] private TextMeshProUGUI maxHealthValue;
    [SerializeField] private TextMeshProUGUI armorLabel;
    [SerializeField] private TextMeshProUGUI armorValue;
    [SerializeField] private TextMeshProUGUI reductionLabel;
    [SerializeField] private TextMeshProUGUI reductionValue;
    [SerializeField] private TextMeshProUGUI dodgeLabel;
    [SerializeField] private TextMeshProUGUI dodgeValue;

    [Header("Default")]
    [SerializeField] private TextMeshProUGUI defaultDamageLabel;
    [SerializeField] private TextMeshProUGUI defaultDamageValue;
    [SerializeField] private TextMeshProUGUI defaultASPDLabel;
    [SerializeField] private TextMeshProUGUI defaultASPDValue;
    [SerializeField] private TextMeshProUGUI defaultRangeLabel;
    [SerializeField] private TextMeshProUGUI defaultRangeValue;
    [SerializeField] private TextMeshProUGUI defaultCriticalChanceLabel;
    [SerializeField] private TextMeshProUGUI defaultCriticalChanceValue;

    [Header("Utility")]
    [SerializeField] private TextMeshProUGUI moveSpeedLabel;
    [SerializeField] private TextMeshProUGUI moveSpeedValue;
    [SerializeField] private TextMeshProUGUI lootRangeLabel;
    [SerializeField] private TextMeshProUGUI lootRangeValue;
    [SerializeField] private TextMeshProUGUI dashCountLabel;
    [SerializeField] private TextMeshProUGUI dashCountValue;
    [SerializeField] private TextMeshProUGUI dashRangeLabel;
    [SerializeField] private TextMeshProUGUI dashRangeValue;


    // ========================================

    private GodType currentGod;
    private float attackDamage;


    private void Start()
    {
        InfoUI.SetActive(false);
        skillInventory.SetActive(false);

        undoButton.onClick.AddListener(() => { ToggleInfo(); });
    }

    private void Update()
    {
        UpdateGodIconBasedOnAttackType();
    }

    private void UpdateGodIconBasedOnAttackType()
    {
        // PlayerStats에서 선택된 공격 타입 가져오기
        var selectedAttackType = PlayerStats.Instance.selectedAttackType;

        // 공격 타입에 따라 GodType 결정
        GodType selectedGod = GodType.Bathory; // 기본값 설정

        switch (selectedAttackType)
        {
            case PlayerStateManager.AttackType.Claw:
                selectedGod = GodType.Strigoi; // Claw는 Strigoi
                godName.text = "스트리고이";
                break;
            case PlayerStateManager.AttackType.Arrow:
                selectedGod = GodType.Bathory; // Arrow는 Bathory\
                godName.text = "바토리 에르제베트";
                break;
            case PlayerStateManager.AttackType.Sword:
                selectedGod = GodType.Vlad; // Sword는 Vlad
                godName.text = "블라드 3세";
                break;
        }

        UpdateGodIcon(selectedGod);
    }

    private void OnEnable()
    {
        toggleInfoAction.action.performed += OnToggleInfo;  // 기존 이벤트 구독
        playerStateManager.OnStatsUpdated += UpdateCharacterInfo;  // 기존 이벤트 구독
        PlayerStats.Instance.OnAttackTypeChanged += UpdateGodIconBasedOnAttackType;  // 새로운 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 이벤트 구독 추가
    }

    private void OnDisable()
    {
        toggleInfoAction.action.performed -= OnToggleInfo;  // 기존 이벤트 해제
        playerStateManager.OnStatsUpdated -= UpdateCharacterInfo;  // 기존 이벤트 해제
        PlayerStats.Instance.OnAttackTypeChanged -= UpdateGodIconBasedOnAttackType;  // 새로운 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded; // 씬 로드 이벤트 구독 해제
    }

    private void OnToggleInfo(InputAction.CallbackContext context)
    {
        ToggleInfo();
    }

    private void ToggleInfo()
    {
        InfoVisible = !InfoVisible;
        InfoUI.SetActive(InfoVisible);
        skillInventory.SetActive(InfoVisible);

        if (InfoVisible)
        {
            audioManager.PlayAmbient("A13");
        }
        else
        {
            audioManager.PlayAmbient("A14");
        }

        pauseController.ToggleUIState();
        if (InfoVisible)
        {
            UpdateSkillInventory();
        }
    }

    private void UpdateCharacterInfo(CharacterInfo.PlayerStatData newStats)
    {
        currentStats = newStats;
        currentStats.defaultDamage = playerStateManager.GetCurrentDamage();
        UpdateInfoUI();
    }

    private void UpdateInfoUI()
    {
        attackDamageLabel.text = $"피해량:";
        criticalChanceLabel.text = $"치명타 확률:";
        criticalDamageLabel.text = $"치명타 피해:";
        rangeLabel.text = $"공격 범위:";

        attackDamageValue.text = $"{currentStats.damageMultiplier * 100}%";
        criticalChanceValue.text = $"{currentStats.criticalChance * 100}%";
        criticalDamageValue.text = $"{currentStats.criticalDamage * 100}%";
        rangeValue.text = $"{currentStats.range * 100}%";

        // =======================================

        maxHealthLabel.text = $"최대 체력:";
        armorLabel.text = $"방어력:";
        reductionLabel.text = $"피해 감소:";
        dodgeLabel.text = $"회피율:";

        maxHealthValue.text = $"{currentStats.maxHealth}";
        armorValue.text = $"{currentStats.armor}";
        reductionValue.text = $"{currentStats.reduction}%";
        dodgeValue.text = $"{currentStats.dodge}%";

        // =======================================

        defaultDamageLabel.text = $"피해량:";
        defaultASPDLabel.text = $"공격속도:";
        defaultRangeLabel.text = $"사거리:";
        defaultCriticalChanceLabel.text = $"치명타 확률:";

        defaultDamageValue.text = $"{currentStats.defaultDamage}";
        defaultASPDValue.text = $"{currentStats.defaultAttackSpeed}초";
        defaultRangeValue.text = $"{currentStats.defaultRange}";
        defaultCriticalChanceValue.text = $"{currentStats.defaultCriticalChance * 100}%";

        // =======================================

        moveSpeedLabel.text = $"이동 속도:";
        lootRangeLabel.text = $"획득 범위:";
        dashCountLabel.text = $"대쉬 횟수:";
        dashRangeLabel.text = $"대쉬 거리:";

        moveSpeedValue.text = $"{currentStats.moveSpeed}";
        lootRangeValue.text = $"{currentStats.lootRange * 100}px";
        dashCountValue.text = $"{currentStats.dashCount}회";
        dashRangeValue.text = $"{currentStats.dashRange * 100}px";
    }

    // =======================================

    public void UpdateGodIcon(GodType selectedGod)
    {
        if (IconFiller != null && GodIcons != null)
        {
            int iconIndex = (int)selectedGod;
            if (iconIndex >= 0 && iconIndex < GodIcons.Length)
            {
                IconFiller.sprite = GodIcons[iconIndex];
            }
        }
    }

    private void UpdateGodIconBasedOnAttackType(PlayerStateManager.AttackType attackType)
    {
        GodType selectedGod;
        switch (attackType)
        {
            case PlayerStateManager.AttackType.Claw:
                selectedGod = GodType.Strigoi;
                break;
            case PlayerStateManager.AttackType.Arrow:
                selectedGod = GodType.Bathory;
                break;
            case PlayerStateManager.AttackType.Sword:
                selectedGod = GodType.Vlad;
                break;
            default:
                selectedGod = GodType.Bathory;
                break;
        }
        UpdateGodIcon(selectedGod);
    }

    // =======================================

    private void UpdateSkillInventory()
    {
        foreach (Transform container in new[] {
        bathoryContainer.transform.Find("Transform"),
        strigoiContainer.transform.Find("Transform"),
        vladContainer.transform.Find("Transform")
    })
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        var playerSkills = PlayerSkills.Instance;
        if (playerSkills != null)
        {
            Dictionary<Transform, int> iconCounts = new Dictionary<Transform, int>();

            foreach (var skill in playerSkills.activeSkills)
            {
                Transform parentContainer = null;
                switch (skill.skillData.godOwner)
                {
                    case SkillData.GodOwner.Bathory:
                        parentContainer = bathoryContainer.transform.Find("Transform");
                        break;
                    case SkillData.GodOwner.Strigoi:
                        parentContainer = strigoiContainer.transform.Find("Transform");
                        break;
                    case SkillData.GodOwner.Vlad:
                        parentContainer = vladContainer.transform.Find("Transform");
                        break;
                }

                if (parentContainer != null)
                {
                    if (!iconCounts.ContainsKey(parentContainer))
                        iconCounts[parentContainer] = 0;

                    GameObject skillIcon = new GameObject("SkillIcon", typeof(Image), typeof(RectTransform));
                    skillIcon.transform.SetParent(parentContainer, false);
                    RectTransform rectTransform = skillIcon.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(iconCounts[parentContainer] * 100f, 0); // 100px 간격
                    rectTransform.sizeDelta = new Vector2(100, 100);

                    Image iconImage = skillIcon.GetComponent<Image>();
                    iconImage.sprite = skill.skillData.skillIcon;

                    iconCounts[parentContainer]++;
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetCharacterInfo();
    }

    private void ResetCharacterInfo()
    {
        InfoVisible = false;
        InfoUI.SetActive(false);
        skillInventory.SetActive(false);
    }
}