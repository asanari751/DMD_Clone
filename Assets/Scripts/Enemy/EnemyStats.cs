[System.Serializable]
public class EnemyStats
{
    public enum EnemyType
    {
        Melee,
        Arrow
    }

    public EnemyType enemyType;
    public float MoveSpeed;
    public float MaxHealth;
    public float AttackRange;
    public float attackDelay = 1f;
    public float attackCooldown = 2f;
    public float attackDamage = 10f;
    public float RetreatSpeed;
    public float attackAngleRange = 30f;
}