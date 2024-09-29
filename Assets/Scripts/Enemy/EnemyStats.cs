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
    public float RetreatSpeed;
}