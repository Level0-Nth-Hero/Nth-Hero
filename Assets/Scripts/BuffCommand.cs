using System.Collections;
using UnityEngine;

public class BuffCommand : ICommand
{
    private PlayerMove _player;
    private int _amount;

    public BuffCommand(PlayerMove player, int amount)
    {
        _player = player;
        _amount = amount;
    }

    public IEnumerator Execute()
    {
        // 플레이어에게 힘 버프 부여
        _player.AddTempStrength(_amount);

        // 연출 시간 (버프 이펙트가 있다면 여기서 대기)
        yield return new WaitForSeconds(0.5f);
    }
}