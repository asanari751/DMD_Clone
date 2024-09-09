using UnityEngine;
using UnityEngine.UI;

public class Experience : MonoBehaviour
{
	[SerializeField] private Image expFillImage;

	private float currentExp;
	private float maxExp = 100f;
	private float lerpSpeed = 5f;
	private int currentLevel = 1;

	private void Update()
	{
		if (expFillImage.fillAmount != currentExp / maxExp)
		{
			expFillImage.fillAmount = Mathf.Lerp(expFillImage.fillAmount, currentExp / maxExp, lerpSpeed * Time.deltaTime);
		}
	}

	public void AddExperience(float amount)
	{
		currentExp += amount;
		if (currentExp >= maxExp)
		{
			LevelUp();
		}
	}

	private void LevelUp()
	{
		currentLevel++;
		Debug.Log("레벨업! 현재 레벨: " + currentLevel);
		
		currentExp -= maxExp;
		// 여기에 레벨업 시 최대 경험치 증가 로직을 추가할 수 있습니다.
		// 예: maxExp *= 1.2f; // 최대 경험치를 20% 증가

	}

	public float GetExpPercentage()
	{
		return currentExp / maxExp;
	}

	public int GetCurrentLevel()
	{
		return currentLevel;
	}

	public void AddDebugExperience()
	{
		AddExperience(500f);
		Debug.Log("디버그: 500 경험치 추가됨");
	}
}
