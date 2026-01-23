using System.Collections;
using UnityEngine;

public class TurnChangeCommand : ICommand
{
    // 누가 턴을 받을지, 혹은 다음 상태가 무엇인지 저장
    private BattleState _nextState; // 다음 전투 상태

    public TurnChangeCommand(BattleState nextState) // 생성자
    {
        _nextState = nextState; // 다음 상태 설정
    }

    public IEnumerator Execute() // 커맨드 실행
    {
        Debug.Log("🔄 턴 변경 중...");
        
        // 1. 잠시 뜸 들이기 (바로 바뀌면 정신없으니까)
        yield return new WaitForSeconds(0.5f);

        // 2. 다음 상태가 플레이어 턴이라면 -> 플레이어 턴 시작 함수 호출
        if (_nextState == BattleState.PlayerTurn)
        {
            BattleManager.Instance.StartPlayerTurn(); // 플레이어 턴 시작
        }
    }
}