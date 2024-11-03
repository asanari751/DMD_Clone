using UnityEngine;
using DG.Tweening;

public class EnemyKnockback : MonoBehaviour
{
    [SerializeField] private float knockbackDistance = 1f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private Color knockbackColor = Color.black;

    private bool isKnockedBack = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Sequence knockbackSequence;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void ApplyKnockback(Vector2 hitDirection)
    {
        // 이전 넉백 시퀀스 정리
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }

        isKnockedBack = true;
        Vector2 knockbackTarget = (Vector2)transform.position + (-hitDirection.normalized * knockbackDistance);

        knockbackSequence = DOTween.Sequence();
        knockbackSequence.Append(spriteRenderer.DOColor(knockbackColor, knockbackDuration * 0.5f));
        knockbackSequence.Join(transform.DOMove(knockbackTarget, knockbackDuration).SetEase(Ease.OutQuad));
        knockbackSequence.Append(spriteRenderer.DOColor(originalColor, knockbackDuration * 0.5f));
        knockbackSequence.OnComplete(() =>
        {
            isKnockedBack = false;
            knockbackSequence = null;
        });
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    public void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void OnDisable()
    {
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }
    }
}