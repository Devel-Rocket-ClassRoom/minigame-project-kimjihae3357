using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 한 페이지에 표시될 데이터.
/// Tutorial_window prefab의 슬롯과 1:1 대응:
///   - title       → Text_info_title
///   - info        → Text_Box > Text_info
///   - tip         → Tip_Text_Box > Text_tip_info (빈 문자열이면 Tip_Text_Box 통째로 숨김)
///
/// 좌측 비주얼은 페이지별 스프라이트 애니메이션을 지원하기 위해 데이터가 아니라
/// UI_TutorialWindow의 leftContainer 자식 GameObject로 관리한다 (자식 순서 = 페이지 인덱스).
/// (Image_Line, Tip_Text_Box 안의 "TIP" 헤더/장식 Image는 고정 요소이므로 데이터에 없음.)
/// </summary>
[System.Serializable]
public class TutorialPage
{
    [Tooltip("페이지 제목 (Text_info_title에 표시).")]
    public string title;

    [Tooltip("본문 설명 (Text_info에 표시).")]
    [TextArea(3, 10)]
    public string info;

    [Tooltip("팁 박스 본문 (Text_tip_info에 표시). 비워두면 Tip_Text_Box 자체가 자동 숨김.")]
    [TextArea(2, 6)]
    public string tip;
}

/// <summary>
/// 한 튜토리얼 묶음. 페이지 List를 순서대로 표시.
/// 향후 다른 튜토리얼(첫 플레이 / 카드팩 설명 등)은 다른 TutorialBook 에셋만 만들면 됨.
/// </summary>
[CreateAssetMenu(fileName = "TutorialBook", menuName = "Tutorial/TutorialBook")]
public class TutorialBook : ScriptableObject
{
    [Tooltip("순서대로 표시될 튜토리얼 페이지 목록.")]
    public List<TutorialPage> pages = new List<TutorialPage>();
}
