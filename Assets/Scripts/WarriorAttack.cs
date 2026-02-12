using UnityEngine;

public class WarriorAttack : IAttackCondition
{
    public bool CanAttack(Transform targetTransform)
    {
        return targetTransform.position.x <= 5f;
    }
}