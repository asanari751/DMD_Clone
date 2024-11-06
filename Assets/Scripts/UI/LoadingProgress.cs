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
    [SerializeField] private float minimumLoadingTime = 2.0f;
    [SerializeField] private Sprite[] loadingSprites;
    [SerializeField] private TextMeshProUGUI loadingText;  // UI Text 컴포넌트 참조
    [SerializeField] private string[] loadingTexts = new string[] { };
    [SerializeField] private float textChangeInterval = 0.3f;
    [SerializeField] private float[] loadingStopPoints;
    [SerializeField] private float stopDuration = 0.25f;

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
}
