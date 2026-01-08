using System.Collections;
using UnityEngine;

public class AttackCommand : ICommand
{
    private IDamageable _target;
    private int _damage;
    private Animator _attackerAnim;

    public AttackCommand(IDamageable target, int damage, Animator anim)
    {
        _target = target;
        _damage = damage;
        _attackerAnim = anim;
    }

    public IEnumerator Execute()
    {
        _attackerAnim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        if (_target != null)
        {
            _target.TakeDamage(_damage);
            Debug.Log("커맨드 패턴: 공격 적중!");
        }

        yield return new WaitForSeconds(0.5f);
    }
}
