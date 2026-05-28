/// <summary>
/// 날씨 타입. 룰렛 휠 이미지의 시계방향 배치와 인덱스가 일치해야 한다.
/// 휠 이미지: 12시=Sunny(0), 3시=Rain(1), 6시=Snow(2), 9시=Storm(3).
/// 만약 휠 이미지 배치를 바꾸면 이 enum 순서도 같이 맞춰야 함.
/// </summary>
public enum WeatherType
{
    Sunny,   // 0 - 12시 (맑음)
    Rain,    // 1 - 3시  (비)
    Snow,    // 2 - 6시  (눈)
    Storm    // 3 - 9시  (폭풍)
}
