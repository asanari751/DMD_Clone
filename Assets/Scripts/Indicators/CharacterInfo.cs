using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CharacterInfo : MonoBehaviour
{
    [SerializeField] private GameObject InfoUI;
    [SerializeField] private InputActionReference toggleInfoAction;
    [SerializeField] private bool InfoVisible = false;
    [SerializeField] private PauseController pauseController;
    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI projectileDamageText;
    [SerializeField] private TextMeshProUGUI penetrateCountText;
    private float attackDamage;
    private float projectileDamage;
    private int penetrateCount;

    private void Start()
    {
        InfoUI.SetActive(false);
    }

    private void OnEnable()
    {
        toggleInfoAction.action.performed += OnToggleInfo;
        playerStateManager.OnStatsUpdated += UpdateCharacterInfo;
    }

    private void OnDisable()
    {
        toggleInfoAction.action.performed -= OnToggleInfo;
        playerStateManager.OnStatsUpdated -= UpdateCharacterInfo;
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
        projectileDamage = projDmg;
        penetrateCount = penCount;
        UpdateInfoUI();
    }

    private void UpdateInfoUI()
    {
        attackDamageText.text = $"Melee Damage: {attackDamage}";
        projectileDamageText.text = $"Projectile Damage: {projectileDamage}";
        penetrateCountText.text = $"Penetrate Count: {penetrateCount}";
    }
}