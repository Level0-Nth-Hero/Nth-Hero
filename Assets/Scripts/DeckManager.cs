using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour // 덱과 무덤을 관리하는 매니저
{
    public static DeckManager Instance; // 싱글톤 인스턴스

    [Header("설정")] // 카드 프리팹과 핸드 패널
    public GameObject cardPrefab; // 카드 프리팹
    public Transform handPanel; // 핸드 패널 트랜스폼
    
    [Header("부채꼴 정렬 설정")]
    [Range(0, 500)] public float spacing = 180f;    // 카드 사이 간격
    [Range(0, 90)] public float totalAngle = 30f;   // 전체 부채꼴 각도
    [Range(0, 100)] public float archHeight = 40f;  // 둥근 정도 (아치 높이)

    [Header("타겟 정보")] // 플레이어와 적 스크립트
    public PlayerMove player; // 플레이어 스크립트
    public EnemyMove enemy; // 적 스크립트

    [Header("데이터")] // 초기 덱 리스트와 현재 덱, 무덤
    public List<CardData> initialDeck; // 초기 덱 (인스펙터에서 설정)
    private List<CardData> currentDeck = new List<CardData>(); // 현재 덱
    public List<CardData> discardDeck = new List<CardData>(); // 무덤
    // [추가] 소멸된 카드 리스트 (게임 중엔 안 섞이고, 전투 끝나면 복구용)
    public List<CardData> exhaustDeck = new List<CardData>();

    void Awake() { Instance = this; } // 싱글톤 초기화

    void Start()
    {
        SetupDeck(); // 덱 설정
    }

    public void SetupDeck() // 덱 초기화 함수
    {
        currentDeck.Clear(); // 현재 덱 비우기
        discardDeck.Clear(); // 무덤 비우기
        exhaustDeck.Clear(); // 소멸 덱 비우기

        for (int i = 0; i < initialDeck.Count; i++) 
        {
            currentDeck.Add(initialDeck[i]); // 초기 덱의 카드를 현재 덱에 복사
        }
        Shuffle(); // 덱 섞기

        UIManager.Instance.UpdateDiscardCount(0); // 무덤 카운트 UI 갱신
        UIManager.Instance.UpdateCurrentCount(currentDeck.Count); // 현재 덱 카운트 UI 갱신
        UIManager.Instance.UpdateExhaustCount(0); // 소멸 카운트 UI 갱신
    }

    public void Shuffle() // 덱 섞기 함수
    {
        for (int i = 0; i < currentDeck.Count; i++)
        {
            int randomIndex = Random.Range(0, currentDeck.Count); // 랜덤 인덱스 선택
            CardData temp = currentDeck[i]; // 카드 교환
            currentDeck[i] = currentDeck[randomIndex]; // 카드 교환
            currentDeck[randomIndex] = temp; // 카드 교환
        }
    }


    public void DrawCard(int count) // 카드 뽑기 함수
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
            CardData cardData = currentDeck[0]; // 맨 위 카드 가져오기
            currentDeck.RemoveAt(0); // 덱에서 제거

            UIManager.Instance.UpdateCurrentCount(currentDeck.Count); // 현재 덱 카운트 UI 갱신
            // 3. 카드 오브젝트 생성 및 설정
            GameObject newCardObj = Instantiate(cardPrefab, handPanel); // 핸드 패널에 카드 프리팹 생성
            CardDisplay display = newCardObj.GetComponent<CardDisplay>(); // 카드 디스플레이 컴포넌트 가져오기
            display.Setup(cardData, player); // 카드 데이터와 플레이어 스크립트 설정
        }

        AlignCards(); // 카드 부채꼴 정렬
    }

    // [신규] 카드를 무덤으로 보내는 함수 (데이터만)
    public void AddCardToDiscard(CardData card) // 무덤에 카드 추가
    {
        discardDeck.Add(card); // 무덤에 카드 데이터 추가
        UIManager.Instance.UpdateDiscardCount(discardDeck.Count); // 무덤 카운트 UI 갱신
    }

    public void AddCardToExhaust(CardData card)
    {
        exhaustDeck.Add(card);
        Debug.Log($"{card.cardName} 카드가 소멸되었습니다! (이번 전투에서 제외)");
        UIManager.Instance.UpdateExhaustCount(exhaustDeck.Count);
    }

    void ReshuffleDiscardToDeck() // 무덤을 덱으로 섞어 옮기는 함수
    {
        Debug.Log("덱이 다 떨어져서 무덤을 섞어 가져옵니다!");

        // 무덤의 모든 카드를 덱으로 이동
        foreach (CardData card in discardDeck) // 무덤의 카드들을
        {
            currentDeck.Add(card); // 덱으로 이동
        }
        discardDeck.Clear(); // 무덤 비우기

        UIManager.Instance.UpdateDiscardCount(0); // 무덤 카운트 UI 갱신

        Shuffle(); // 섞기
    }

    public void DiscardHand() // 핸드의 모든 카드를 무덤으로 보내는 함수
    {
        // HandPanel의 모든 자식(카드)을 돌면서
        foreach (Transform child in handPanel)
        {
            CardDisplay cardDisplay = child.GetComponent<CardDisplay>(); // 카드 디스플레이 컴포넌트 가져오기
            if (cardDisplay != null) // null 체크
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
        int cardCount = handPanel.childCount; // 핸드 패널의 카드 개수
        
        if (cardCount == 0) return; // 카드가 없으면 종료

        float centerIndex = (cardCount - 1) / 2f; // 중앙 인덱스 계산

        for (int i = 0; i < cardCount; i++) // 모든 카드에 대해
        {
            Transform card = handPanel.GetChild(i); // i번째 카드 가져오기 Position, rotation and scale of an object.는 Transform 컴포넌트에서 제공하는 속성입니다. 이 속성들은 게임 오브젝트의 위치, 회전 및 크기를 제어하는 데 사용됩니다.
            
            float offset = i - centerIndex; 

            // 1. 회전 계산 (왼쪽은 +, 오른쪽은 - 회전)
            // 총 각도를 카드 개수로 나눠서 분배
            float rotationAngle = -offset * (totalAngle / Mathf.Max(1, cardCount - 1));
            // * 카드 한 장일 때는 회전 안 하게 Max 처리

            // 2. X 위치 계산 (간격 벌리기)
            float posX = offset * spacing;

            // 3. Y 위치 계산 (둥근 아치 만들기)
            // y = -x^2 그래프를 생각하면 됨. 중심에서 멀어질수록 아래로 내려감.
            float posY = -Mathf.Abs(offset) * Mathf.Abs(offset) * (archHeight / 10f); // 값 보정 / 10f

            card.localPosition = new Vector3(posX, posY, 0); // 위치 설정
            card.localRotation = Quaternion.Euler(0, 0, rotationAngle); // 회전 설정
        }
    }
    
    // [팁] 게임 실행 중에 인스펙터 값을 바꾸면 바로바로 적용되게 하는 기능
    void OnValidate() // 인스펙터 값 변경 시 호출
    {
        if (handPanel != null) // 핸드 패널이 설정되어 있으면
        {
            AlignCards(); // 카드 정렬 함수 호출
        }
    }
}