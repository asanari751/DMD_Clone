using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingProgress : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private Image loadingImage;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float imageChangeInterval = 0.5f;
    [SerializeField] private float minimumLoadingTime = 2.0f;
    [SerializeField] private Sprite[] loadingSprites;
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
