using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;

public class CharacterInfo : MonoBehaviour
{
    public enum GodType
    {
        Bathory = 0,
        Strigoi = 1,
        Vlad = 2,
    }

    public struct PlayerStats
    {
        // 1. Attack
        public float attackDamage;
        public float projectileDamage;
        public float attackSpeed;
        public float criticalChance;

        // 2. Defense
        public float maxHealth;
        public float defense;
        public float moveSpeed;
        public float dashCooldown;

        // 3. Default Attack
        public int penetrateCount;
        public float projectileSpeed;
        public float projectileSize;

        // 4. Utility
        public float lootRange;
    }

    [SerializeField] private GameObject InfoUI;
    [SerializeField] private InputActionReference toggleInfoAction;
    [SerializeField] private bool InfoVisible = false;
    [SerializeField] private PauseController pauseController;
    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private PlayerStats currentStats;

    [Header("Icon")]
    [SerializeField] private Sprite[] GodIcons;
    [SerializeField] private Image IconFiller;
    [SerializeField] private GodType tempSelectedGod;

    [Header("Skill Inventory")]
    [SerializeField] private GameObject skillInventory;
    [SerializeField] private GameObject bathoryContainer;
    [SerializeField] private GameObject strigoiContainer;
    [SerializeField] private GameObject vladContainer;
    [SerializeField] private Button undoButton;

    [SerializeField] private TextMeshProUGUI attackDamageLabel;
    [SerializeField] private TextMeshProUGUI attackDamageValue;

    private GodType currentGod;
    private float attackDamage;


    private void Start()
    {
        InfoUI.SetActive(false);
        skillInventory.SetActive(false);
        UpdateGodIcon(tempSelectedGod);

        undoButton.onClick.AddListener(() => { ToggleInfo(); });
    }

    private void Update()
    {
        UpdateGodIcon(tempSelectedGod);
    }

    private void OnEnable()
    {
        toggleInfoAction.action.performed += OnToggleInfo;  // 이벤트 구독
        playerStateManager.OnStatsUpdated += UpdateCharacterInfo;
    }

    private void OnDisable()
    {
        toggleInfoAction.action.performed -= OnToggleInfo;  // 이벤트 해제
        playerStateManager.OnStatsUpdated -= UpdateCharacterInfo;
    }

    private void UpdateCharacterInfo(PlayerStats newStats)
    {
        currentStats = newStats;
        UpdateInfoUI();
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
            UpdateSkillInventory();
            pauseController.ToggleUIState();
        }
        else
        {
            pauseController.ToggleUIState();
        }
    }

    private void UpdateCharacterInfo(float atk, float projDmg, int penCount)
    {
        attackDamage = atk;
        UpdateInfoUI();
    }

    private void UpdateInfoUI()
    {
        attackDamageLabel.text = $"피해량:";
        attackDamageValue.text = $"{attackDamage}";
    }

    // =======================================

    public void UpdateGodIcon(GodType selectedGod)
    {
        currentGod = selectedGod;
        if (IconFiller != null && GodIcons != null)
        {
            int iconIndex = (int)selectedGod;
            if (iconIndex >= 0 && iconIndex < GodIcons.Length)
            {
                IconFiller.sprite = GodIcons[iconIndex];
            }
        }
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

        // 기존 코드 계속...
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
                    // 현재 컨테이너의 아이콘 개수 확인
                    if (!iconCounts.ContainsKey(parentContainer))
                        iconCounts[parentContainer] = 0;

                    // 스킬 아이콘 생성
                    GameObject skillIcon = new GameObject("SkillIcon", typeof(Image), typeof(RectTransform));
                    skillIcon.transform.SetParent(parentContainer, false);

                    // RectTransform 설정
                    RectTransform rectTransform = skillIcon.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(iconCounts[parentContainer] * 100f, 0); // 100px 간격
                    rectTransform.sizeDelta = new Vector2(100, 100); // 아이콘 크기 설정

                    // 이미지 설정
                    Image iconImage = skillIcon.GetComponent<Image>();
                    iconImage.sprite = skill.skillData.skillIcon;

                    // 아이콘 카운트 증가
                    iconCounts[parentContainer]++;
                }
            }
        }
    }
}