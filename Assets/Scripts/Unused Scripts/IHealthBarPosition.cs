using UnityEngine;

public interface IHealthBarPositionStrategy
{
    void UpdatePosition(GameObject healthBar, Transform targetTransform);
}