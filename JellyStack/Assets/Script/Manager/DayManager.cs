using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [SerializeField] private float dayDuration = 120f;

    public int CurrentDay { get; private set; } = 1;
    public float ElapsedTime { get; private set; } = 0f;

    public System.Action<int> OnDayChanged;

    private void Awake() => Instance = this;

    private void Update()
    {
        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= dayDuration)
        {
            ElapsedTime -= dayDuration;
            CurrentDay++;
            OnDayChanged?.Invoke(CurrentDay);
        }
    }
}
