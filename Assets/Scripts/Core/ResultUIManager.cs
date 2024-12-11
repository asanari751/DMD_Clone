using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalDamageText;
    [SerializeField] private TextMeshProUGUI totalKillText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI elapsedTimeText;

    [SerializeField] private TextMeshProUGUI[] skillTexts; // 스킬

    public void ShowResults()
    {
        foreach (var text in skillTexts)
        {
            text.text = "";
            text.gameObject.SetActive(false);
        }

        int killCount = ResultManager.Instance.GetKillCount();
        float totalDamage = ResultManager.Instance.GetTotalDamage();
        int playerLevel = FindAnyObjectByType<Experience>().GetCurrentLevel();
        float survivalTime = FindAnyObjectByType<GameTimerController>().GetElapsedTime();

        // UI 텍스트 업데이트 추가
        totalDamageText.text = $"{totalDamage:N0}";
        totalKillText.text = $"{killCount}";
        currentLevelText.text = $"{playerLevel}";

        // 시간을 분:초 형식으로 변환
        int minutes = Mathf.FloorToInt(survivalTime / 60);
        int seconds = Mathf.FloorToInt(survivalTime % 60);
        elapsedTimeText.text = $"{minutes:00}:{seconds:00}";

        var playerSkills = FindAnyObjectByType<PlayerSkills>();
        var sortedSkills = playerSkills.activeSkills.OrderByDescending(s => s.skillLevel).ToList();

        for (int i = 0; i < sortedSkills.Count && i < skillTexts.Length; i++)
        {
            skillTexts[i].text = $"{sortedSkills[i].skillData.skillName} {sortedSkills[i].skillLevel}";
            skillTexts[i].gameObject.SetActive(true);
        }
    }
}