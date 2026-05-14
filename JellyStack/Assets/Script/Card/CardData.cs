using UnityEngine;

public enum CardType
{
    villager,
    Resource,
    Building,
    Enemy,
    Food,
}

[CreateAssetMenu(fileName = "CardData", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite icon;
    public CardType cardType;
    public string description;

}
