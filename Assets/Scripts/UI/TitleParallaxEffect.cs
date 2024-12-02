using DG.Tweening;
using UnityEngine;

public class TitleParallaxEffect : MonoBehaviour 
{
    [Header("Foreground Layer")]
    [SerializeField] private RectTransform foregroundImage;
    [SerializeField] [Range(0f, 100f)] private float foreIntensityX = 40f;
    [SerializeField] [Range(0f, 100f)] private float foreIntensityY = 40f;

    [Header("Middleground Layer")]
    [SerializeField] private RectTransform middlegroundImage;
    [SerializeField] [Range(0f, 100f)] private float middleIntensityX = 20f;
    [SerializeField] [Range(0f, 100f)] private float middleIntensityY = 20f;

    [Header("Background Layer")]
    [SerializeField] private RectTransform backgroundImage;
    [SerializeField] [Range(0f, 100f)] private float backIntensityX = 10f;
    [SerializeField] [Range(0f, 100f)] private float backIntensityY = 10f;

    [Header("Movement Settings")]
    [SerializeField] private float movementDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] [Range(0f, 0.5f)] private float deadZone = 0.1f;

    private Vector2 foregroundInitialPos;
    private Vector2 middlegroundInitialPos;
    private Vector2 backgroundInitialPos;

    private void Start()
    {
        foregroundInitialPos = foregroundImage.anchoredPosition;
        middlegroundInitialPos = middlegroundImage.anchoredPosition;
        backgroundInitialPos = backgroundImage.anchoredPosition;
    }

    private void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 normalizedPos = new Vector2(
            (mousePos.x / Screen.width) - 0.5f,
            (mousePos.y / Screen.height) - 0.5f
        );

        // 데드존 적용
        normalizedPos = ApplyDeadZone(normalizedPos);

        MoveLayer(foregroundImage, foregroundInitialPos, normalizedPos, foreIntensityX, foreIntensityY);
        MoveLayer(middlegroundImage, middlegroundInitialPos, normalizedPos, middleIntensityX, middleIntensityY);
        MoveLayer(backgroundImage, backgroundInitialPos, normalizedPos, backIntensityX, backIntensityY);
    }

    private Vector2 ApplyDeadZone(Vector2 input)
    {
        float x = Mathf.Abs(input.x) < deadZone ? 0 : input.x;
        float y = Mathf.Abs(input.y) < deadZone ? 0 : input.y;
        return new Vector2(x, y);
    }

    private void MoveLayer(RectTransform layer, Vector2 initialPos, Vector2 normalizedPos, float intensityX, float intensityY)
    {
        Vector2 targetPos = initialPos + new Vector2(
            normalizedPos.x * intensityX,
            normalizedPos.y * intensityY
        );

        layer.DOAnchorPos(targetPos, movementDuration)
            .SetEase(moveEase)
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        // DOTween 정리
        foregroundImage.DOKill();
        middlegroundImage.DOKill();
        backgroundImage.DOKill();
    }
}
