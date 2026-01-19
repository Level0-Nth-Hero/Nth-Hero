using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{   
    public static bool isAnyCardDragging = false;
    public CardData cardData;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image artworkImage;

    private PlayerMove playerScript;
    private CanvasGroup canvasGroup;
    private Canvas myCanvas;
    
    private Quaternion originalRot;   // 원래 회전
    private int originalSiblingIndex; // 원래 순서
    private Vector3 originalScale; 
    
    private bool isDragging = false;
    private bool isHovering = false; // 중복 호버 방지용
    
    private Transform originalParent; // 원래 부모 (HandPanel)

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        myCanvas = GetComponent<Canvas>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        // 시작하자마자 내 부모(HandPanel)가 누군지 기억해야 함
        // 안 그러면 드래그 안 하고 호버만 했다가 나갈 때 부모가 null이라 에러 남
        originalParent = transform.parent; 
    }

    public void Setup(CardData data, PlayerMove pScript)
    {
        this.cardData = data;
        this.playerScript = pScript;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (cardData != null)
        {
            nameText.text = cardData.cardName;
            costText.text = cardData.cost.ToString();
            artworkImage.sprite = cardData.icon;
            if(descriptionText != null) descriptionText.text = cardData.description;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        if (isAnyCardDragging) return;
        //if (eventData.dragging) return; 
        
        // 이미 떠 있으면 호버 ㄴ
        if (isHovering) return; 

        isHovering = true; // 나 떴다 표시

        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = 10; // 호버 시 최상위로

        //originalSiblingIndex = transform.GetSiblingIndex(); 
        //transform.SetAsLastSibling(); 

        transform.localScale = originalScale * 1.2f;
        
        
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 40f, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        
        ResetCardState();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isHovering = false; // 드래그 시작하면 호버 상태는 해제한다고 침

        isAnyCardDragging = true;

        // 부모를 캔버스로 옮김
        Canvas mainCanvas = GetComponentInParent<Canvas>();
        if(mainCanvas != null && mainCanvas.rootCanvas != null)
        {
             // rootCanvas는 최상위 캔버스를 의미함
            transform.SetParent(mainCanvas.rootCanvas.transform);
        }
        else
        {
             transform.SetParent(originalParent.parent);
        }

        // 드래그 중엔 확실하게 맨 위에 보여야 하므로 SortingOrder 최대로
        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = 100;
        
        //transform.SetAsLastSibling();

        // 3. 크기랑 회전 초기화
        transform.localScale = originalScale;
        originalRot = transform.rotation;
        transform.rotation = Quaternion.identity; // 똑바로 세우기

        // 4. 레이캐스트 끄기 (적 인식용)
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        isAnyCardDragging = false;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                if (playerScript.TryUseEnergy(cardData.cost)) 
                {
                    UseCard(target);
                }
                else
                {
                    ResetCardState();
                }
                return;
            }
        }
        
        ResetCardState();
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

    public void UseCard(IDamageable target)
    {
        if (cardData != null && cardData.cardType == CardType.Attack)
        {
            isAnyCardDragging = false;

            Animator anim = playerScript.GetComponent<Animator>();
            ICommand attackCmd = new AttackCommand(target, cardData.value, anim);
            BattleManager.Instance.AddCommand(attackCmd);
            DeckManager.Instance.AddCardToDiscard(cardData);

            Destroy(gameObject);
        }
    }
}