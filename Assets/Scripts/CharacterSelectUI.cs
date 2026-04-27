using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    public AutoAttack  autoAttack;
    public SwordAttack swordAttack;
    public Sprite      characterSprite;

    private bool selected;
    private bool showingTitle = true;

    void Awake()
    {
        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        if (selected) return;

        // 어두운 전체 배경
        GUI.color = new Color(0f, 0f, 0f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        if (showingTitle) { DrawTitleScreen(); return; }

        // 타이틀
        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(0, Screen.height * 0.08f, Screen.width, 50f),
            "캐릭터를 선택하세요", titleStyle);

        float cardW  = 240f;
        float cardH  = 380f;
        float gap    = 60f;
        float totalW = cardW * 2 + gap;
        float startX = (Screen.width - totalW) / 2f;
        float cardY  = Screen.height * 0.2f;

        // ── 캐릭터 1 카드 ──
        DrawCard(startX, cardY, cardW, cardH,
            "캐릭터 1",
            "원거리 공격",
            "달팽이를 투척하여\n적을 원거리에서 처치!\n\n공격 범위: 넓음\n공격 방식: 투사체",
            new Color(0.15f, 0.35f, 0.6f, 1f));

        GUI.color = new Color(0.25f, 0.6f, 0.95f, 1f);
        if (GUI.Button(new Rect(startX + 20f, cardY + cardH - 60f, cardW - 40f, 44f), "선택"))
            SelectCharacter(1);

        // ── 캐릭터 2 카드 ──
        float cx2 = startX + cardW + gap;
        DrawCard(cx2, cardY, cardW, cardH,
            "캐릭터 2",
            "근거리 공격",
            "검을 휘둘러\n인접한 적을 베어라!\n\n공격 범위: 근접\n공격 방식: 광역 참격",
            new Color(0.45f, 0.15f, 0.15f, 1f));

        GUI.color = new Color(0.95f, 0.35f, 0.25f, 1f);
        if (GUI.Button(new Rect(cx2 + 20f, cardY + cardH - 60f, cardW - 40f, 44f), "선택"))
            SelectCharacter(2);

        GUI.color = Color.white;
    }

    void DrawCard(float x, float y, float w, float h, string charName, string attackType, string desc, Color bgColor)
    {
        // 카드 배경
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

        // 테두리
        GUI.color = new Color(1f, 1f, 1f, 0.25f);
        GUI.DrawTexture(new Rect(x,         y,     w,  2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x,         y+h-2, w,  2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x,         y,     2f, h),  Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + w - 2, y,     2f, h),  Texture2D.whiteTexture);

        var nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        var subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        var descStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 12,
            alignment = TextAnchor.UpperLeft,
            wordWrap  = true
        };

        // 캐릭터 이름
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y + 14f, w, 30f), charName, nameStyle);

        // 캐릭터 이미지 (이름 아래 중앙 정렬)
        if (characterSprite != null)
        {
            float imgSize = 110f;
            float imgX    = x + (w - imgSize) / 2f;
            float imgY    = y + 50f;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(imgX, imgY, imgSize, imgSize),
                characterSprite.texture, ScaleMode.ScaleToFit, true);
        }

        // 공격 타입
        GUI.color = new Color(1f, 0.85f, 0.3f, 1f);
        GUI.Label(new Rect(x, y + 168f, w, 24f), attackType, subStyle);

        // 설명
        GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        GUI.Label(new Rect(x + 16f, y + 198f, w - 32f, 100f), desc, descStyle);
    }

    void DrawTitleScreen()
    {
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 52,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        var subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            alignment = TextAnchor.MiddleCenter
        };

        // 게임 타이틀
        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 70f),
            "Maple Survivor", titleStyle);

        // 부제
        GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        GUI.Label(new Rect(0, Screen.height * 0.3f + 72f, Screen.width, 30f),
            "메이플스토리 세계에서 살아남아라!", subStyle);

        // 시작 버튼
        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Bold
        };
        float btnW = 200f, btnH = 54f;
        float btnX = (Screen.width  - btnW) / 2f;
        float btnY = Screen.height * 0.72f;

        GUI.color = new Color(0.25f, 0.7f, 0.3f, 1f);
        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "시작", btnStyle))
            showingTitle = false;

        GUI.color = Color.white;
    }

    void SelectCharacter(int id)
    {
        if (autoAttack  != null) autoAttack.enabled  = (id == 1);
        if (swordAttack != null) swordAttack.enabled = (id == 2);

        selected = true;
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}
