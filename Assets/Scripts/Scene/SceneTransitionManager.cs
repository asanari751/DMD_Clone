using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private const string loadingScene = "0_Loading";
    private string sceneToLoad;
    private AsyncOperation loadOperation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        sceneToLoad = sceneName;
        FadeController.Instance.FadeOut(() => StartCoroutine(LoadingProcess()));
    }

    private IEnumerator LoadingProcess()
    {
        SceneManager.LoadScene(loadingScene);

        yield return new WaitForSeconds(0.1f);

        LoadingProgress loadingProgress = FindAnyObjectByType<LoadingProgress>();
        if (loadingProgress == null)
        {
            Debug.LogError("LoadingProgress");
            yield break;
        }

        loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingProgress.SetProgress(progress);

            if (loadOperation.progress >= 0.9f && loadingProgress.IsLoadingComplete())
            {
                FadeController.Instance.FadeOut(() =>
                {
                    loadOperation.allowSceneActivation = true;
                });

                yield return new WaitUntil(() => loadOperation.isDone);
                FadeController.Instance.FadeIn(null);
                break;
            }

            yield return null;
        }
    }
}