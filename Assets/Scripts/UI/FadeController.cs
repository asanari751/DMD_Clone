using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            fadeImage.canvas.transform.SetParent(null);
            DontDestroyOnLoad(fadeImage.canvas.gameObject);
            fadeImage.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fadeImage.gameObject.SetActive(false);
    }

    public void FadeOut(TweenCallback onComplete)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;
        fadeImage.DOFade(1f, fadeDuration).OnComplete(onComplete);
    }

    public void FadeIn(TweenCallback onComplete)
    {
        fadeImage.DOFade(0f, fadeDuration)
            .OnComplete(() => {
                fadeImage.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }
}
