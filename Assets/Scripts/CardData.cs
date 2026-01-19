using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public int value;
    public Sprite icon;

    public CardType cardType;

    [TextArea] // 에디터에서 글 쓰는 칸이 넓어지는 기능
    public string description;
}
public enum CardType { Attack, Skill, Power }
