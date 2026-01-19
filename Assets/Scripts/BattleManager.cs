using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("상태 및 연결")]
    public BattleState state;
    public PlayerMove player;
    public EnemyMove enemy;
    public Button endTurnButton;

    // [수정 1] 턴 숫자를 세는 변수 추가
    public int globalTurnCount = 0; 

    private Queue<ICommand> commandQueue = new Queue<ICommand>();
    private bool isBusy = false;

    void Awake() { Instance = this; }

    void Start()
    {
        state = BattleState.Start;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        yield return new WaitForSeconds(1f);
        StartPlayerTurn(); 
    }

    public void StartPlayerTurn()
    {
        state = BattleState.PlayerTurn;
        
        // [수정 2] 턴 카운트 증가시키고 UI 갱신!
        globalTurnCount++; 
        UIManager.Instance.UpdateTurnInfo(globalTurnCount, true); // true = 플레이어 턴 색상

        endTurnButton.interactable = true; 
        
        player.RefillEnergy();
        //DeckManager.Instance.DrawCard(5);
        if (globalTurnCount == 1)
        {
            DeckManager.Instance.DrawCard(5);
        }
        else
        {
            DeckManager.Instance.DrawCard(1);
        }
        
        Debug.Log($" 플레이어 턴 시작! (Turn: {globalTurnCount})");
    }

    public void OnEndTurnButton()
    {
        if (state != BattleState.PlayerTurn) return;
        if (isBusy) return; 

        StartCoroutine(EnemyTurnProcess());
    }

    IEnumerator EnemyTurnProcess()
    {
        state = BattleState.EnemyTurn;
        
        // [수정 3] 적 턴일 때 UI 갱신 (빨간색 텍스트로)
        UIManager.Instance.UpdateTurnInfo(globalTurnCount, false); // false = 적 턴 색상

        endTurnButton.interactable = false; 
        
        Debug.Log(" 적 턴 시작!");
        yield return new WaitForSeconds(1f); 

        enemy.QueueEnemyTurn();
    }

    public void AddCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
        if (!isBusy)
        {
            StartCoroutine(ProcessCommandRoutine());
        }
    }
    
    IEnumerator ProcessCommandRoutine()
    {
        isBusy = true;
        while (commandQueue.Count > 0)
        {
            ICommand currentCommand = commandQueue.Dequeue();
            yield return StartCoroutine(currentCommand.Execute());
        }
        isBusy = false;
    }
}