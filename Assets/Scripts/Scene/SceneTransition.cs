using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private float transitionDelay;
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration;
    [SerializeField] private SceneSelector sceneSelector;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            sceneSelector.ShowUI();
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionScene(sceneName));
    }

    private IEnumerator TransitionScene(string sceneName)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0, 0, 0, 0);

        yield return fadePanel.DOFade(1, fadeDuration).WaitForCompletion();
        yield return new WaitForSeconds(transitionDelay - fadeDuration);
        
        SceneManager.LoadScene(sceneName);
    }
}