using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카드팩 데이터의 베이스. 모든 팩 타입(StarterPack, RandomCardPack 등)은
/// BuildInitialList()를 override해서 팩을 열 때 나올 카드 목록을 결정한다.
/// </summary>
public abstract class CardPackData : ScriptableObject
{
    public string cardName;
    public Sprite Image;

    /// <summary>팩을 열 때 호출. 한 번에 모든 카드를 결정해 리스트로 반환.</summary>
    public abstract List<CardData> BuildInitialList();
}
