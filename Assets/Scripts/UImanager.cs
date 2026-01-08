using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트메쉬프로 쓰려면 필수
using System.Collections;

public class UIManager : MonoBehaviour
{
    // 싱글톤: 어디서든 UIManager.Instance 로 부르기 위해
    public static UIManager Instance;

    [Header("HP UI")]
    public Slider playerHpSlider;
    public Slider enemyHpSlider;

    [Header("Turn UI")]
    public TMP_Text turnStateText; // "나의 턴 / 적의 턴"
    public TMP_Text turnCountText; // "Turn: 1"

    [Header("Dialogue UI")]
    public CanvasGroup dialogueGroup; // 페이드 효과용
    public TMP_Text dialogueText;

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
    public void UpdateTurnInfo(int turnCount, bool isPlayerTurn)
    {
        turnCountText.text = "Turn: " + turnCount;
        
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
        StartCoroutine(DialogueRoutine(text, duration));
    }

    IEnumerator DialogueRoutine(string text, float duration)
    {
        dialogueText.text = text;

        // 페이드 인 (0 -> 1)
        float fadeTime = 0.5f;
        float time = 0;
        while(time < fadeTime)
        {
            time += Time.deltaTime;
            dialogueGroup.alpha = Mathf.Lerp(0, 1, time / fadeTime);
            yield return null;
        }
        dialogueGroup.alpha = 1;

        // 대사 유지
        yield return new WaitForSeconds(duration);

        // 페이드 아웃 (1 -> 0)
        time = 0;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            dialogueGroup.alpha = Mathf.Lerp(1, 0, time / fadeTime);
            yield return null;
        }
        dialogueGroup.alpha = 0;
    }
}