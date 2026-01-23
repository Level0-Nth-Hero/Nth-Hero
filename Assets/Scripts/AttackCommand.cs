using System.Collections;
using UnityEngine;

public class AttackCommand : ICommand // 공격 커맨드
{
    private IDamageable _target; // 공격 대상
    private int _damage; // 입힐 데미지
    private Animator _attackerAnim; // 공격자 애니메이터
    public AttackCommand(IDamageable target, int damage, Animator anim) // 생성자
    {
        _target = target; // 공격 대상 설정
        _damage = damage; // 데미지 설정
        _attackerAnim = anim; // 공격자 애니메이터 설정
    }

    public IEnumerator Execute() // 커맨드 실행
    {
        _attackerAnim.SetTrigger("Attack"); // 공격 애니메이션 재생

        yield return new WaitForSeconds(0.5f); // 애니메이션 재생 대기

        if (_target != null) // 대상이 유효한지 확인
        {
            _target.TakeDamage(_damage); // 대상에게 데미지 입히기
            Debug.Log("커맨드 패턴: 공격 적중!"); // 디버그 메시지
        }

        yield return new WaitForSeconds(0.5f); // 추가 대기 시간
    }
}