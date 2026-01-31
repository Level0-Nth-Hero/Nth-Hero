using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card Data")] // 에디터에서 우클릭으로 생성 가능하게 함
public class CardData : ScriptableObject // 카드의 데이터들을 담는 스크립터블 오브젝트
/// ScriptableObject는 Unity에서 데이터 중심의 객체를 만들기 위한 특별한 클래스입니다. 쓰는 이유는 다음과 같습니다:
/// 1. 데이터 분리: 게임 오브젝트와 컴포넌트에서 데이터를 분리하여 관리할 수 있습니다. 이렇게 하면 데이터 변경이 게임 오브젝트에 직접적인 영향을 미치지 않아 유지보수가 쉬워집니다.
/// 2. 재사용성: ScriptableObject는 여러 게임 오브젝트에서 공유 할 수 있습니다. 예를 들어, 여러 카드가 동일한 카드 데이터를 참조할 수 있어 메모리 사용을 줄이고 일관성을 유지할 수 있습니다.
/// 3. 에디터 통합: Unity 에디터 내에서 쉽게 생성, 편집 및 관리할 수 있습니다. 개발자는 에디터에서 직접 데이터를 수정할 수 있어 작업 효율이 향상됩니다.
/// 4. 직렬화: ScriptableObject는 Unity의 직렬화 시스템과 잘 통합되어 있어 저장 및 로드가 용이합니다. 이는 게임 상태를 저장하거나 데이터를 외부 파일로 내보내는 데 유용합니다.
/// 5. 성능 최적화: ScriptableObject는 메모리 사용을 최적화하고 런타임 성능을 향상시킬 수 있습니다. 특히 많은 양의 데이터를 다룰 때 유리합니다.
/// 종합적으로, ScriptableObject는 데이터 중심의 설계를 촉진하고, 코드와 데이터를 분리하며, Unity 프로젝트의 유지보수성과 확장성을 향상시키는 데 중요한 역할을 합니다.
{
    public string cardName; // 카드 이름
    public int cost; // 카드 비용
    public int value; // 카드 효과 수치
    public Sprite icon; // 카드 아이콘 이미지

    public CardType cardType; // 카드 종류 (공격, 스킬, 파워 등)

    [TextArea] // 에디터에서 글 쓰는 칸이 넓어지는 기능
    public string description; // 카드 설명
    public bool isExhaust; // 카드 사용 후 소멸 여부
}
public enum CardType { Attack, Skill, Power } // 카드 종류 열거형
