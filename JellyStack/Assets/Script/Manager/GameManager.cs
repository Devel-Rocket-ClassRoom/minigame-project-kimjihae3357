using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject starterPackPrefab;
    [SerializeField] private GameObject cardStackPrefab;
    [SerializeField] private Vector3 packSpawnPosition = Vector3.zero;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnStarterPack();
    }

    private void SpawnStarterPack()
    {
        var packGo = Instantiate(starterPackPrefab, packSpawnPosition, Quaternion.identity);
        var packCard = packGo.GetComponent<PackCard>();
        if (packCard == null) return;

        var stackGo = Instantiate(cardStackPrefab, packSpawnPosition, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();
        stack.AddCard(packCard);
    }
}
