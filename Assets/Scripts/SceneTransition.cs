using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float transitionDelay;
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(TransitionScene());
        }
    }

    private IEnumerator TransitionScene()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0, 0, 0, 0);

        yield return fadePanel.DOFade(1, fadeDuration).WaitForCompletion();
        yield return new WaitForSeconds(transitionDelay - fadeDuration);
        
        SceneManager.LoadScene(sceneToLoad);
    }
}