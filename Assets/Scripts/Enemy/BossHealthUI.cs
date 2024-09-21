using UnityEngine;

public class BossHealthUI : HealthUI
{
    [SerializeField] private RectTransform bossHealthBarContainer;

    private void Awake()
    {
        SetupBossHealthBar();
    }

    private void SetupBossHealthBar()
    {
        if (bossHealthBarContainer != null)
        {
            bossHealthBarContainer.anchorMin = new Vector2(0, 1);
            bossHealthBarContainer.anchorMax = new Vector2(1, 1);
            bossHealthBarContainer.anchoredPosition = Vector2.zero;
            bossHealthBarContainer.sizeDelta = new Vector2(0, 30); // 높이 30으로 설정
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}