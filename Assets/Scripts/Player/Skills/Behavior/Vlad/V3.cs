using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V3 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private CircleCollider2D circleCollider;
    private List<GameObject> hitEnemies = new List<GameObject>();
    private Vector3 moveDirection;
    private bool canMove = true;
    private AudioManager audioManager;

    public void Initialize(SkillData data, int level)
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        skillData = data;
        skillLevel = level;
        circleCollider = GetComponent<CircleCollider2D>();
        transform.position = GameObject.FindGameObjectWithTag("Player").transform.position;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.skillName);
        }

        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            Vector3 baseDirection = (nearestEnemy.transform.position - transform.position).normalized;

            int stormCount = skillLevel == 1 ? 1 : 3;
            float angleStep = stormCount == 2 ? 60f : 45f;

            // 메인 폭풍 (중앙)
            moveDirection = baseDirection;
            audioManager.PlaySFX("S13");
            StartCoroutine(LifetimeRoutine());
            StartCoroutine(DamageRoutine());

            // 서브 폭풍들 (양 옆)
            if (stormCount > 1)
            {
                for (int i = 1; i <= 2; i++)
                {
                    float angle = angleStep * (i % 2 == 1 ? 1 : -1); // 양쪽으로 퍼지도록
                    Vector3 rotatedDirection = Quaternion.Euler(0, 0, angle) * baseDirection;

                    GameObject storm = Instantiate(gameObject, transform.position, Quaternion.identity);
                    V3 stormScript = storm.GetComponent<V3>();
                    stormScript.skillData = this.skillData;
                    stormScript.skillLevel = this.skillLevel;
                    stormScript.moveDirection = rotatedDirection;
                    stormScript.StartCoroutine(stormScript.LifetimeRoutine());
                    stormScript.StartCoroutine(stormScript.DamageRoutine());

                    Animator stormAnimator = storm.GetComponent<Animator>();
                    if (stormAnimator != null)
                    {
                        stormAnimator.Play(skillData.skillName);
                    }
                }
            }
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        return nearest;
    }

    private void Update()
    {
        if (canMove)
        {
            float moveSpeed = 6f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    private IEnumerator DamageRoutine()
    {
        while (true)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, skillData.radius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    EnemyHealthController enemy = collider.GetComponent<EnemyHealthController>();
                    if (enemy != null)
                    {
                        Vector2 knockbackDirection = (collider.transform.position - transform.position).normalized;
                        enemy.TakeDamage(skillData.damage, knockbackDirection);
                        enemy.GetComponent<Rigidbody2D>()?.AddForce(knockbackDirection * skillData.knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
            yield return new WaitForSeconds(skillData.attackInterval);
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(2.5f);
        canMove = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
