using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tutorial_window prefab에 부착되는 UI 컨트롤러.
/// TutorialBook(ScriptableObject) 의 페이지 목록을 좌/우 버튼으로 넘기며 표시한다.
///
/// 페이지별로 갱신되는 슬롯:
///   - leftImage     (Left_Panel의 Image)            → page.mainImage
///   - titleText     (Text_info_title)               → page.title
///   - infoText      (Text_Box > Text_info)          → page.info
///   - tipInfoText   (Tip_Text_Box > Text_tip_info)  → page.tip
///   - tipBox        (Tip_Text_Box GameObject)       → page.tip 빈 문자열이면 비활성
///   - pageText      (Button > Button > Text_page)   → "현재/전체"
///   - pageDots[]    (Page > 1~5 의 Image들)         → 활성 페이지만 activeDotColor
///
/// 좌측/우측 페이지 버튼은 첫 페이지/마지막 페이지에서 자동으로 interactable=false.
/// </summary>
public class UI_TutorialWindow : MonoBehaviour
{
    [Header("튜토리얼 데이터")]
    [SerializeField] private TutorialBook book;

    [Header("이미지")]
    [Tooltip("Left_Panel의 Image 컴포넌트 (좌측 메인 비주얼).")]
    [SerializeField] private Image leftImage;

    [Header("텍스트")]
    [Tooltip("페이지 번호 표시 (\"1/5\" 형식).")]
    [SerializeField] private TMP_Text pageText;
    [Tooltip("Text_info_title — 페이지 제목.")]
    [SerializeField] private TMP_Text titleText;
    [Tooltip("Text_info — 본문 설명.")]
    [SerializeField] private TMP_Text infoText;
    [Tooltip("Text_tip_info — 페이지별로 바뀌는 팁 본문. 'TIP' 헤더와 장식 Image는 prefab 안에 고정.")]
    [SerializeField] private TMP_Text tipInfoText;
    [Tooltip("Tip_Text_Box GameObject — tip이 빈 문자열인 페이지에서 통째로 숨김.")]
    [SerializeField] private GameObject tipBox;

    [Header("페이지 점 (왼→오 순서)")]
    [Tooltip("Page > 1~5 의 Image 컴포넌트들. 페이지 수가 적으면 남는 점은 자동 숨김.")]
    [SerializeField] private Image[] pageDots;
    [SerializeField] private Color activeDotColor = new Color(0.54f, 0.62f, 0.36f, 1f);
    [SerializeField] private Color inactiveDotColor = new Color(0.86f, 0.77f, 0.59f, 1f);

    [Header("버튼")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    /// <summary>
    /// 튜토리얼이 닫힐 때(Close 버튼 또는 외부 Hide() 호출) 발화.
    /// UIManager 등 호출자가 게임 진행 재개(timeScale 복원)에 사용.
    /// </summary>
    public System.Action OnClosed;

    private int currentPage;

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    private void OnEnable()
    {
        currentPage = 0;
        UpdateUI();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        // OnEnable이 자동 호출되어 currentPage=0 + UpdateUI()
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }

    /// <summary>외부에서 다른 TutorialBook으로 바꿔 열고 싶을 때.</summary>
    public void SetBook(TutorialBook newBook)
    {
        book = newBook;
        currentPage = 0;
        if (gameObject.activeSelf) UpdateUI();
    }

    private void PrevPage()
    {
        if (book == null || book.pages == null) return;
        if (currentPage <= 0) return;
        currentPage--;
        UpdateUI();
    }

    private void NextPage()
    {
        if (book == null || book.pages == null) return;
        if (currentPage >= book.pages.Count - 1) return;
        currentPage++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 데이터 없으면 안전 출력만
        if (book == null || book.pages == null || book.pages.Count == 0)
        {
            if (pageText != null) pageText.text = "0/0";
            if (titleText != null) titleText.text = "";
            if (infoText != null) infoText.text = "";
            if (tipInfoText != null) tipInfoText.text = "";
            if (tipBox != null) tipBox.SetActive(false);
            if (prevButton != null) prevButton.interactable = false;
            if (nextButton != null) nextButton.interactable = false;
            if (pageDots != null)
            {
                for (int i = 0; i < pageDots.Length; i++)
                    if (pageDots[i] != null) pageDots[i].gameObject.SetActive(false);
            }
            return;
        }

        currentPage = Mathf.Clamp(currentPage, 0, book.pages.Count - 1);
        var page = book.pages[currentPage];

        // 텍스트
        if (pageText != null) pageText.text = $"{currentPage + 1}/{book.pages.Count}";
        if (titleText != null) titleText.text = page.title ?? "";
        if (infoText != null) infoText.text = page.info ?? "";

        // 팁: 빈 문자열이면 Tip_Text_Box 통째로 숨김 (안의 "TIP" 헤더/장식 Image까지 함께 사라짐)
        bool hasTip = !string.IsNullOrEmpty(page.tip);
        if (tipInfoText != null) tipInfoText.text = page.tip ?? "";
        if (tipBox != null) tipBox.SetActive(hasTip);

        // 좌측 메인 이미지
        if (leftImage != null && page.mainImage != null)
            leftImage.sprite = page.mainImage;

        // 페이지 점: 현재 페이지만 active 색, 나머지는 inactive 색, 페이지 수 초과 점은 숨김
        if (pageDots != null)
        {
            for (int i = 0; i < pageDots.Length; i++)
            {
                if (pageDots[i] == null) continue;
                bool inRange = i < book.pages.Count;
                pageDots[i].gameObject.SetActive(inRange);
                if (inRange)
                    pageDots[i].color = (i == currentPage) ? activeDotColor : inactiveDotColor;
            }
        }

        // 좌우 버튼 활성/비활성 (첫 페이지에선 prev, 마지막에선 next 비활성)
        if (prevButton != null) prevButton.interactable = currentPage > 0;
        if (nextButton != null) nextButton.interactable = currentPage < book.pages.Count - 1;
    }
}
