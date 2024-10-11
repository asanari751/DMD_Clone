using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SkillSelectorProduction : MonoBehaviour
{
    [SerializeField] private SkillSelector skillSelector;
    [SerializeField] private float productionTime = 1f;
    [SerializeField] private float maximizeButton = 1.1f;
    [SerializeField] private float buttonEdge = 2f;

    private Button[] skillButtons;
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private bool isProducing = false;

    private void Start()
    {
        skillButtons = skillSelector.skillButtons;
        originalPositions = new Vector3[skillButtons.Length];
        originalScales = new Vector3[skillButtons.Length];

        for (int i = 0; i < skillButtons.Length; i++)
        {
            originalPositions[i] = skillButtons[i].transform.position;
            originalScales[i] = skillButtons[i].transform.localScale;
            AddHoverListeners(skillButtons[i]);
        }

        skillSelector.skillSelectorUI.SetActive(false);
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
        skillSelector.skillSelectorUI.SetActive(true);

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.position = originalPositions[i] + Vector3.up * 1000f;
            MoveButtonDown(skillButtons[i], i);
            yield return new WaitForSeconds(productionTime / skillButtons.Length);
        }

        yield return new WaitForSeconds(productionTime);
        isProducing = false;
    }

    private void MoveButtonDown(Button button, int index)
    {
        button.transform.DOMove(originalPositions[index], productionTime)
            .SetEase(Ease.OutBack);
    }
    private void AddHoverListeners(Button button)
    {
        button.onClick.AddListener(() => { if (!isProducing) button.onClick.Invoke(); });

        button.transition = Selectable.Transition.None;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)] * maximizeButton, 0.2f);
                AddWhiteOutline(button.gameObject, buttonEdge);
            }
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)], 0.2f);
                RemoveWhiteOutline(button.gameObject);
            }
        });
        trigger.triggers.Add(exitEntry);
    }

    private void AddWhiteOutline(GameObject obj, float edgeSize)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(edgeSize, edgeSize);
        outline.enabled = true;
    }

    private void RemoveWhiteOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
