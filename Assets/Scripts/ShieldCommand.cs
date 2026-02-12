using UnityEngine;
using System.Collections;

public class ShieldCommand : ICommand
{
    private PlayerMove player;// 쉴드를 받을 플레이어
    private int shieldAmount;// 추가할 쉴드의 량

    // 생성자: 이 작업을 만들 때 플레이어 정보와 쉴드량을 미리 받아둔다.

    public ShieldCommand(PlayerMove player, int amount)
    {
        this.player = player;
        this.shieldAmount = amount;
    }
    // 커맨드 실행: 배틀 매니저가 이 작업을 실행할 때 호출
    public IEnumerator Execute()
    {
        if (player != null)
        {
            player.AddShield(shieldAmount); // 플레이어의 AddShield 메서드 실행
        }
        yield break; //대기 없이 종료
    }

}
