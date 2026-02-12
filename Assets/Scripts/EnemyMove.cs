using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour, IDamageable 
{
    public PlayerMove playerScript;
    private Animator anim; // 적 애니메이터, 필드 변수는 아래서 Awake로 가져옴

    //IDamageable 인터페이스 구현으로 충돌되는 EnemyMove의 일부분 수정 및 추가 
    //플레이어와 적을 똑같은 '타격 대상'으로 취급을 위한 약속 IDamageable
    //이 약속에 "현재 체력과 쉴드를 알려달라"는 내용이 추가
    //적은 실드를 안 쓰지만, 에러를 막고 시스템이 똑같이 작동하게 하려고 0으로 설정 이후 적에게도 실드가 생기면 수정

    public float currentHp { get; private set; } // 현재 체력
    public float currentShield { get; private set; }

    public float CurrentHp => currentHp; // 외부 ui등에서 적의 피 몇인지 답해주기 위한 것
    public float CurrentShield => currentShield;  // 외부 ui등에서 실드가 몇인지 답해주기 위한 것

    // [추가 1] 대사 목록을 저장할 배열
    [Header("대사 설정")]
    public string[] attackDialogues; // 공격할 때 칠 대사들
    
    public float maxHp = 100; // 최대 체력
    
    public int attackDamage = 10; // 적의 공격 데미지

    public Transform TargetTransform => transform;

    void Awake()
    {
        anim = GetComponent<Animator>(); 
        currentHp = maxHp; // 현재 체력 초기화
        currentShield = 0; //턴 시작 시 현재 쉴드 초기화
    }

    void Start()
    {
        string dialogue = GetRandomDialogue(); // 랜덤 대사 선택
        UIManager.Instance.ShowDialogue(dialogue, 2.0f); // 전투 시작 대사 출력
    }
    public void QueueEnemyTurn() // 적 턴 커맨드 큐에 등록 함수
    {
        // 1. 대사 커맨드 등록
        string dialogue = GetRandomDialogue(); // 랜덤 대사 선택
        ICommand dialogueCmd = new DialogueCommand(dialogue, 2.0f); // 대사 커맨드 생성
        BattleManager.Instance.AddCommand(dialogueCmd); // 커맨드 큐에 등록

        // 2. 공격 커맨드 등록 (타겟은 플레이어)
        List<IAttackCondition> conditions = new List<IAttackCondition>();
        // 아무 조건도 안 넣음 = 무조건 공격 가능

        ICommand attackCmd = new AttackCommand(playerScript, playerScript.TargetTransform, attackDamage, anim, conditions);
        BattleManager.Instance.AddCommand(attackCmd);

        /*
        ICommand attackCmd = new AttackCommand(playerScript, playerScript.transform, attackDamage, anim);
        BattleManager.Instance.AddCommand(attackCmd);
        */

        // 3. 턴 넘김 커맨드 등록 (적이 다 때리고 나면 턴 변경)
        ICommand turnChangeCmd = new TurnChangeCommand(BattleState.PlayerTurn); 
        BattleManager.Instance.AddCommand(turnChangeCmd);
    }

    string GetRandomDialogue()
    {
        if (attackDialogues != null && attackDialogues.Length > 0)
            return attackDialogues[Random.Range(0, attackDialogues.Length)];
        return "...";
    }

    // [인터페이스 구현] IDamageable 때문에 반드시 있어야 함
    public void TakeDamage(float damage) //체력 차감 전, 보유한 실드가 있다면 실드 차감을 위해  currentHp -= damage;을 수정
    {
        float remainingDamage = damage; // 계산을 위해 받은 데미지를 별도 저장

        if (currentShield > 0) //실드가 있다면 데미지를 우선적으로 소모
        {
            if (currentShield >= remainingDamage) // 실드로 막을 수 있는 데미지일 경우 (데미지보다 많거나 같을 때)
            {
                currentShield -= remainingDamage;
                remainingDamage = 0; // 데미지 완전 방어
            }
            else // 실드로 막기 부족할 때
            {
                remainingDamage -= currentShield; // 실드만큼 데미지 감소
                currentShield = 0;// 실드 소진
            }
        }
        currentHp -= remainingDamage; //// 실드를 뚫고 남은 데미지만큼만 현재 체력에서 차감
        if (currentHp < 0) currentHp = 0;
        anim.SetTrigger("Damaged");
        UIManager.Instance.UpdateHP(currentHp, maxHp, false); // false는 적
    }
}