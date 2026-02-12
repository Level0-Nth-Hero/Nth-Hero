using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 전환(다시 시작) 기능을 위해 필요
using System.Collections;

public class UI_Defeat : MonoBehaviour 
{
    //  싱글톤: 어디서든 UI_Defeat.Instance로 접근하게 해주는 주소록
    public static UI_Defeat Instance; 

    [Header("UI Components")]
    public CanvasGroup canvasGroup; // 투명도(Alpha)와 클릭 차단을 한꺼번에 조절

    [Header("Buttons")]
    public Button retryButton; // 다시 시작 버튼
    public Button homeButton;  // 홈으로 이동 버튼

    void Awake()
    {
        if (Instance == null) Instance = this;

        // 버튼들에게 "클릭하면 이 함수를 실행해!"라고 명령을 미리 내립니다.
        if (retryButton != null)
            retryButton.onClick.AddListener(RestartGame);
        
        if (homeButton != null)
            homeButton.onClick.AddListener(GoToHome);

        canvasGroup.alpha = 0;              // 투명하게 만들기
        canvasGroup.blocksRaycasts = false; // 마우스 클릭이 패널을 뚫고 지나가게 하기
        canvasGroup.interactable = false;   // 버튼 상호작용 끄기
    }

    public void Show()
    {
        gameObject.SetActive(true);    // 혹시 꺼져있다면 오브젝트를 켭니다.
        StartCoroutine(FadeInRoutine()); // 서서히 나타나는 연출(코루틴) 시작
    }

    // 시간을 나눠서 UI를 부드럽게 띄우는 코루틴 연출
    IEnumerator FadeInRoutine()
    {
        float duration = 1.0f; // 1초 동안 연출 진행
        float time = 0;

        // 연출이 시작되는 순간 마우스 클릭을 막고 버튼을 활성화
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        while (time < duration)
        {
            time += Time.deltaTime; // 프레임마다 흐른 시간을 더함
            // DefeatPanel의 투명도를 0에서 1로 서서히 변경
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1; // 연출 종료 후 상태 고정
    }

    // [다시 시작] 버튼
    private void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    
    // [홈 이동] 버튼: 지금은 로직이 없으니 콘솔창에 기록만 남김
    private void GoToHome() => Debug.Log("홈으로 이동 버튼 클릭됨");
}