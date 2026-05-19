using UnityEngine;

public class SourceCard : Card
{
    public int CurrentCount { get; private set;  }

    private SourceCardData sourceData => data as SourceCardData;

    private void Awake()
    {
        if (sourceData != null)
            CurrentCount = sourceData.GatherCount;

    }

    public void Gather()
    {
        CurrentCount--;
    }

    public bool IsExhausted => CurrentCount <= 0;
}
