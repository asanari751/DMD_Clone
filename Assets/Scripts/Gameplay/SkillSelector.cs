using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [System.Serializable]
    public class SkillLevelColor
    {
        public Color color;
    }

    [SerializeField] private GameObject darkBackground;
    [SerializeField] private float productionTime = 1f;
    [SerializeField] private float maximizeButton = 1.1f;
    [SerializeField] private float buttonEdge = 10f;
    [SerializeField] private SkillLevelColor[] skillLevelColors = new SkillLevelColor[3];

    public GameObject skillSelectorUI;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillDescriptionTexts;
    public List<Skilldata> availableSkills;

    public Transform skillPanel;
    public Transform skillInventory;
    public List<Image> skillIconImages = new List<Image>();
    public List<Image> skillInventoryIcons = new List<Image>();
    public int currentSkillSlot = 0;

    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private bool isProducing = false;
    private PauseController pauseController;

    private void Start()
    {
        Debug.Log("Test");
        Experience experienceComponent = FindAnyObjectByType<Experience>();
        pauseController = FindAnyObjectByType<PauseController>();
        if (experienceComponent != null)
        {
            experienceComponent.onLevelUp.AddListener(ShowSkillSelector);
        }

        skillSelectorUI.SetActive(false);
        darkBackground.gameObject.SetActive(false);

        for (int j = 1; j <= 4; j++)
        {
            Transform iconTransform = skillPanel.Find($"Skill Icon {j}");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    skillIconImages.Add(iconImage);
                    skillInventoryIcons.Add(iconImage);
                }
            }

            originalPositions = new Vector3[skillButtons.Length];
            originalScales = new Vector3[skillButtons.Length];

            for (int i = 0; i < skillButtons.Length; i++)
            {
                originalPositions[i] = skillButtons[i].transform.position;
                originalScales[i] = skillButtons[i].transform.localScale;
                AddHoverListeners(skillButtons[i]);
            }
        }
    }

    private void ShowSkillSelector()
    {
        ResetSkillButtons();
        darkBackground.gameObject.SetActive(true);
        skillSelectorUI.SetActive(true);
        pauseController.TempPause();

        ShuffleSkills();

        UpdateSkillUI();
        StartProduction();
    }

    private void ShuffleSkills() // 순서 섞기
    {
        System.Random rng = new System.Random();
        int n = availableSkills.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Skilldata value = availableSkills[k];
            availableSkills[k] = availableSkills[n];
            availableSkills[n] = value;
        }
    }

    private void ResetSkillButtons()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].transform.localScale = originalScales[i];
            RemoveColor(skillButtons[i].gameObject);
        }
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

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].gameObject.SetActive(false);
            skillButtons[i].transform.position = originalPositions[i] + Vector3.up * 1200f;
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            skillButtons[i].gameObject.SetActive(true);
            MoveButtonDown(skillButtons[i], i);
            yield return new WaitForSecondsRealtime(productionTime / skillButtons.Length);
        }

        yield return new WaitForSecondsRealtime(productionTime);
        isProducing = false;
    }

    // private void MoveButtonDown(Button button, int index)
    // {
    //     button.transform.DOMove(originalPositions[index], productionTime)
    //         .SetEase(Ease.OutBack).SetUpdate(true);
    // }

    private void MoveButtonDown(Button button, int index)
    {
        button.transform.DOMove(originalPositions[index], productionTime)
            .SetEase(Ease.OutQuart).SetUpdate(true);
    }

    private void SelectSkill(int index)
    {
        Skilldata selectedSkill = availableSkills[index];
        PlayerSkills.Instance.AddOrUpgradeSkill(selectedSkill.skillName);
        UpdateSkillUI();

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

        pauseController.TempResume();
        skillSelectorUI.SetActive(false);
        darkBackground.gameObject.SetActive(false);
    }

    private void UpdateSkillUI()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < availableSkills.Count)
            {
                skillButtons[i].gameObject.SetActive(true);

                Skilldata skill = availableSkills[i];
                int skillLevel = PlayerSkills.Instance.GetSkillLevel(skill.skillName);

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

                    Transform iconBackgroundTransform = skillIconTransform.Find("Icon Background");
                    if (iconBackgroundTransform != null)
                    {
                        Image iconBackgroundImage = iconBackgroundTransform.GetComponent<Image>();
                        if (iconBackgroundImage != null)
                        {
                            int colorIndex = Mathf.Clamp(skillLevel - 1, 0, skillLevelColors.Length - 1);
                            iconBackgroundImage.color = skillLevelColors[colorIndex].color;
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

    private void AddHoverListeners(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { if (!isProducing) SelectSkill(System.Array.IndexOf(skillButtons, button)); });

        button.transition = Selectable.Transition.None;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)] * maximizeButton, 0.2f).SetUpdate(true);
                AddColor(button.gameObject, buttonEdge);
            }
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) =>
        {
            if (!isProducing)
            {
                button.transform.DOScale(originalScales[System.Array.IndexOf(skillButtons, button)], 0.2f).SetUpdate(true);
                RemoveColor(button.gameObject);
            }
        });
        trigger.triggers.Add(exitEntry);
    }

    private void AddColor(GameObject obj, float edgeSize)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(edgeSize, -edgeSize);
        outline.useGraphicAlpha = false;
        outline.enabled = true;
    }

    private void RemoveColor(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}