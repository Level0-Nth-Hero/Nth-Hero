using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    private Quaternion originalRot;
    private int originalSiblingIndex;
    private Vector3 originalScale;

    private bool isDragging = false;
    private bool isHovering = false;

    private Transform originalParent;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        myCanvas = GetComponent<Canvas>();
        originalScale = transform.localScale;
    }

    void Start()
    {
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
            if (descriptionText != null) descriptionText.text = cardData.description;
        }
    }

    // ... (OnPointerEnter, Exit, BeginDrag, Drag는 기존과 동일하여 생략 가능하지만, 전체 코드를 위해 유지) ...
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || isAnyCardDragging || isHovering) return;

        isHovering = true;
        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = 10;
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
        isHovering = false;
        isAnyCardDragging = true;

        Canvas mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas != null && mainCanvas.rootCanvas != null)
        {
            transform.SetParent(mainCanvas.rootCanvas.transform);
        }
        else
        {
            transform.SetParent(originalParent.parent);
        }

        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = 100;
        transform.localScale = originalScale;
        originalRot = transform.rotation;
        transform.rotation = Quaternion.identity;
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

        // 1. 적(Target) 위에 드래그했을 때 (공격 카드)
        if (hit.collider != null)
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                // 공격 카드 사용 시도
                bool used = UseCard(target);

                if (!used) 
                {
                    ResetCardState(); // 실패하면 되돌아감
                }
                return; // 성공했으면 UseCard 안에서 Destroy 되므로 여기서 끝
            }
        }
        // 2. 스킬/버프 카드: 그냥 허공(위쪽)에 던졌을 때
        else if (cardData.cardType == CardType.Skill)
        {
            if (Input.mousePosition.y > Screen.height * 0.25f)
            {
                if (playerScript.TryUseEnergy(cardData.cost))
                {
                    UseCard(null); // 타겟 없이 사용
                    return;
                }
            }
        }

        ResetCardState(); // 아무 조건도 안 맞으면 원래 자리로
    }

    void ResetCardState()
    {
        isHovering = false;
        myCanvas.overrideSorting = false;
        myCanvas.sortingOrder = 0;
        transform.SetParent(originalParent);
        transform.localScale = originalScale;
        DeckManager.Instance.AlignCards();
    }

    // [핵심 수정] 공격과 스킬 로직을 하나로 합친 메서드
    public bool UseCard(IDamageable target)
    {
        if (cardData == null) return false;

        // ====================================================
        // CASE 1: 공격 카드 (TargetingSystem 브랜치 내용)
        // ====================================================
        if (cardData.cardType == CardType.Attack)
        {
            if (target == null) return false;

            isAnyCardDragging = false;
            Animator anim = playerScript.GetComponent<Animator>();

            // 1. 직업별 공격 사거/조건 체크
            List<IAttackCondition> conditions = new List<IAttackCondition>();
            switch (playerScript.playerJob)
            {
                case PlayerJob.Warrior:
                    conditions.Add(new WarriorAttack());
                    break;
                case PlayerJob.Archer:
                    conditions.Add(new ArcherAttack());
                    break;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].CanAttack(target.TargetTransform))
                {
                    Debug.Log("범위 밖이라 카드 사용 불가");
                    return false;
                }
            }

            // 2. 에너지 사용 (공격 카드는 여기서 체크)
            if (!playerScript.TryUseEnergy(cardData.cost, true))
                return false;

            // 3. 커맨드 생성 및 실행
            AttackCommand attackCmd = new AttackCommand(target, target.TargetTransform, cardData.value, anim, conditions);
            BattleManager.Instance.AddCommand(attackCmd);
            
            DeckManager.Instance.AddCardToDiscard(cardData);
            Destroy(gameObject);
            return true;
        }

        // ====================================================
        // CASE 2: 스킬 카드 (Develop 브랜치 내용)
        // ====================================================
        else if (cardData.cardType == CardType.Skill)
        {
            isAnyCardDragging = false;

            // 1. 효과 타입에 따른 커맨드 생성
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
                    Debug.LogWarning("아직 구현되지 않은 스킬 효과: " + cardData.cardName);
                    break;
            }

            if (skillCmd != null)
            {
                BattleManager.Instance.AddCommand(skillCmd);
            }

            // 2. 소멸/무덤 처리
            if (cardData.isExhaust)
                DeckManager.Instance.AddCardToExhaust(cardData);
            else
                DeckManager.Instance.AddCardToDiscard(cardData);

            Destroy(gameObject);
            return true;
        }

        return false;
    }
}