using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("[시작 아이템]")]
    [SerializeField] private GameObject starterPackPrefab;
    [SerializeField] private GameObject cardStackPrefab;

    private Vector3 startSpawnPosition = Vector3.zero;

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
        var packGo = Instantiate(starterPackPrefab, startSpawnPosition, Quaternion.identity);
        var packCard = packGo.GetComponent<PackCard>();
        if (packCard == null) return;

        var stackGo = Instantiate(cardStackPrefab, startSpawnPosition, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();
        stack.AddCard(packCard);
    }
}
