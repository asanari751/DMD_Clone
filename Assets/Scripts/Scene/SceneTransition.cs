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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            sceneSelector.Cancle();
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneTransitionManager.Instance.LoadSceneWithTransition(sceneName);
    }
}