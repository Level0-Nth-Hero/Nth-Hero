using UnityEngine;

public enum PlayerJob // 플레이어의 직업 (TargetingSystem 추가)
{ Warrior, Archer }

public class PlayerMove : MonoBehaviour , IDamageable // 플레이어 이동 및 상태 관리 클래스
{
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    [SerializeField] private Animator anim; // 애니메이터

    [Header("에너지")] // 에너지 설정
    public int maxEnergy = 3; // 최대 에너지
    public int currentEnergy; // 현재 에너지
    
    // [통합] 쉴드 설정 (Shield Branch)
    public float maxShield = 15f; // 쉴드 최대치 제한
    public float currentShield { get; private set; } // 현재 쉴드량

    public float maxHp { get; private set; } = 100; // 최대 체력
    public float currentHp { get; private set; } // 현재 체력

    // ---------------------------------------------------------
    // [TargetingSystem Branch] 직업 및 타겟 정보
    // ---------------------------------------------------------
    public PlayerJob playerJob;
    public Transform TargetTransform => this.transform;

    // ---------------------------------------------------------
    // [Develop Branch] 공격력 버프 설정
    // ---------------------------------------------------------
    // [임시] 나중에 scriptable object로 관리해야 할듯 임시 공격력
    public int tempStrength = 0; 

    // 프로퍼티 (필요시 사용)
    public float CurrentHp => currentHp;
    public float CurrentShield => currentShield;

    void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // 스프라이트 렌더러 가져오기
        //anim = GetComponent<Animator>();
        
        currentHp = maxHp; // 현재 체력 초기화
        currentShield = 0; // 시작 시 실드는 0
        tempStrength = 0; // 시작 시 버프 0
    }

    void Start()
    {
         UIManager.Instance.UpdateHP(currentHp, maxHp, true); // true는 플레이어
         UIManager.Instance.UpdateHP(100, 100, false); // 적 체력 UI 초기화
    }

    // 턴 시작 시 리소스 초기화 (쉴드 초기화, 에너지 충전, 버프 초기화)
    public void UpdateTurnStartResources()
    {
        currentShield = 0; // 턴 시작 시 실드 초기화
        currentEnergy = Mathf.Min(currentEnergy + 2, maxEnergy); // 에너지 2 회복
        UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy);
        
        // [중요] 턴 시작 시 버프도 초기화되어야 함 (Develop 로직 통합)
        ResetTurnBuffs();
    }

    public void RefillEnergy() // 에너지 완전 회복 함수 (전투 시작 등)
    {
        currentEnergy = maxEnergy; 
        UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy); 
        currentShield = 0; // 실드 리셋
        tempStrength = 0; // 버프 리셋
    }

    // [중요] TryUseEnergy 함수 시그니처 수정 (TargetingSystem의 bool canConsume 반영)
    public bool TryUseEnergy(int cost, bool canConsume = true) // 에너지 사용 시도 함수 + 공격 조건에 맞을 때만 에너지 소모 (기본값 true)
    {
        // 1. 소비 가능 여부 체크 (사거리가 안 닿거나 하면 false가 들어옴)
        if (!canConsume)
            return false;

        // 2. 에너지 충분한지 체크
        if (currentEnergy >= cost) 
        {
            currentEnergy -= cost; // 에너지 차감
            UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy); // UI 갱신
            return true; // 사용 성공
        }
        else
        {
            Debug.Log("에너지가 부족합니다!");
            return false; // 사용 실패
        }
    }

    // ---------------------------------------------------------
    // [Shield System] 쉴드 관련 로직
    // ---------------------------------------------------------
    public void AddShield(float amount)//실드 추가 기능
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        Debug.Log($"쉴드 획득! 현재 쉴드: {currentShield} (최대: {maxShield})");
        // UI 갱신 필요시 여기에 추가
    }

    // [중요] 데미지 로직은 Shield 버전이 최신이므로 이걸 사용합니다.
    public void TakeDamage(float damage) //실드 우선 차감 데미지 로직
    {
        float remainingDamage = damage;

        // 1. 쉴드가 있으면 먼저 깎음
        if (currentShield > 0)
        {
            if (currentShield >= remainingDamage)
            {
                currentShield -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                remainingDamage -= currentShield;
                currentShield = 0;
            }
            Debug.Log($"쉴드로 방어함! 남은 쉴드: {currentShield}");
        }

        // 2. 남은 데미지로 체력 깎음
        currentHp -= remainingDamage;
        if (currentHp < 0) currentHp = 0;
        
        anim.SetTrigger("Damaged");
        UIManager.Instance.UpdateHP(currentHp, maxHp, true);
        if (currentHp <= 0)
        {
            UI_Defeat.Instance.Show(); // UI_Defeat 호출
        }
    }

    // ---------------------------------------------------------
    // [Develop Branch] 버프 관련 로직
    // ---------------------------------------------------------
    
    // [추가] 버프 받는 함수
    public void AddTempStrength(int amount)
    {
        tempStrength += amount;
        Debug.Log($"공격력이 {amount}만큼 증가했습니다! (현재 추가공격력: {tempStrength})");
        // 이펙트나 UI 갱신 코드 추가 가능
    }

    // [추가] 턴 종료 시 호출해서 버프 초기화
    public void ResetTurnBuffs()
    {
        if (tempStrength > 0)
        {
            Debug.Log("턴이 종료되어 버프가 사라집니다.");
            tempStrength = 0;
        }
    }
}