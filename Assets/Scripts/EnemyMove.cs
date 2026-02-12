using UnityEngine;
using System.Collections;

public class EnemyMove : MonoBehaviour, IDamageable 
{
    public PlayerMove playerScript; 
    Animator anim; // 적 애니메이터

    // [추가 1] 대사 목록을 저장할 배열
    [Header("대사 설정")]
    public string[] attackDialogues; // 공격할 때 칠 대사들
    
    public float maxHp = 100; // 최대 체력
    public float currentHp; // 현재 체력
    public int attackDamage = 10; // 적의 공격 데미지

    void Awake()
    {
        anim = GetComponent<Animator>(); 
        currentHp = maxHp; // 현재 체력 초기화
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
        ICommand attackCmd = new AttackCommand(playerScript, attackDamage, anim);
        BattleManager.Instance.AddCommand(attackCmd);

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
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        anim.SetTrigger("Damaged");
        if (currentHp < 0) currentHp = 0;
        UIManager.Instance.UpdateHP(currentHp, maxHp, false); // false는 적

        if (currentHp <= 0)
        {
            UI_Reward.Instance.Show(); // UI_Reward 호출
        }
    }
}