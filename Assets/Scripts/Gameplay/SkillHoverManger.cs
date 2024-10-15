using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class SkillHoverManager : MonoBehaviour
{
    [SerializeField] private Image skillInfoImage;
    [SerializeField] private Vector3 infoThreshold;
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
                AddHoverEffect(child.gameObject);
            }
        }
    }

    private void AddHoverEffect(GameObject iconObject)
    {
        EventTrigger trigger = iconObject.GetComponent<EventTrigger>() ?? iconObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { 
            currentHoveredSkillName = iconObject.name;
            ShowHoverUI(); 
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { HideHoverUI(); });
        trigger.triggers.Add(exitEntry);
    }

    private void ShowHoverUI()
    {
        skillInfoImage.gameObject.SetActive(true);
        skillOrderText.text = currentHoveredSkillName;
        StartCoroutine(UpdateHoverUIPosition());
    }

    private void HideHoverUI()
    {
        skillInfoImage.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator UpdateHoverUIPosition()
    {
        while (skillInfoImage.gameObject.activeSelf)
        {
            skillInfoImage.rectTransform.position = Input.mousePosition + infoThreshold;
            yield return null;
        }
    }
}