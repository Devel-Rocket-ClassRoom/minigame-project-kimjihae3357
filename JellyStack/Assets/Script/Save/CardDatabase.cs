using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 이름 → 에셋 복원용 레지스트리. CardData는 Resources에 없어 런타임 로드가 불가하므로
/// 모든 CardData/CardPackData 참조를 모아두고 이름으로 조회한다.
/// 에셋은 Assets/Resources/CardDatabase.asset 에 두고 Resources.Load로 로드.
/// </summary>
[CreateAssetMenu(fileName = "CardDatabase", menuName = "Save/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> allCards = new List<CardData>();
    public List<CardPackData> allPacks = new List<CardPackData>();

    [Tooltip("팩 카드 프리팹들 (StarterCardPack, RandomPack_01 등). 이름으로 복원.")]
    public List<GameObject> packPrefabs = new List<GameObject>();

    [Tooltip("카드 스택 프리팹 (GameManager의 것과 동일).")]
    public GameObject cardStackPrefab;

    private Dictionary<string, CardData> _cardMap;
    private Dictionary<string, CardPackData> _packMap;
    private Dictionary<string, GameObject> _prefabMap;

    private void BuildMaps()
    {
        _cardMap = new Dictionary<string, CardData>();
        foreach (var c in allCards)
            if (c != null && !_cardMap.ContainsKey(c.name)) _cardMap.Add(c.name, c);

        _packMap = new Dictionary<string, CardPackData>();
        foreach (var p in allPacks)
            if (p != null && !_packMap.ContainsKey(p.name)) _packMap.Add(p.name, p);

        _prefabMap = new Dictionary<string, GameObject>();
        foreach (var g in packPrefabs)
            if (g != null && !_prefabMap.ContainsKey(g.name)) _prefabMap.Add(g.name, g);
    }

    public CardData GetCard(string n)
    {
        if (_cardMap == null) BuildMaps();
        return (n != null && _cardMap.TryGetValue(n, out var c)) ? c : null;
    }

    public CardPackData GetPack(string n)
    {
        if (_packMap == null) BuildMaps();
        return (n != null && _packMap.TryGetValue(n, out var p)) ? p : null;
    }

    public GameObject GetPackPrefab(string n)
    {
        if (_prefabMap == null) BuildMaps();
        return (n != null && _prefabMap.TryGetValue(n, out var g)) ? g : null;
    }

#if UNITY_EDITOR
    [ContextMenu("Populate From Assets/Data")]
    private void Populate()
    {
        allCards.Clear();
        allPacks.Clear();

        foreach (var guid in AssetDatabase.FindAssets("t:CardData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (asset != null) allCards.Add(asset);
        }
        foreach (var guid in AssetDatabase.FindAssets("t:CardPackData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<CardPackData>(path);
            if (asset != null) allPacks.Add(asset);
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[CardDatabase] Populated: {allCards.Count} cards, {allPacks.Count} packs. (packPrefabs/cardStackPrefab은 수동 연결)");
    }
#endif
}
