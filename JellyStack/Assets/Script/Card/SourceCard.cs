using UnityEngine;

public class SourceCard : Card
{
    public int CurrentCount { get; private set;  }

    private SourceCardData sourceData => data as SourceCardData;

    private void Awake()
    {
        InitializeFromData();
    }

    public override void InitializeFromData()
    {
        if (sourceData != null)
            CurrentCount = sourceData.GatherCount;
    }

    public void Gather()
    {
        CurrentCount--;
        OnStatChanged?.Invoke();
    }

    /// <summary>세이브 복원용 — 저장된 채집 횟수로 덮어쓰기.</summary>
    public void LoadState(int count)
    {
        CurrentCount = count;
        OnStatChanged?.Invoke();
    }

    public bool IsExhausted => CurrentCount <= 0;
}
