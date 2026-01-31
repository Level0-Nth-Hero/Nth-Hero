using UnityEngine;

public class PlayerMove : MonoBehaviour , IDamageable // 플레이어 이동 및 상태 관리 클래스
{
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    [SerializeField] private Animator anim; // 애니메이터

    [Header("에너지")] // 에너지 설정
    public int maxEnergy = 3; // 최대 에너지
    public int currentEnergy; // 현재 에너지

    public float maxHp { get; private set; } = 100; // 최대 체력
    public float currentHp { get; private set; } // 현재 체력

    //[임시] 나중에 scriptable object로 관리해야 할듯 임시 공격력
    public int tempStrength = 0; 

    void Awake() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // 스프라이트 렌더러 가져오기
        //anim = GetComponent<Animator>();
        
        currentHp = maxHp; // 현재 체력 초기화
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

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        anim.SetTrigger("Damaged");
        if (currentHp < 0) currentHp = 0;
        UIManager.Instance.UpdateHP(currentHp, maxHp, true);
    }

}