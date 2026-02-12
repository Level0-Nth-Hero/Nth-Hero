using UnityEngine;

public class ArcherAttack : IAttackCondition
{
    public bool CanAttack(Transform targetTransform)
    {
        return true;
    }
}