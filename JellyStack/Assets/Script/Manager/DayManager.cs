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
        OnDayChanged?.Invoke(CurrentDay);
    }
}
