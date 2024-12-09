using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingProgress : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private Image loadingImage;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float imageChangeInterval = 0.5f;
    [SerializeField] private float minimumLoadingTime;
    [SerializeField] private Sprite[] loadingSprites;
    [SerializeField] private TextMeshProUGUI loadingText;  // UI Text 컴포넌트 참조
    [SerializeField] private string[] loadingTexts = new string[] { };
    [SerializeField] private float textChangeInterval = 0.3f;
    [SerializeField] private float[] loadingStopPoints;
    [SerializeField] private float stopDuration = 0.25f;
    [SerializeField] private RectTransform runnerImage;
    [SerializeField] private Animator runnerAnimator;
    [SerializeField] private RectTransform progressBarRect;

    private float targetProgress;
    private float currentProgress;
    private int currentSpriteIndex;
    private float loadingStartTime;
    private float? stopStartTime;
    private int currentStopPointIndex;

    private void Start()
    {
        loadingStartTime = Time.time;
        StartCoroutine(CycleLoadingImages());
        StartCoroutine(CycleLoadingTexts());

        if (runnerAnimator != null)
        {
            runnerAnimator.Play("0_Run");
        }
    }

    private void Update()
    {
        float elapsedTime = Time.time - loadingStartTime;
        float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadingTime);
        float finalProgress = Mathf.Min(targetProgress, timeProgress);

        if (currentStopPointIndex < loadingStopPoints.Length &&
            finalProgress >= loadingStopPoints[currentStopPointIndex])
        {
            if (!stopStartTime.HasValue)
            {
                stopStartTime = Time.time;
            }
            else if (Time.time - stopStartTime.Value >= stopDuration)
            {
                stopStartTime = null;
                currentStopPointIndex++;
            }
            else
            {
                return;
            }
        }

        if (currentProgress != finalProgress)
        {
            currentProgress = Mathf.Lerp(currentProgress, finalProgress, Time.deltaTime * smoothSpeed);
            progressBar.fillAmount = currentProgress;
        }

        UpdateRunnerPosition();
    }

    private IEnumerator CycleLoadingImages()
    {
        while (true)
        {
            loadingImage.sprite = loadingSprites[currentSpriteIndex];
            currentSpriteIndex = (currentSpriteIndex + 1) % loadingSprites.Length;
            yield return new WaitForSeconds(imageChangeInterval);
        }
    }

    private IEnumerator CycleLoadingTexts()
    {
        int currentTextIndex = 0;
        while (true)
        {
            loadingText.text = loadingTexts[currentTextIndex];
            currentTextIndex = (currentTextIndex + 1) % loadingTexts.Length;
            yield return new WaitForSeconds(textChangeInterval);
        }
    }

    public void SetProgress(float progress)
    {
        targetProgress = progress;
    }

    public bool IsLoadingComplete()
    {
        float elapsedTime = Time.time - loadingStartTime;
        return elapsedTime >= minimumLoadingTime && Mathf.Approximately(currentProgress, 1f);
    }

    private void UpdateRunnerPosition()
    {
        if (runnerImage == null || progressBarRect == null) return;

        // 프로그레스바의 전체 너비
        float barWidth = progressBarRect.rect.width;

        // 현재 진행도에 따른 x 위치 계산
        float xPosition = (barWidth * currentProgress) - (barWidth * 0.5f);

        // 캐릭터의 현재 위치 업데이트
        Vector3 newPosition = runnerImage.localPosition;
        newPosition.x = xPosition;
        runnerImage.localPosition = newPosition;
    }
}
