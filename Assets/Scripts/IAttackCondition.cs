using UnityEngine;

public interface IAttackCondition
{
    // 공격 조건 인터페이스, 추후에 조건이 추가되어도 직업에 맞는 스크립트에 조건을 추가하는 방식이 괜찮아보여서 이 방식으로 채택 해봤습니다. (김성민)

    bool CanAttack(Transform targetTransform);
}
