using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class SkillHoverManager : MonoBehaviour
{
    [SerializeField] private Image skillInfoImage;
    [SerializeField] private Vector3 skillIconInfoThreshold;
    [SerializeField] private Vector3 inventoryIconInfoThreshold;
    [SerializeField] private TextMeshProUGUI skillOrderText;
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private TextMeshProUGUI skillDamageText;
    [SerializeField] private TextMeshProUGUI skillCooldownText;
    [SerializeField] private TextMeshProUGUI skillEffectText;
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
        int index = int.Parse(currentHoveredSkillName.Split(' ').Last()) - 1;
        SkillData skillData = skillSelector.GetSkillDataByIndex(index);

        if (skillData == null) return;

        skillInfoImage.gameObject.SetActive(true);
        // skillOrderText.text = currentHoveredSkillName;
        skillLevelText.text = $"레벨: {skillData.skillLevel + 1}";
        skillDamageText.text = $"공격력: {skillData.damage}";
        skillCooldownText.text = $"대기시간: {skillData.cooldown}초";
        skillEffectText.text = $"상태이상: {GetKoreanStatusEffect(skillData.statusEffectOnHit)}";

        StartCoroutine(UpdateHoverUIPosition(isSkillIcon));
    }

    private string GetKoreanStatusEffect(SkillData.StatusEffectOnHit effect)
    {
        switch (effect)
        {
            case SkillData.StatusEffectOnHit.None:
                return "없음";
            case SkillData.StatusEffectOnHit.Fear:
                return "공포";
            case SkillData.StatusEffectOnHit.Bleed:
                return "출혈";
            case SkillData.StatusEffectOnHit.Slow:
                return "둔화";
            case SkillData.StatusEffectOnHit.Weakness:
                return "쇠약";
            case SkillData.StatusEffectOnHit.Poison:
                return "중독";
            case SkillData.StatusEffectOnHit.Stun:
                return "기절";
            default:
                return "알 수 없음";
        }
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