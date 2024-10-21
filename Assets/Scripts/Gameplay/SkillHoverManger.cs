using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class SkillHoverManager : MonoBehaviour
{
    [SerializeField] private Image skillInfoImage;
    [SerializeField] private Vector3 skillIconInfoThreshold;
    [SerializeField] private Vector3 inventoryIconInfoThreshold;
    [SerializeField] private TextMeshProUGUI skillOrderText;
    private SkillSelector skillSelector;
    private string currentHoveredSkillName;

    private void Start()
    {
        skillSelector = FindAnyObjectByType<SkillSelector>();
        SetupHoverEffects();
    }

    private void SetupHoverEffects()
    {
        foreach (Transform child in skillSelector.skillPanel)
        {
            if (child.name.StartsWith("Skill Icon"))
            {
                AddHoverEffect(child.gameObject, true);
            }
        }

        foreach (Transform child in skillSelector.skillInventory)
        {
            if (child.name.StartsWith("Inventory Icon"))
            {
                AddHoverEffect(child.gameObject, false);
            }
        }
    }

    private void AddHoverEffect(GameObject iconObject, bool isSkillIcon)
    {
        EventTrigger trigger = iconObject.GetComponent<EventTrigger>() ?? iconObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) =>
        {
            currentHoveredSkillName = iconObject.name;
            ShowHoverUI(isSkillIcon);
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { HideHoverUI(); });
        trigger.triggers.Add(exitEntry);
    }

    private void ShowHoverUI(bool isSkillIcon)
    {
        skillInfoImage.gameObject.SetActive(true);
        skillOrderText.text = currentHoveredSkillName;
        StartCoroutine(UpdateHoverUIPosition(isSkillIcon));
    }

    private void HideHoverUI()
    {
        skillInfoImage.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator UpdateHoverUIPosition(bool isSkillIcon)
    {
        RectTransform canvasRect = skillInfoImage.canvas.GetComponent<RectTransform>();
        float minYPosition = -270f;
        bool isYFixed = false;

        while (skillInfoImage.gameObject.activeSelf)
        {
            Vector3 threshold = isSkillIcon ? skillIconInfoThreshold : inventoryIconInfoThreshold;
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localPoint);
            Vector2 desiredPosition = localPoint + (Vector2)threshold;

            if (desiredPosition.y <= minYPosition)
            {
                desiredPosition.y = minYPosition;
                isYFixed = true;
            }

            if (isYFixed && localPoint.y + threshold.y > minYPosition)
            {
                isYFixed = false;
            }

            if (isYFixed)
            {
                desiredPosition.y = skillInfoImage.rectTransform.anchoredPosition.y;
            }

            skillInfoImage.rectTransform.anchoredPosition = desiredPosition;
            yield return null;
        }
    }
}