using UnityEngine;

[RequireComponent (typeof(Collider))]
public class Card : MonoBehaviour
{
    public CardData data;

    [HideInInspector] public CardStack stack;
}
