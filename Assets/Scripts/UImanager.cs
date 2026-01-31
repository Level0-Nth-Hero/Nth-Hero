using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트메쉬프로 쓰려면 필수
using System.Collections;

public class UIManager : MonoBehaviour
{
    // 싱글톤: 어디서든 UIManager.Instance 로 부르기 위해
    public static UIManager Instance;

    [Header("HP UI")]
    public Slider playerHpSlider; // 플레이어 HP 슬라이더
    public Slider enemyHpSlider; // 적 HP 슬라이더

    [Header("Turn UI")]
    public TMP_Text turnStateText; // "나의 턴 / 적의 턴"
    public TMP_Text turnCountText; // "Turn: 1"

    [Header("Dialogue UI")]
    public CanvasGroup dialogueGroup; // 페이드 효과용
    public TMP_Text dialogueText; // 대사 텍스트

    [Header("Energy UI")]
    public TMP_Text energyText; // 에너지 텍스트

    [Header("Deck UI")]
    public TMP_Text discardCountText; // 버린 카드 수 텍스트
    public TMP_Text currentCountText; // 현재 덱 카드 수 텍스트
    public TMP_Text exhaustCountText; // 소멸된 카드 수 텍스트

    void Awake()
    {
        Instance = this;
    }

    // 1. HP 갱신 함수
    public void UpdateHP(float currentHp, float maxHp, bool isPlayer)
    {
        if (isPlayer)
        {
            playerHpSlider.value = currentHp / maxHp; // 0.0 ~ 1.0 비율로 변환
        }
        else
        {
            enemyHpSlider.value = currentHp / maxHp;
        }
    }

    // 2. 턴 정보 갱신 함수
    public void UpdateTurnInfo(int turnCount, bool isPlayerTurn) // isPlayerTurn: 플레이어 턴인지 여부
    {
        turnCountText.text = "Turn: " + turnCount; // 턴 카운트 갱신
        
        if (isPlayerTurn)
        {
            turnStateText.text = "Player Turn";
            turnStateText.color = Color.cyan; // 플레이어 턴 색상
        }
        else
        {
            turnStateText.text = "Enemy Turn";
            turnStateText.color = Color.red; // 적 턴 색상
        }
    }

    // 3. 대사 출력 (페이드 인 -> 대기 -> 페이드 아웃)
    public void ShowDialogue(string text, float duration)
    {
        StartCoroutine(DialogueRoutine(text, duration)); // 코루틴 시작
    }

    public void UpdateEnergy(int current, int max) // 에너지 갱신
    {
        energyText.text = $"{current} / {max}";
    }

    public void UpdateDiscardCount(int count) // 무덤 카드 수 갱신
    {
        discardCountText.text = count.ToString();
    }

    public void UpdateCurrentCount(int count) // 현재 덱 카드 수 갱신
    {
        currentCountText.text = count.ToString();
    }

    public void UpdateExhaustCount(int count) // 소멸 카드 수 갱신
    {
        if (exhaustCountText != null)
        {
            exhaustCountText.text = count.ToString();
        }
    }

    IEnumerator DialogueRoutine(string text, float duration) // 대사 출력 코루틴
    {
        dialogueText.text = text;

        // 페이드 인 (0 -> 1)
        float fadeTime = 0.5f; // 페이드 시간
        float time = 0; // 경과 시간
        while(time < fadeTime) // 페이드 인
        {
            time += Time.deltaTime; 
            dialogueGroup.alpha = Mathf.Lerp(0, 1, time / fadeTime); // 알파값 보간은 Lerp 사용은 무슨 의미냐면 처음부터 끝까지 부드럽게 변화시키는 것
            yield return null; // 한 프레임 대기 null 은 다음 프레임까지 기다리라는 의미 그 다음 프레임은 16.67ms 후임
        }
        dialogueGroup.alpha = 1; // 완전 보임

        // 대사 유지
        yield return new WaitForSeconds(duration); // 대기

        // 페이드 아웃 (1 -> 0)
        time = 0;
        while (time < fadeTime) // 페이드 아웃
        {
            time += Time.deltaTime; // 경과 시간 증가
            dialogueGroup.alpha = Mathf.Lerp(1, 0, time / fadeTime); // 알파값 보간
            yield return null; // 한 프레임 대기
        }
        dialogueGroup.alpha = 0; // 완전 안 보임
    }
}