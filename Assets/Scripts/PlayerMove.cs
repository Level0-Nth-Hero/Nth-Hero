using UnityEngine;

public class PlayerMove : MonoBehaviour , IDamageable // 플레이어 이동 및 상태 관리 클래스
{
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    [SerializeField] private Animator anim; // 애니메이터

    [Header("에너지")] // 에너지 설정
    public int maxEnergy = 3; // 최대 에너지
    public int currentEnergy; // 현재 에너지
    public float maxShield = 15f; // 쉴드 최대치 제한

    public float maxHp { get; private set; } = 100; // 최대 체력
    public float currentHp { get; private set; } // 현재 체력

    public float currentShield { get; private set; } // 현재 쉴드량 추가

    public float CurrentHp => currentHp;
    public float CurrentShield => currentShield;


    void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // 스프라이트 렌더러 가져오기
        //anim = GetComponent<Animator>();
        
        currentHp = maxHp; // 현재 체력 초기화
        currentShield = 0; // 시작 시 실드는 0
    }

    public void UpdateTurnStartResources()//턴 시작 시 방어도 초기화 및 코스트 +2 충전
    {
        currentShield = 0; // 턴 시작 시 실드 초기화
        currentEnergy = Mathf.Min(currentEnergy + 2, maxEnergy);
        UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy);
        
    }

    public void AddShield(float amount)//실드 추가 기능
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        Debug.Log($"쉴드 획득! 현재 쉴드: {currentShield} (최대: {maxShield})");
    }

    public void TakeDamage(float damage) //실드 우선 차감 데미지 로직
    {
        float remainingDamage = damage;

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
        }

        currentHp -= remainingDamage;
        if (currentHp < 0) currentHp = 0;
        anim.SetTrigger("Damaged");
        UIManager.Instance.UpdateHP(currentHp, maxHp, true);
    }

    void Start()
    {
         UIManager.Instance.UpdateHP(currentHp, maxHp, true); // true는 플레이어
         UIManager.Instance.UpdateHP(100, 100, false); // 적 체력 UI 초기화
    }

    public void RefillEnergy() // 에너지 채우기 함수
    {
        currentEnergy = maxEnergy; // 에너지 최대치로 채우기
        UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy); // UI 갱신
        currentShield = 0;//실드 리셋
    }

    public bool TryUseEnergy(int cost) // 에너지 사용 시도 함수
    {
        if (currentEnergy >= cost) // 충분한 에너지 있으면
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

}