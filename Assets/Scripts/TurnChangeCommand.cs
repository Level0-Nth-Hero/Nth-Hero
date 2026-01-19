using System.Collections;
using UnityEngine;

public class TurnChangeCommand : ICommand
{
    // 누가 턴을 받을지, 혹은 다음 상태가 무엇인지 저장
    private BattleState _nextState;

    public TurnChangeCommand(BattleState nextState)
    {
        _nextState = nextState;
    }

    public IEnumerator Execute()
    {
        Debug.Log("🔄 턴 변경 중...");
        
        // 1. 잠시 뜸 들이기 (바로 바뀌면 정신없으니까)
        yield return new WaitForSeconds(0.5f);

        // 2. 다음 상태가 플레이어 턴이라면 -> 플레이어 턴 시작 함수 호출
        if (_nextState == BattleState.PlayerTurn)
        {
            BattleManager.Instance.StartPlayerTurn();
        }
    }
}