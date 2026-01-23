using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost } // 전투 상태 열거형

public class BattleManager : MonoBehaviour 
{
    public static BattleManager Instance; // 싱글톤 인스턴스

    [Header("상태 및 연결")] 
    public BattleState state; // 현재 전투 상태
    public PlayerMove player; // 플레이어 스크립트
    public EnemyMove enemy; // 적 스크립트
    public Button endTurnButton; // 턴 종료 버튼

    // [수정 1] 턴 숫자를 세는 변수 추가
    public int globalTurnCount = 0; // 전체 턴 카운트

    private Queue<ICommand> commandQueue = new Queue<ICommand>(); // 커맨드 큐
    private bool isBusy = false; // 커맨드 처리 중인지 여부

    void Awake() { Instance = this; } // 싱글톤 초기화

    void Start() // 전투 시작
    {
        state = BattleState.Start; // 상태 초기화
        StartCoroutine(SetupBattle()); // 전투 설정 코루틴 시작
    }

    IEnumerator SetupBattle() // 전투 설정 코루틴
    {
        yield return new WaitForSeconds(1f); // 잠시 대기
        StartPlayerTurn(); // 플레이어 턴 시작
    }

    public void StartPlayerTurn() // 플레이어 턴 시작 함수
    {
        state = BattleState.PlayerTurn; // 상태를 플레이어 턴으로 변경
        
        // [수정 2] 턴 카운트 증가시키고 UI 갱신!
        globalTurnCount++; 
        UIManager.Instance.UpdateTurnInfo(globalTurnCount, true); // true = 플레이어 턴 색상

        endTurnButton.interactable = true; // 턴 종료 버튼 활성화
        
        player.RefillEnergy(); // 플레이어 에너지 채우기
        //DeckManager.Instance.DrawCard(5);
        if (globalTurnCount == 1)
        {
            DeckManager.Instance.DrawCard(5); // 첫 턴에는 5장 뽑기
        }
        else
        {
            DeckManager.Instance.DrawCard(1); // 이후 턴에는 1장 뽑기
        }
        
        Debug.Log($" 플레이어 턴 시작! (Turn: {globalTurnCount})"); // 디버그 메시지
    }

    public void OnEndTurnButton() // 턴 종료 버튼 클릭 시
    {
        if (state != BattleState.PlayerTurn) return; // 플레이어 턴이 아니면 무시
        if (isBusy) return; // 커맨드 처리 중이면 무시

        StartCoroutine(EnemyTurnProcess()); // 적 턴 처리 코루틴 시작
    }

    IEnumerator EnemyTurnProcess()// 적 턴 처리 코루틴
    {
        state = BattleState.EnemyTurn; // 상태를 적 턴으로 변경
        
        // [수정 3] 적 턴일 때 UI 갱신 (빨간색 텍스트로)
        UIManager.Instance.UpdateTurnInfo(globalTurnCount, false); // false = 적 턴 색상

        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        
        Debug.Log(" 적 턴 시작!");
        yield return new WaitForSeconds(1f); // 잠시 대기

        enemy.QueueEnemyTurn(); // 적의 행동 커맨드 큐에 등록
    }

    public void AddCommand(ICommand command) // 커맨드 추가 함수
    {
        commandQueue.Enqueue(command); // 커맨드 큐에 추가
        if (!isBusy) // 처리 중이 아니면
        {
            StartCoroutine(ProcessCommandRoutine()); // 커맨드 처리 코루틴 시작
        }
    }
    
    IEnumerator ProcessCommandRoutine() // 커맨드 처리 코루틴
    {
        isBusy = true; // 처리 중 상태로 설정
        while (commandQueue.Count > 0) // 큐에 커맨드가 남아있는 동안
        {       
            ICommand currentCommand = commandQueue.Dequeue(); // 커맨드 하나 꺼내기
            yield return StartCoroutine(currentCommand.Execute()); // 커맨드 실행 대기
        }
        isBusy = false; // 처리 완료 상태로 설정
    }
}