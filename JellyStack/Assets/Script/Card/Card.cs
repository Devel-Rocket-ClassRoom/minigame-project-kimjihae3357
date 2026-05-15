using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Card : MonoBehaviour
{
    public CardData data;
    [HideInInspector] public CardStack stack;

    [HideInInspector] public Vector3 targetLocalPosition;
    [HideInInspector] public float followSpeed = 10f;

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, targetLocalPosition, Time.deltaTime * followSpeed);
    }
}
