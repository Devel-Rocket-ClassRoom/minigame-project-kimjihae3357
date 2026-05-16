using UnityEngine;

public enum CardType
{
    None,
    villager,
    Resource,
    Building,
    Enemy,
    Food,
    
}

[CreateAssetMenu(fileName = "CardData", menuName = "Card/CardData")]
public abstract class CardData : ScriptableObject
{
    public string cardName;
    public Sprite icon;
    public CardType cardType;

}
