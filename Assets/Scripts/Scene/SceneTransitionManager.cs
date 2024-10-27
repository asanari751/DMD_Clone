using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            fadeImage.canvas.transform.SetParent(null);
            DontDestroyOnLoad(fadeImage.canvas.gameObject);
            DontDestroyOnLoad(gameObject);
            fadeImage.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;

        // 페이드 아웃 (투명 -> 검정)
        fadeImage.DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
                // 페이드 인 (검정 -> 투명)
                fadeImage.DOFade(0f, fadeDuration)
                    .OnComplete(() =>
                    {
                        fadeImage.gameObject.SetActive(false);
                    });
            });
    }
}