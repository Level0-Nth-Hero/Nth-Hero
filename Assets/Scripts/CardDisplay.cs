using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler // 카드 UI 표시 및 드래그/호버 처리 클래스
{   
    public static bool isAnyCardDragging = false; // 전체 카드 중 드래그 중인 카드가 있는지 여부
    public CardData cardData; // 카드 데이터 참조

    public TextMeshProUGUI nameText; // 카드 이름 텍스트
    public TextMeshProUGUI costText; // 카드 비용 텍스트
    public TextMeshProUGUI descriptionText; // 카드 설명 텍스트
    public Image artworkImage; // 카드 이미지

    private PlayerMove playerScript; // 플레이어 스크립트 참조
    private CanvasGroup canvasGroup; // 캔버스 그룹 (드래그 시 레이캐스트 제어용)
    private Canvas myCanvas; // 내 캔버스 참조
    
    private Quaternion originalRot;   // 원래 회전
    private int originalSiblingIndex; // 원래 순서
    private Vector3 originalScale;  // 원래 크기
    
    private bool isDragging = false; // 드래그 중인지 여부
    private bool isHovering = false; // 중복 호버 방지용
    
    private Transform originalParent; // 원래 부모 (HandPanel)

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>(); // 캔버스 그룹 가져오기
        myCanvas = GetComponent<Canvas>(); // 내 캔버스 가져오기
        originalScale = transform.localScale; // 원래 크기 저장
    }

    void Start() 
    {
        // 시작하자마자 내 부모(HandPanel)가 누군지 기억해야 함
        // 안 그러면 드래그 안 하고 호버만 했다가 나갈 때 부모가 null이라 에러 남
        originalParent = transform.parent; // 원래 부모 저장 
    }

    public void Setup(CardData data, PlayerMove pScript) // 카드 데이터와 플레이어 스크립트로 초기화
    {
        this.cardData = data; // 카드 데이터 설정
        this.playerScript = pScript; // 플레이어 스크립트 설정
        RefreshUI(); // UI 갱신
    }

    void RefreshUI() // 카드 UI 갱신 함수
    {
        if (cardData != null) // 카드 데이터가 있으면
        {
            nameText.text = cardData.cardName; // 이름 설정
            costText.text = cardData.cost.ToString(); // 비용 설정
            artworkImage.sprite = cardData.icon; // 이미지 설정
            if(descriptionText != null) descriptionText.text = cardData.description; // 설명 설정
        }
    }

    public void OnPointerEnter(PointerEventData eventData) // 마우스 오버 시
    {
        if (isDragging) return; // 드래그 중이면 호버 ㄴ
        if (isAnyCardDragging) return; // 다른 카드가 드래그 중이면 호버 ㄴ
        //if (eventData.dragging) return; 
        
        // 이미 떠 있으면 호버 ㄴ
        if (isHovering) return; 

        isHovering = true; // 나 떴다 표시

        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = 10; // 호버 시 최상위로

        //originalSiblingIndex = transform.GetSiblingIndex(); 
        //transform.SetAsLastSibling(); 

        transform.localScale = originalScale * 1.2f; // 크기 1.2배 확대
        
        
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 40f, 0); // Y 위치 살짝 올리기
    }

    public void OnPointerExit(PointerEventData eventData) // 마우스 나갈 시
    {
        if (isDragging) return; // 드래그 중이면 무시
        
        ResetCardState(); // 카드 상태 초기화
    }

    public void OnBeginDrag(PointerEventData eventData) // 드래그 시작 시
    {
        isDragging = true; // 드래그 상태 표시
        isHovering = false; // 드래그 시작하면 호버 상태는 해제한다고 침

        isAnyCardDragging = true; // 전체 카드 중 드래그 중인 카드가 있다고 표시

        // 부모를 캔버스로 옮김
        Canvas mainCanvas = GetComponentInParent<Canvas>(); // 최상위 캔버스 찾기
        if(mainCanvas != null && mainCanvas.rootCanvas != null) // 루트 캔버스가 있으면
        {
             // rootCanvas는 최상위 캔버스를 의미함
            transform.SetParent(mainCanvas.rootCanvas.transform); // 최상위 캔버스로 부모 변경
        }
        else // 안전장치
        {
             transform.SetParent(originalParent.parent); // 원래 부모의 부모로 변경
        }

        // 드래그 중엔 확실하게 맨 위에 보여야 하므로 SortingOrder 최대로
        myCanvas.overrideSorting = true; // 강제 오버라이드
        myCanvas.sortingOrder = 100; // 최상위로
        
        //transform.SetAsLastSibling();

        // 3. 크기랑 회전 초기화
        transform.localScale = originalScale; // 원래 크기
        originalRot = transform.rotation; // 원래 회전 저장
        transform.rotation = Quaternion.identity; // 똑바로 세우기

        // 4. 레이캐스트 끄기 (적 인식용)
        canvasGroup.blocksRaycasts = false; // 드래그 중엔 레이캐스트 무시
    }

    public void OnDrag(PointerEventData eventData) // 드래그 중일 때
    {
        transform.position = eventData.position; // 마우스 위치로 이동
    }

    public void OnEndDrag(PointerEventData eventData) // 드래그 끝났을 때
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true; // 레이캐스트 다시 켜기

        isAnyCardDragging = false; // 전체 카드 중 드래그 중인 카드 없다고 표시

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 마우스 위치 월드 좌표로 변환
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero); // 마우스 위치에 레이캐스트 발사

        if (hit.collider != null) // 뭔가 맞았으면
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>(); // 맞은 대상이 IDamageable인지 확인
            if (target != null) // 맞은 대상이 IDamageable이면
            {
                if (playerScript.TryUseEnergy(cardData.cost)) // 에너지 충분하면
                {
                    UseCard(target); // 카드 사용
                }
                else // 에너지 부족하면
                {
                    ResetCardState(); // 카드 상태 초기화
                }
                return; // 성공했으면 종료
            }
        }
        // 2. 스킬/버프 카드: 그냥 허공에 던지면 됨
        else if (cardData.cardType == CardType.Skill)
        {
            // 카드를 화면 중간 정도(핸드보다 위)까지 드래그했는지 확인
            // (Y값이 -200~-300 정도가 보통 핸드 위치라고 가정하면, 
            //  local position이나 world position을 체크하거나, 
            //  가장 쉬운 건 그냥 "원래 위치보다 많이 올라갔나?" 체크)

            // 팁: transform.position.y가 카드 패널보다 확실히 위쪽인지 체크
            // 여기서는 단순히 '드래그를 했다면' 사용하는 걸로 하되, 실수 방지를 위해
            // 화면 하단(핸드 영역)을 벗어났는지만 봅니다.

            // 화면 높이의 1/4 이상 위로 드래그했으면 사용 (실수 방지)
            if (Input.mousePosition.y > Screen.height * 0.25f)
            {
                if (playerScript.TryUseEnergy(cardData.cost))
                {
                    UseCard(null); // 타겟 필요 없음 (null 전달)
                    return;
                }
            }
        }

        ResetCardState(); // 카드 상태 초기화
    }

    void ResetCardState()
    {
        isHovering = false; // 호버 상태 끄기

        myCanvas.overrideSorting = false;
        myCanvas.sortingOrder = 0;
        // 1. 원래 부모(HandPanel)로 복귀
        // (Start에서 찾아놨으니 안전함)
        transform.SetParent(originalParent);

        // 2. 크기 복구
        transform.localScale = originalScale;

        // 3. [핵심] 위치는 DeckManager가 잡아줌
        // 이렇게 하면 아까 +40 했던 것도 무시하고 정확한 곡선 위치로 강제 이동됨
        DeckManager.Instance.AlignCards();
    }

    public void UseCard(IDamageable target) // 카드를 사용해서 대상에게 효과 적용
    {
        if (cardData != null && cardData.cardType == CardType.Attack) // 공격 카드면
        {
            isAnyCardDragging = false; // 드래그 상태 해제

            Animator anim = playerScript.GetComponent<Animator>(); // 플레이어 애니메이터 가져오기
            int finalDamage = cardData.value + playerScript.tempStrength;
            ICommand attackCmd = new AttackCommand(target, finalDamage, anim); // 공격 커맨드 생성
            BattleManager.Instance.AddCommand(attackCmd); // 커맨드 매니저에 등록
            if (cardData.isExhaust)
            {
                DeckManager.Instance.AddCardToExhaust(cardData); // 소멸존으로!
            }
            else
            {
                DeckManager.Instance.AddCardToDiscard(cardData); // 무덤으로!
            }

            Destroy(gameObject); // 카드 오브젝트 파괴
        }
        else if (cardData != null && cardData.cardType == CardType.Skill)
        {
            isAnyCardDragging = false; // 드래그 해제

            // 1. 효과 타입(effectType)에 따라 다른 커맨드 생성!
            // (주의: CardEffectType은 나중에 유니티 가서 추가할 예정임. 지금은 그냥 적어두기!)
            ICommand skillCmd = null;

            switch (cardData.effectType)
            {
                case CardEffectType.Shield:
                    skillCmd = new ShieldCommand(playerScript, cardData.value);
                    break;

                case CardEffectType.BuffStrength:
                    skillCmd = new BuffCommand(playerScript, cardData.value);
                    break;
                
                default:
                    Debug.LogWarning("아직 구현되지 않은 스킬 효과입니다: " + cardData.cardName);
                    break;
            }

            // 커맨드가 생성되었다면 실행
            if (skillCmd != null)
            {
                BattleManager.Instance.AddCommand(skillCmd);
            }

            // 2. 카드 뒷정리 (develop 브랜치의 소멸/무덤 로직 적용)
            if (cardData.isExhaust) 
                DeckManager.Instance.AddCardToExhaust(cardData);
            else 
                DeckManager.Instance.AddCardToDiscard(cardData);

            Destroy(gameObject); // 카드 파괴
        }
}