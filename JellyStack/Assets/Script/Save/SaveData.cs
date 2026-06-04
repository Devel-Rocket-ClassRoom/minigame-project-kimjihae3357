using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>JsonUtility 직렬화용 DTO. Dictionary·다형성 불가 → name + 평면 상태 구조 사용.</summary>
[Serializable]
public class GameSaveData
{
    public bool hasData;

    public int currentDay = 1;
    public float elapsedTime;

    public int weather;                 // (int)WeatherType
    public int weatherDaysRemaining;

    public List<StackSave> stacks = new List<StackSave>();
    public List<CoinPoketSave> coinPokets = new List<CoinPoketSave>();
}

[Serializable]
public class StackSave
{
    public Vector3 pos;
    public List<CardSave> cards = new List<CardSave>();
}

[Serializable]
public class CardSave
{
    public string dataName;     // CardData 에셋 이름

    // 카드별 가변 상태 (해당 없으면 무시)
    public int health;
    public int hunger;
    public int fullness;
    public int count;
    public int birthDay = -1;

    // 팩 카드 전용
    public bool isPack;
    public string packPrefabName;
    public string packDataName;
    public List<string> packRemaining = new List<string>();
}

[Serializable]
public class CoinPoketSave
{
    public string name;
    public int amount;
}
