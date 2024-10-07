using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;
using UnityEngine.Events;

public class Experience : MonoBehaviour
{
	[SerializeField] private Image expFillImage;
	[SerializeField] private float lerpSpeed = 5f;

	[SerializedDictionary("Level", "Required Experience")]
	public SerializedDictionary<int, float> expTable;

	private float currentExp;
	private int currentLevel = 1;  // 초기 레벨을 1로 설정
	private float expForCurrentLevel;
	private float expForNextLevel;
	public UnityEvent onLevelUp;

	private void Start()
	{
		InitializeExperience();
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
			LevelUp();
		}
		UpdateExpBar();
	}

	// 레벨업 처리
	public void LevelUp()
	{
		currentLevel++;
		UpdateLevelExperience();
		onLevelUp.Invoke();
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

	// 현재 레벨 반환
	public int GetCurrentLevel() => currentLevel;
}
