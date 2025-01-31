using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed;
    private float damage;
    private float lifetime = 5f;
    private ProjectilePool pool;
    private int penetrateCount;
    private int currentPenetrateCount = 0;
    private Animator animator;

    private TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
            damageable.TakeDamage(damage, -knockbackDirection);

            currentPenetrateCount++;
            if (currentPenetrateCount >= penetrateCount)
            {
                ReturnToPool();
            }
        }
    }

    public void Initialize(float projectileSpeed, float projectileDamage, int projectilePenetrateCount, ProjectilePool projectilePool)
    {
        speed = projectileSpeed;
        damage = projectileDamage;
        penetrateCount = projectilePenetrateCount;
        currentPenetrateCount = 0;
        pool = projectilePool;
        CancelInvoke();
        Invoke("ReturnToPool", lifetime);

        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.Clear();
        }

        // 애니메이션 실행
        if (animator != null)
        {
            animator.Play("Bat"); // "ProjectileAnimation"을 실제 애니메이션 이름으로 변경하세요
        }
    }

    private void ReturnToPool()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        if (pool != null)
        {
            CancelInvoke();
            pool.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
