using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ingame 씬에 배치. 현재 게임 상태를 캡처해 저장하고, LoadRequested 시 복원한다.
/// 복원은 한 프레임 뒤(코루틴)에 실행해 모든 매니저의 Awake/Start가 끝난 뒤 덮어쓴다.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Tooltip("비워두면 Resources/CardDatabase 를 자동 로드.")]
    [SerializeField] private CardDatabase database;

    private void Awake()
    {
        Instance = this;
        if (database == null) database = Resources.Load<CardDatabase>("CardDatabase");
    }

    private void Start()
    {
        if (SaveSystem.LoadRequested)
            StartCoroutine(RestoreRoutine());
    }

    // ─────────────────────────────── 저장 ───────────────────────────────

    public void Save()
    {
        SaveSystem.Write(Capture());
    }

    private GameSaveData Capture()
    {
        var data = new GameSaveData();

        if (DayManager.Instance != null)
        {
            data.currentDay = DayManager.Instance.CurrentDay;
            data.elapsedTime = DayManager.Instance.ElapsedTime;
        }

        if (WeatherManager.Instance != null)
        {
            data.weather = (int)WeatherManager.Instance.CurrentWeather;
            data.weatherDaysRemaining = WeatherManager.Instance.WeatherDaysRemaining;
        }

        foreach (var stack in Object.FindObjectsByType<CardStack>(FindObjectsSortMode.None))
        {
            if (stack == null || stack.IsEmpty) continue;
            if (stack is BattlePoint) continue;   // 전투 중 스택은 저장 제외

            var ss = new StackSave { pos = stack.transform.position };
            foreach (var card in stack.cards)
            {
                if (card == null) continue;
                ss.cards.Add(CaptureCard(card));
            }
            if (ss.cards.Count > 0) data.stacks.Add(ss);
        }

        foreach (var poket in Object.FindObjectsByType<CoinPoket>(FindObjectsSortMode.None))
        {
            if (poket == null) continue;
            data.coinPokets.Add(new CoinPoketSave { name = poket.name, amount = poket.CoinAmount });
        }

        return data;
    }

    private CardSave CaptureCard(Card card)
    {
        var cs = new CardSave();

        if (card is PackCard pc)
        {
            cs.isPack = true;
            cs.packPrefabName = StripClone(pc.gameObject.name);
            cs.packDataName = pc.PackDataName;
            cs.packRemaining = new List<string>();
            foreach (var d in pc.GetRemaining())
                if (d != null) cs.packRemaining.Add(d.name);
            return cs;
        }

        cs.dataName = card.data != null ? card.data.name : "";

        if (card is VillagerCard vc)
        {
            cs.health = vc.CurrentHealth;
            cs.hunger = vc.Currenthunger;
            cs.birthDay = vc.BirthDay;
        }
        else if (card is EnemyCard ec)
        {
            cs.health = ec.CurrentHealth;
        }
        else if (card is FoodCard fc)
        {
            cs.fullness = fc.CurrentFullness;
        }
        else if (card is SourceCard sc)
        {
            cs.count = sc.CurrentCount;
        }

        return cs;
    }

    private static string StripClone(string n)
        => n.Replace("(Clone)", "").Trim();

    // ─────────────────────────────── 복원 ───────────────────────────────

    private IEnumerator RestoreRoutine()
    {
        // 모든 매니저의 Awake/Start(예: WeatherManager.ClearWeather)가 끝난 뒤 덮어쓰기
        yield return null;

        // 안전망: 이전 씬(Title/Pause/GameOver)에서 넘어온 비정상 상태 강제 정상화.
        // 특히 Time.timeScale은 씬 로드 시 자동 리셋되지 않는 전역 변수이므로, 직전 세션에서
        // pause/gameover 상태(timeScale=0)로 메인 메뉴로 나갔다면 이어하기 직후에도 0이 유지됨.
        // ESC → Resume Game이 사후에 처리해주던 timeScale 복원을 처음부터 명시적으로 보장.
        Time.timeScale = 1f;
        InputManager.IsBlocked = false;
        ProgressTask.IsPaused = false;
        EnemyManager.IsPaused = false;

        var data = SaveSystem.Read();
        if (data == null || !data.hasData) yield break;
        if (database == null)
        {
            Debug.LogError("[Save] CardDatabase가 없어 복원 불가. Resources/CardDatabase.asset 확인.");
            yield break;
        }

        if (DayManager.Instance != null)
            DayManager.Instance.LoadDay(data.currentDay, data.elapsedTime);
        if (UI_Ingame.Instance != null)
            UI_Ingame.Instance.RefreshDayText();

        if (WeatherManager.Instance != null)
            WeatherManager.Instance.LoadWeather((WeatherType)data.weather, data.weatherDaysRemaining);

        foreach (var ss in data.stacks)
            RestoreStack(ss);

        // CoinPoket 복원 (이름 매칭)
        var pokets = Object.FindObjectsByType<CoinPoket>(FindObjectsSortMode.None);
        foreach (var save in data.coinPokets)
        {
            foreach (var poket in pokets)
            {
                if (poket != null && poket.name == save.name)
                {
                    poket.LoadCoinAmount(save.amount);
                    break;
                }
            }
        }
    }

    private void RestoreStack(StackSave ss)
    {
        if (database.cardStackPrefab == null)
        {
            Debug.LogError("[Save] CardDatabase.cardStackPrefab 미설정.");
            return;
        }

        var stackGo = Instantiate(database.cardStackPrefab, ss.pos, Quaternion.identity);
        var stack = stackGo.GetComponent<CardStack>();
        if (stack == null) { Destroy(stackGo); return; }

        foreach (var cs in ss.cards)
        {
            Card card = cs.isPack ? RestorePackCard(cs) : RestoreNormalCard(cs);
            if (card != null) stack.AddCard(card);
        }

        // 카드 targetLocalPosition을 ArrangeCards로 갱신 (CheckStack 전에 먼저!)
        stack.Refresh();

        // Card.Update의 Lerp가 한 프레임 후에야 transform을 따라잡기 때문에,
        // 복원 직후 카드 transform.position이 아직 targetLocalPosition에 도달 안 한 상태.
        // 그대로 CheckStack을 호출하면 ProgressTask가 stack.TopCard.transform.position을
        // 잘못된 위치(원점 등)로 읽어서 채집 effect가 엉뚱한 자리에 박힘.
        // → 강제로 transform.localPosition을 targetLocalPosition으로 즉시 동기화.
        foreach (var card in stack.cards)
        {
            if (card != null)
                card.transform.localPosition = card.targetLocalPosition;
        }

        // 채집/제작 패턴이면 작업 자동 재시작 (진행도는 0부터). 위치 동기화 이후 호출이 핵심.
        if (RecipeManager.Instance != null && !stack.IsEmpty)
            RecipeManager.Instance.CheckStack(stack);
    }

    private Card RestoreNormalCard(CardSave cs)
    {
        var cardData = database.GetCard(cs.dataName);
        if (cardData == null || cardData.cardPrefab == null)
        {
            Debug.LogWarning($"[Save] 카드 복원 실패: '{cs.dataName}' (CardDatabase에 없음)");
            return null;
        }

        var go = Instantiate(cardData.cardPrefab);
        var card = go.GetComponent<Card>();
        if (card == null) { Destroy(go); return null; }

        card.data = cardData;
        card.InitializeFromData();   // 최대치로 초기화 후 저장값으로 덮어쓰기

        if (card is VillagerCard vc) vc.LoadState(cs.health, cs.hunger, cs.birthDay);
        else if (card is EnemyCard ec) ec.LoadState(cs.health);
        else if (card is FoodCard fc) fc.LoadState(cs.fullness);
        else if (card is SourceCard sc) sc.LoadState(cs.count);

        return card;
    }

    private Card RestorePackCard(CardSave cs)
    {
        var prefab = database.GetPackPrefab(cs.packPrefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[Save] 팩 프리팹 복원 실패: '{cs.packPrefabName}' (CardDatabase.packPrefabs에 없음)");
            return null;
        }

        var go = Instantiate(prefab);
        var pc = go.GetComponent<PackCard>();
        if (pc == null) { Destroy(go); return null; }

        var packData = database.GetPack(cs.packDataName);
        if (packData != null) pc.SetPackData(packData);

        var remaining = new List<CardData>();
        foreach (var n in cs.packRemaining)
        {
            var d = database.GetCard(n);
            if (d != null) remaining.Add(d);
        }
        pc.LoadRemaining(remaining);

        return pc;
    }
}
