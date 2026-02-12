using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // 리스트(List) 기능을 사용하기 위해 필요

public class UI_Reward : MonoBehaviour
{
    public static UI_Reward Instance;

    [Header("UI Objects")]
    public RectTransform victoryText;      // "Victory!" 글자 오브젝트 (위치, 크기 조절용)
    public GameObject cardContainer;       // 생성된 카드 3장이 들어갈 바구니 (부모 오브젝트)
    public CanvasGroup canvasGroup;        // 패널 전체의 투명도와 클릭 가능 여부를 조절하는 컴포넌트

    [Header("Reward Settings")]
    public GameObject cardPrefab;          // 화면에 새로 만들 카드 오브젝트의 원본(설계도)
    public PlayerMove player;              // 카드를 생성할 때 플레이어 정보를 넘겨주기 위한 참조
    
    // 유니티 인스펙터에서 보상 후보로 등록할 카드 데이터 리스트입니다.
    public List<CardData> allRewardCards; 

    private void Awake()
    {
        Instance = this;
        
        canvasGroup.alpha = 0;              // 투명하게 만들기
        canvasGroup.blocksRaycasts = false; // 뒤에 있는 버튼 등이 눌리지 않게 클릭 차단 해제
        canvasGroup.interactable = false;   // 상호작용 불가능하게 설정
        
        // 글자도 미리 크기를 0으로
        if (victoryText != null) victoryText.localScale = Vector3.zero;
    }

    public void Show()
    {
        gameObject.SetActive(true);         // 오브젝트 활성화
        StartCoroutine(RewardSequence());   // 전체 연출 시퀀스(코루틴) 시작
    }

    // 전체적인 보상 연출 순서를 관리하는 코루틴
    IEnumerator RewardSequence()
    {
        // --- 1단계: 배경 패널 페이드 인 ---
        canvasGroup.interactable = true;    // 이제 UI 조작 가능
        canvasGroup.blocksRaycasts = true;  // 마우스 클릭 감지 시작

        float fadeT = 0;
        while (fadeT < 1.0f)
        {
            fadeT += Time.deltaTime * 2f;   // 약 0.5초 동안 진행
            canvasGroup.alpha = fadeT;      // 서서히 밝아짐
            yield return null;
        }

        // --- 2단계: Victory! 글자 등장 ---
        victoryText.localScale = Vector3.zero;
        float popT = 0;
        while (popT < 1.0f)
        {
            popT += Time.deltaTime * 2f;
            // 0에서 1.5배까지 커지는 연출
            victoryText.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, popT);
            yield return null;
        }

        // 중앙에서 잠시 멈춤 (여운을 주는 시간)
        yield return new WaitForSeconds(0.5f);

        // --- 3단계: Victory! 글자가 작아지며 위로 이동 ---
        float moveT = 0;
        Vector2 startPos = Vector2.zero;    // 화면 중앙
        Vector2 endPos = new Vector2(0, 200f); // 중앙 위쪽으로 200만큼 이동
        while (moveT < 1.0f)
        {
            moveT += Time.deltaTime * 2f;
            // 위치와 크기를 동시에 변화시킴
            victoryText.anchoredPosition = Vector2.Lerp(startPos, endPos, moveT);
            victoryText.localScale = Vector2.Lerp(Vector3.one * 1.5f, Vector3.one, moveT);
            yield return null;
        }

        // --- 4단계: 카드 등장 함수 실행 ---
        SpawnRewardCards();
    }

    // 무작위로 3장의 카드를 생성하는 함수
    void SpawnRewardCards()
    {
        // 보상 후보가 3개 미만이면 오류가 날 수 있으므로 경고 메시지 출력
        if (allRewardCards.Count < 3)
        {
            Debug.LogError("UI_Reward의 All Reward Cards 리스트에 카드를 3개 이상 넣어주세요!");
            return;
        }

        // 원본 리스트를 건드리지 않기 위해 임시 리스트에 복사 (중복 제거용)
        List<CardData> tempCards = new List<CardData>(allRewardCards);

        for (int i = 0; i < 3; i++)
        {
            // 남은 후보 카드 중 랜덤하게 하나를 선택
            int randomIndex = Random.Range(0, tempCards.Count);
            CardData selectedData = tempCards[randomIndex];

            // 카드 프리팹을 바구니(cardContainer)의 자식으로 생성
            GameObject newCard = Instantiate(cardPrefab, cardContainer.transform);
            
            // 생성된 카드의 CardDisplay 컴포넌트를 가져와 데이터를 세팅
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.Setup(selectedData, player);
            }

            // 각 카드마다 빙글빙글 도는 애니메이션 코루틴을 실행
            StartCoroutine(FlipCardRoutine(newCard.transform));
            
            // 뽑힌 카드는 임시 리스트에서 지워 다음 반복에서 중복되지 않게
            tempCards.RemoveAt(randomIndex);
        }
    }


    // 카드가 생성될 때 가로로 회전하는 연출
    IEnumerator FlipCardRoutine(Transform cardTransform)
    {
        // --- 수치 조절 구간 ---
        float duration = 1.5f;    // 회전이 지속되는 전체 시간 (더 오래: 1.5초 ~ 2초 추천)
        int rotateCount = 8;     // 회전할 바퀴 수 (더 빠르게: 5바퀴 이상 추천)
        // -----------------------

        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            float yAngle = Mathf.Lerp(0, 360f * rotateCount, timer / duration);
            
            cardTransform.localEulerAngles = new Vector3(0, yAngle, 0);
            yield return null;
        }

        // 마지막에는 정면을 바라보게 고정
        cardTransform.localEulerAngles = Vector3.zero;

        // 마지막 멈추는 임팩트 연출
        cardTransform.localScale = Vector3.one * 1.2f; // 살짝 더 크게 튀기기
        yield return new WaitForSeconds(0.1f);
        cardTransform.localScale = Vector3.one;
    }
}