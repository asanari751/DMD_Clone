
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration;

    private void Start()
    {
        fadePanel.gameObject.SetActive(true);
        StartFadeIn();
    }

    private void StartFadeIn()
    {
        fadePanel.color = Color.black;
        fadePanel.DOFade(0, fadeDuration);
    }
}
