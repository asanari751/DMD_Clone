using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactionAction;
    [SerializeField] private InputActionReference cancleAction;

    [Header("Character Data")]
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private bool isInRange = false;

    private void Start()
    {
        SetInteractionUI(false);
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            Debug.Log("접근 감지");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        isInRange = false;
        if (isInRange == false)
        {
            SetInteractionUI(false);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isInRange)
        {
            Debug.Log("상호작용");
            SetInteractionUI(true);
            DisplayCharacterData();
        }
    }

    private void SetInteractionUI(bool enable)
    {
        interactionField.SetActive(enable);
        backgroundOverlay.gameObject.SetActive(enable);
    }

    private void DisplayCharacterData()
    {
        int randomIndex = Random.Range(0, characters.Length);
        CharacterData selectedCharacter = characters[randomIndex];

        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].illustration.gameObject.SetActive(i == randomIndex);
        }

        characterNameText.text = selectedCharacter.characterName;
        dialogueText.text = selectedCharacter.dialogueText;
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        SetInteractionUI(false);
    }
}