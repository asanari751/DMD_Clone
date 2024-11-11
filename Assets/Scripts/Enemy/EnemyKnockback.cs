using UnityEngine;
using DG.Tweening;

public class EnemyKnockback : MonoBehaviour
{
    [SerializeField] private float defaultKnockbackDistance = 1f;  // 기본 넉백 거리
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private Color knockbackColor = Color.black;

    private float currentKnockbackDistance;  // 현재 적용될 넉백 거리
    public bool isKnockedBack = false;
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
        currentKnockbackDistance = defaultKnockbackDistance;
    }

    // 스킬별 넉백 거리를 설정하는 메서드
    public void SetKnockbackDistance(float distance)
    {
        currentKnockbackDistance = distance;
    }

    // 넉백 거리를 기본값으로 리셋
    public void ResetKnockbackDistance()
    {
        currentKnockbackDistance = defaultKnockbackDistance;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    public void ApplyKnockback(Vector2 hitDirection, float? customKnockbackDistance = null)
    {
        // 이전 넉백 시퀀스 정리
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }

        // 커스텀 넉백 거리가 지정된 경우 해당 값 사용
        float knockbackDistance = customKnockbackDistance ?? currentKnockbackDistance;

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
            ResetKnockbackDistance();  // 넉백 완료 후 기본값으로 리셋
        });
    }

    public void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void ResetKnockback()
    {
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }
        isKnockedBack = false;
        ResetColor();
        ResetKnockbackDistance();
    }

    private void OnDisable()
    {
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }
        isKnockedBack = false;  // OnDisable에서도 상태 초기화
        ResetKnockbackDistance();
    }
}