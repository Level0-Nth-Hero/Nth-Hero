using UnityEngine;

public interface IDamageable // 데미지를 받을 수 있는 객체 인터페이스
{
    float CurrentHp { get; }//현재 HP 외부 확인용
    float CurrentShield { get; }//현재 방어 외부 확인용
    void TakeDamage(float damage); // 데미지 받기 메서드
}
