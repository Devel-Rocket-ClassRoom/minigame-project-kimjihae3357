using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [SerializeField] private float dayDuration = 120f;

    public int CurrentDay { get; private set; } = 1;
    public float ElapsedTime { get; private set; } = 0f;

    public System.Action<int> OnDayChanged;
    public System.Action<System.Action> OnBeforeDayChanged;

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

            if (OnBeforeDayChanged != null)
                OnBeforeDayChanged.Invoke(ContinueDayChange);
            else
                ContinueDayChange();
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
