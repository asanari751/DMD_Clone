using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

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

    [SerializeField] private TextMeshProUGUI attackDamageLabel;
    [SerializeField] private TextMeshProUGUI attackDamageValue;

    private GodType currentGod;
    private float attackDamage;


    private void Start()
    {
        InfoUI.SetActive(false);
        UpdateGodIcon(tempSelectedGod);
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

        if (InfoVisible)
        {
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
}