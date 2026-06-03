using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [SerializeField] private float dayDuration = 120f;

    public int CurrentDay { get; private set; } = 1;
    public float ElapsedTime { get; private set; } = 0f;

    public float DayProgress => ElapsedTime / dayDuration;

    public System.Action<int> OnDayChanged;
    public System.Action<System.Action> OnBeforeDayChanged;

    // FeedPhase 이후, ContinueDayChange 이전에 정산(카드 한도 체크 등)을 끼워넣을 훅.
    // 구독자가 받은 콜백을 호출해야 다음 단계(ContinueDayChange)로 진행한다.
    public System.Action<System.Action> OnBeforeSettlement;

    private bool _pendingDayChange;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (_pendingDayChange) return;

        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= dayDuration)
        {
            ElapsedTime -= dayDuration;
            _pendingDayChange = true;

            // 콜백 체인: FeedPhase 종료 → SettlementPhase → ContinueDayChange
            System.Action afterSettlement = ContinueDayChange;
            System.Action afterFeed = () =>
            {
                if (OnBeforeSettlement != null) OnBeforeSettlement.Invoke(afterSettlement);
                else afterSettlement();
            };

            if (OnBeforeDayChanged != null)
                OnBeforeDayChanged.Invoke(afterFeed);
            else
                afterFeed();
        }
    }

    public void ContinueDayChange()
    {
        _pendingDayChange = false;
        CurrentDay++;

        // 2일차 이후 주민 카드가 한 장도 없으면 게임오버
        if (CurrentDay >= 2)
        {
            var villagers = UnityEngine.Object.FindObjectsByType<VillagerCard>(FindObjectsSortMode.None);
            if (villagers.Length == 0)
            {
                GameManager.Instance?.GameOver();
                return;
            }
        }

        OnDayChanged?.Invoke(CurrentDay);
    }
}
