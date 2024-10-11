using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SkillSelector : MonoBehaviour
{
    [System.Serializable]
    public class Skilldata
    {
        public string skillName;
        public Sprite skillIcon;
        public int skillLevel;
        public string skillDescription;
    }

    [SerializeField] private GameObject darkBackground;

    public GameObject skillSelectorUI;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillDescriptionTexts;
    public List<Skilldata> availableSkills;

    public Transform skillPanel;
    private List<Image> skillIconImages = new List<Image>();
    private int currentSkillSlot = 0;

    private void Start()
    {
        Experience experienceComponent = FindAnyObjectByType<Experience>();
        if (experienceComponent != null)
        {
            experienceComponent.onLevelUp.AddListener(ShowSkillSelector);
        }

        skillSelectorUI.SetActive(false);
        darkBackground.gameObject.SetActive(false);

        for (int i = 1; i <= 4; i++)
        {
            Transform iconTransform = skillPanel.Find($"Skill Icon {i}");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    skillIconImages.Add(iconImage);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckButtonClick();
        }
    }

    private void ShowSkillSelector()
    {
        darkBackground.gameObject.SetActive(true);
        skillSelectorUI.SetActive(true);
        UpdateSkillUI();
    }

    private void SelectSkill(int index)
    {
        Skilldata selectedSkill = availableSkills[index];


        bool isAlreadySelected = false;
        for (int i = 0; i < currentSkillSlot; i++)
        {
            if (skillIconImages[i].sprite == selectedSkill.skillIcon)
            {
                isAlreadySelected = true;
                break;
            }
        }

        if (!isAlreadySelected && currentSkillSlot < skillIconImages.Count)
        {
            skillIconImages[currentSkillSlot].sprite = selectedSkill.skillIcon;
            currentSkillSlot++;
        }

        skillSelectorUI.SetActive(false);
        darkBackground.gameObject.SetActive(false);
    }

    private void CheckButtonClick()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                if (result.gameObject == skillButtons[i].gameObject)
                {
                    SelectSkill(i);
                    return;
                }
            }
        }
    }

    private void UpdateSkillUI()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < availableSkills.Count)
            {
                skillButtons[i].gameObject.SetActive(true);

                Skilldata skill = availableSkills[i];

                TextMeshProUGUI buttonText = skillButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = skill.skillName;
                }

                Transform skillIconTransform = skillButtons[i].transform.Find("Skill Icon");
                if (skillIconTransform != null)
                {
                    Transform iconSpriteTransform = skillIconTransform.Find("Icon - Sprite");
                    if (iconSpriteTransform != null)
                    {
                        Image skillIconImage = iconSpriteTransform.GetComponent<Image>();
                        if (skillIconImage != null)
                        {
                            skillIconImage.sprite = skill.skillIcon;
                        }
                    }
                }

                if (skillDescriptionTexts.Length > i && skillDescriptionTexts[i] != null)
                {
                    skillDescriptionTexts[i].text = skill.skillDescription;
                }

                int index = i;
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() => SelectSkill(index));
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }
}