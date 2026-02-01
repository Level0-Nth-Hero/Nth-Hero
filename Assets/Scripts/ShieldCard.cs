using UnityEngine;

public class ShieldCard : MonoBehaviour
{
    // 유니티 인스펙터에서 PlayerMove를 드래그해서 넣어줄 칸
    public PlayerMove playerMove;

    // 카드를 클릭했을 때 실행될 함수
    public void Use(CardData data)
    {
        if (data == null || playerMove == null) return;

        // 코스트 충분한지 확인
        if (playerMove.currentEnergy >= data.cost)
        {
            //실드 작동!
            ICommand cmd = new ShieldCommand(playerMove, 5);
            BattleManager.Instance.AddCommand(cmd);

            Debug.Log($"<color=blue>[ShieldCard]</color> {data.cardName} 사용! 쉴드 {data.value} 생성");
        }
    }
}