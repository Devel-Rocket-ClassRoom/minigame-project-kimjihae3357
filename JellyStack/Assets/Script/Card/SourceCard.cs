using UnityEngine;

public class SourceCard : Card
{
    public int CurrentCount { get; private set;  }

    private SourceCardData sourceData => data as SourceCardData;

    private void Awake()
    {
        CurrentCount = sourceData.GatherCount;
    }
}
