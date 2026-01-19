using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("설정")]
    public GameObject cardPrefab;
    public Transform handPanel;
    
    [Header("부채꼴 정렬 설정")]
    [Range(0, 500)] public float spacing = 180f;    // 카드 사이 간격
    [Range(0, 90)] public float totalAngle = 30f;   // 전체 부채꼴 각도
    [Range(0, 100)] public float archHeight = 40f;  // 둥근 정도 (아치 높이)

    [Header("타겟 정보")]
    public PlayerMove player;
    public EnemyMove enemy;

    [Header("데이터")]
    public List<CardData> initialDeck;
    private List<CardData> currentDeck = new List<CardData>();
    public List<CardData> discardDeck = new List<CardData>();

    void Awake() { Instance = this; }

    void Start()
    {
        SetupDeck();
    }

    public void SetupDeck()
    {
        currentDeck.Clear();
        discardDeck.Clear();

        for (int i = 0; i < initialDeck.Count; i++)
        {
            currentDeck.Add(initialDeck[i]);
        }
        Shuffle();

        UIManager.Instance.UpdateDiscardCount(0);
        UIManager.Instance.UpdateCurrentCount(currentDeck.Count);
    }

    public void Shuffle()
    {
        for (int i = 0; i < currentDeck.Count; i++)
        {
            int randomIndex = Random.Range(0, currentDeck.Count);
            CardData temp = currentDeck[i];
            currentDeck[i] = currentDeck[randomIndex];
            currentDeck[randomIndex] = temp;
        }
    }


    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 1. 덱이 비었는지 확인
            if (currentDeck.Count <= 0)
            {
                // 1-1. 무덤도 비었으면 더 이상 뽑을 게 없음 -> 중단
                if (discardDeck.Count <= 0) break;

                // 1-2. 무덤에 카드가 있으면 -> 덱으로 옮기고 셔플
                ReshuffleDiscardToDeck();
            }

            // 2. 카드 뽑기
            CardData cardData = currentDeck[0];
            currentDeck.RemoveAt(0);

            UIManager.Instance.UpdateCurrentCount(currentDeck.Count);

            GameObject newCardObj = Instantiate(cardPrefab, handPanel);
            CardDisplay display = newCardObj.GetComponent<CardDisplay>();
            display.Setup(cardData, player); 
        }

        AlignCards();
    }

    // [신규] 카드를 무덤으로 보내는 함수 (데이터만)
    public void AddCardToDiscard(CardData card)
    {
        discardDeck.Add(card);
        UIManager.Instance.UpdateDiscardCount(discardDeck.Count);
    }

    void ReshuffleDiscardToDeck()
    {
        Debug.Log("덱이 다 떨어져서 무덤을 섞어 가져옵니다!");

        // 무덤의 모든 카드를 덱으로 이동
        foreach (CardData card in discardDeck)
        {
            currentDeck.Add(card);
        }
        discardDeck.Clear(); // 무덤 비우기

        UIManager.Instance.UpdateDiscardCount(0);

        Shuffle(); // 섞기
    }

    public void DiscardHand()
    {
        // HandPanel의 모든 자식(카드)을 돌면서
        foreach (Transform child in handPanel)
        {
            CardDisplay cardDisplay = child.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                // 무덤에 데이터 추가
                AddCardToDiscard(cardDisplay.cardData);
            }
            
            // 오브젝트 파괴
            Destroy(child.gameObject);
        }
    }

    //카드 부채꼴 정렬 함수
    public void AlignCards()
    {
        int cardCount = handPanel.childCount;
        
        if (cardCount == 0) return;

        float centerIndex = (cardCount - 1) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            Transform card = handPanel.GetChild(i);
            
            float offset = i - centerIndex;

            // 1. 회전 계산 (왼쪽은 +, 오른쪽은 - 회전)
            // 총 각도를 카드 개수로 나눠서 분배
            float rotationAngle = -offset * (totalAngle / Mathf.Max(1, cardCount - 1));
            // * 카드 한 장일 때는 회전 안 하게 Max 처리

            // 2. X 위치 계산 (간격 벌리기)
            float posX = offset * spacing;

            // 3. Y 위치 계산 (둥근 아치 만들기)
            // y = -x^2 그래프를 생각하면 됨. 중심에서 멀어질수록 아래로 내려감.
            float posY = -Mathf.Abs(offset) * Mathf.Abs(offset) * (archHeight / 10f); // 값 보정

            card.localPosition = new Vector3(posX, posY, 0);
            card.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        }
    }
    
    // [팁] 게임 실행 중에 인스펙터 값을 바꾸면 바로바로 적용되게 하는 기능
    void OnValidate()
    {
        if (handPanel != null)
        {
            AlignCards();
        }
    }
}