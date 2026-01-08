using UnityEngine;

public class PlayerMove : MonoBehaviour , IDamageable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;
    
    [Header("에디터 설정")]
    [SerializeField] private EnemyMove enemyScript; 

    public float maxHp { get; private set; } = 100;
    public float currentHp { get; private set; }
    
    // [변경] 이건 0과 1만 오가는 '행동권' 스위치로만 씁니다.
    public int turncount { get; private set; }
    
    // [추가 2] 게임의 진짜 턴 번호 (1턴, 2턴, 3턴...)
    public int globalTurnCount { get; private set; } = 1;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //anim = GetComponent<Animator>();
        
        currentHp = maxHp;
        turncount = 0; // 0이면 내 차례, 1이면 행동 완료
    }

    void Start()
    {
         UIManager.Instance.UpdateHP(currentHp, maxHp, true);
         UIManager.Instance.UpdateHP(100, 100, false); 
         
         // UI에는 진짜 턴 번호를 보냅니다
         UIManager.Instance.UpdateTurnInfo(globalTurnCount, true);
    }

    public void Attack()
    {
        // [중요] 내 턴일 때만 명령을 내려야 함!
        if (turncount < 1)
        {
            turncount++; 
            ICommand attackCmd = new AttackCommand(enemyScript, 10, anim);
            BattleManager.Instance.AddCommand(attackCmd);
            enemyScript.QueueEnemyTurn(); 
            // [추가] 공격 후 턴 넘김 처리를 여기서 할지, 커맨드 끝난 뒤에 할지 정해야 함.
            // 일단은 UI 갱신은 여기서 해도 무방함.
            UIManager.Instance.UpdateTurnInfo(globalTurnCount, false); 
        }
    }

    public void Guard()
    {
        if (turncount < 1)
        {
            turncount++;
            anim.SetTrigger("Guard");
            UIManager.Instance.UpdateTurnInfo(globalTurnCount, false);
        }
    }
    public void Parry()
    {
        if (turncount < 1)
        {
            turncount++;
            anim.SetTrigger("Parry");
            UIManager.Instance.UpdateTurnInfo(globalTurnCount, false);
        }
    }
    public void Dodge()
    {
        if (turncount < 1)
        {
            turncount++;
            anim.SetTrigger("Dodge");
            UIManager.Instance.UpdateTurnInfo(globalTurnCount, false);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        anim.SetTrigger("Damaged");
        if (currentHp < 0) currentHp = 0;
        UIManager.Instance.UpdateHP(currentHp, maxHp, true);
    }

    public void RestTurn(int newGlobalTurn)
    {
        turncount = 0; // 행동권 복구
        globalTurnCount = newGlobalTurn;
    }
}