using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;
using UnityEngine.Events;
using System.Linq;
using TMPro;

public class Experience : MonoBehaviour
{
	[SerializeField] private AudioManager audioManager;
	[SerializeField] private Image expFillImage;
	[SerializeField] private float lerpSpeed = 5f;
	[SerializeField] private int[] skillSelectableLevels;
	[SerializeField] private TMP_Text levelText;


	[SerializedDictionary("Level", "Required Experience")]
	public SerializedDictionary<int, float> expTable;

	public UnityEvent onSkillSelectLevel;

	private float currentExp;
	private int currentLevel = 1;  // 초기 레벨을 1로 설정
	private float expForCurrentLevel;
	private float expForNextLevel;
	public UnityEvent onLevelUp;

	private void Start()
	{
		InitializeExperience();
		UpdateLevelText();  // 초기 레벨 표시
	}

	private void Update()
	{
		UpdateExpBar();
	}

	// 경험치 초기화 및 업데이트
	private void InitializeExperience()
	{
		UpdateLevelExperience();
		UpdateExpBar();
	}

	// 경험치 추가 및 레벨업 처리
	public void AddExperience(float amount)
	{
		currentExp += amount;
		while (currentExp >= expForNextLevel && currentLevel < expTable.Count)
		{
			audioManager.PlayAmbient("A7");
			LevelUp();
		}
		UpdateExpBar();
	}

	private void LevelUp()
	{
		currentLevel++;
		UpdateLevelText();
		UpdateLevelExperience();

		if (IsSkillSelectableLevel(currentLevel))
		{
			onSkillSelectLevel.Invoke();
		}
		else
		{
			onLevelUp.Invoke();
		}
	}

	// 레벨별 경험치 업데이트
	private void UpdateLevelExperience()
	{
		expForCurrentLevel = GetExperienceForLevel(currentLevel - 1);
		expForNextLevel = GetExperienceForLevel(currentLevel);

		// 레벨 간 경험치 차이가 0인 경우 처리
		if (expForNextLevel <= expForCurrentLevel)
		{
			expForNextLevel = expForCurrentLevel + 1; // 임시 해결책
		}
	}

	// 특정 레벨의 경험치 반환
	private float GetExperienceForLevel(int level)
	{
		if (level <= 0) return 0f;
		return expTable.TryGetValue(level, out float exp) ? exp : (level == 1 ? 15f : float.MaxValue);
	}

	// UI 경험치 바 업데이트
	private void UpdateExpBar()
	{
		float targetFillAmount = GetExpPercentage();
		expFillImage.fillAmount = Mathf.Lerp(expFillImage.fillAmount, targetFillAmount, lerpSpeed * Time.deltaTime);
	}

	// 현재 경험치 퍼센티지 계산
	public float GetExpPercentage()
	{
		return (expForNextLevel > expForCurrentLevel) ? (currentExp - expForCurrentLevel) / (expForNextLevel - expForCurrentLevel) : 0f;
	}

	private bool IsSkillSelectableLevel(int level)
	{
		return skillSelectableLevels.Contains(level);
	}

	private void UpdateLevelText()
	{
		if (levelText != null)
		{
			levelText.text = $"Lv.{currentLevel}";
		}
	}

	// 현재 레벨 반환
	public int GetCurrentLevel() => currentLevel;
}
