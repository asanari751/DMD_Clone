using System.Collections;
using UnityEngine;

public class B3 : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float range;
    private float duration;
    private int skillLevel;
    private SkillData.StatusEffectOnHit effectOnHit;
    private Vector2 direction;
    private float expandSpeed = 100f; // 콜라이더 확장 속도
    private BoxCollider2D boxCollider;
    private AudioManager audioManager;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null)
        {
            Destroy(gameObject);
            return;
        }

        this.damage = skillData.damage * skillLevel;
        this.knockbackForce = skillData.knockbackForce;
        this.range = skillData.range;
        this.skillLevel = skillLevel;
        this.effectOnHit = skillData.statusEffectOnHit;
        this.duration = skillData.duration;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = playerTransform.position;

        direction = ((Vector2)nearestEnemy.transform.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // BoxCollider2D 설정
        boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(0.5f, 0.5f);
        boxCollider.isTrigger = true;

        audioManager.PlaySFX("S10");
        StartExpandingCollider();
    }

    private void StartExpandingCollider()
    {
        StartCoroutine(ExpandCollider());
    }

    private IEnumerator ExpandCollider()
    {
        float currentWidth = boxCollider.size.x;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Cubic
            float easedProgress = 1 - Mathf.Pow(1 - t, 3);

            float newWidth = Mathf.Lerp(currentWidth, range, easedProgress);
            boxCollider.size = new Vector2(newWidth, boxCollider.size.y);
            boxCollider.offset = new Vector2(newWidth / 2, 0);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = collision.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, direction * -knockbackForce);

                if (effectOnHit != SkillData.StatusEffectOnHit.None)
                {
                    EnemyStatusEffect statusEffect = enemyHealth.GetComponent<EnemyStatusEffect>();
                    if (statusEffect != null)
                    {
                        statusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(effectOnHit), 2f);
                    }
                }
            }
        }
    }

    private EnemyStatusEffect.StatusEffectType ConvertToEnemyStatusEffectType(SkillData.StatusEffectOnHit effect)
    {
        switch (effect)
        {
            case SkillData.StatusEffectOnHit.Slow:
                return EnemyStatusEffect.StatusEffectType.Slow;
            case SkillData.StatusEffectOnHit.Bleed:
                return EnemyStatusEffect.StatusEffectType.Bleed;
            case SkillData.StatusEffectOnHit.Poison:
                return EnemyStatusEffect.StatusEffectType.Poison;
            default:
                return EnemyStatusEffect.StatusEffectType.None;
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.red;
            Vector3 center = transform.position + transform.right * boxCollider.offset.x;
            Vector3 size = new Vector3(boxCollider.size.x, boxCollider.size.y, 1f);
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}
