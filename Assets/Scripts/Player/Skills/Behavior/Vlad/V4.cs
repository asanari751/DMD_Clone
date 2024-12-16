using UnityEngine;

public class V4 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float damage;
    private float radius;
    private float fearDuration = 3f;
    private AudioManager audioManager;

    private readonly float[] damageValues = { 50f, 75f, 100f };
    private readonly float[] cooldownValues = { 14f, 13f, 12f };
    private readonly float[] radiusValues = { 7f, 8f, 9f };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        int index = skillLevel - 1;
        
        damage = damageValues[index];
        radius = radiusValues[index];
        audioManager = FindAnyObjectByType<AudioManager>();

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.SetParent(playerTransform);
        transform.localPosition = Vector3.zero;

        ApplyDamageAndFear();

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.skillName);
        }

        audioManager.PlaySFX("S14");
        Destroy(gameObject, 1.5f);
    }

    private void ApplyDamageAndFear()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyHealthController enemyHealth = collider.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    Vector2 direction = (collider.transform.position - transform.position).normalized;
                    enemyHealth.TakeDamage(damage, -direction * skillData.knockbackForce);

                    EnemyStatusEffect statusEffect = collider.GetComponent<EnemyStatusEffect>();
                    if (statusEffect != null)
                    {
                        statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Fear, fearDuration);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (radius > 0)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
