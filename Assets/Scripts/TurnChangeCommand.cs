using System.Collections;
using UnityEngine;

public class TurnChangeCommand : ICommand
{
    private PlayerMove _player;
    private int _newTurnCount;

    public TurnChangeCommand(PlayerMove player, int newTurnCount)
    {
        _player = player;
        _newTurnCount = newTurnCount;
    }

    public IEnumerator Execute()
    {
        // 턴 넘김 처리
        //_player.turncount = 0; // 플레이어 행동권 복구
        //_player.globalTurnCount = _newTurnCount;

        _player.RestTurn(_newTurnCount);
        
        // UI 갱신
        UIManager.Instance.UpdateTurnInfo(_newTurnCount, true);
        
        Debug.Log("턴 변경 완료: Player Turn");
        yield return null;
    }
}