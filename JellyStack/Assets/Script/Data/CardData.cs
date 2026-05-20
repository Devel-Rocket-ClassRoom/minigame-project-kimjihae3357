using UnityEngine;

public enum CardType
{
    None,
    villager,
    Source,
    Resource,
    Building,
    Enemy,
    Food,
    
}

[CreateAssetMenu(fileName = "CardData", menuName = "Card/CardData")]
public abstract class CardData : ScriptableObject
{
    public CardType cardType;
    public GameObject cardPrefab;
    public string cardName;
    public Sprite Image;

}
