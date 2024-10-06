using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInfo : MonoBehaviour
{
    [SerializeField] private GameObject InfoUI;
    [SerializeField] private InputActionReference toggleInfoAction;
    [SerializeField] private bool InfoVisible = false;
    private GameTimerController gameTimerController;

    private void Awake()
    {
        InfoUI.SetActive(false);
        gameTimerController = FindAnyObjectByType<GameTimerController>();
    }

    private void OnEnable()
    {
        toggleInfoAction.action.performed += OnToggleInfo;
    }

    private void OnDisable()
    {
        toggleInfoAction.action.performed -= OnToggleInfo;
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
            // gameTimerController.PauseGame();
        }
        else
        {
            // gameTimerController.ResumeGame();
        }
    }
}