using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class CharSpriteSet
    {
        public Sprite[] rightSprites;
        public Sprite[] leftSprites; // 비어있으면 flipX로 처리
    }

    public AutoAttack         autoAttack;
    public SwordAttack        swordAttack;
    public IceOrbAttack       iceOrbAttack;
    public ThrowingStarAttack throwingStarAttack;
    public CannonAttack       cannonAttack;
    public BowAttack          bowAttack;
    public Sprite[]           characterSprites = new Sprite[6];
    public CharSpriteSet[]    charSpriteSets   = new CharSpriteSet[6];

    private bool selected;
    private bool showingTitle = true;
    private int  selectedChar = 1;

    private static readonly string[] CharNames = {
        "초보자", "히어로", "썬콜", "나이트로드", "캐논슈터", "보우마스터"
    };
    private static readonly string[] CharTypes = {
        "원거리 공격", "근접 공격", "마법 공격", "암살자", "포격", "원거리 공격"
    };
    private static readonly string[] CharDescs = {
        "달팽이를 투척하여\n적을 원거리에서 처치!\n\n공격 방식: 투사체\n공격 범위: 넓음",
        "검을 휘둘러\n인접한 적을 베어라!\n\n공격 방식: 광역 참격\n공격 범위: 근접 부채꼴",
        "가장 가까운 적을 향해\n얼음 오브를 천천히 발사!\n날아가는 동안 주변 적에게\n지속적으로 냉기 데미지를 입힌다.\n\n공격 방식: 오브 투사체\n공격 범위: 오브 주변",
        "가장 가까운 적을 향해\n표창을 2발 연속 투척!\n주기적으로 빠르게 반복한다.\n\n공격 방식: 연속 투사체\n공격 범위: 원거리",
        "주변 랜덤 위치에 대포를 발사!\n착탄 시 폭발 데미지 후\n일정 시간 화염 지역을 생성.\n화염 지역 위 적에게 도트 데미지.\n\n공격 방식: 포물선 포격\n공격 범위: 광역",
        "바라보는 방향으로\n화살을 단발 발사!\n빠른 속도로 적을 관통한다.\n\n공격 방식: 단발 화살\n공격 범위: 직선 원거리",
    };
    private static readonly bool[] CharUnlocked = { true, true, true, true, true, true };

    void Awake() => Time.timeScale = 0f;

    void OnGUI()
    {
        if (selected) return;

        GUI.color = new Color(0f, 0f, 0f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (showingTitle) { DrawTitleScreen(); return; }

        DrawCharacterSelect();
    }

    void DrawCharacterSelect()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        // 타이틀
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 26,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        GUI.Label(new Rect(0, sh * 0.05f, sw, 40f), "캐릭터를 선택하세요", titleStyle);

        // 패널 레이아웃
        float panelY = sh * 0.14f;
        float panelH = sh * 0.72f;
        float panelX = sw * 0.1f;
        float totalW = sw * 0.8f;
        float leftW  = totalW * 0.26f;
        float gap    = totalW * 0.02f;
        float rightW = totalW - leftW - gap;
        float leftX  = panelX;
        float rightX = panelX + leftW + gap;

        DrawIconList(leftX, panelY, leftW, panelH);
        DrawDetailPanel(rightX, panelY, rightW, panelH);
    }

    void DrawIconList(float x, float y, float w, float h)
    {
        GUI.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

        float iconH = h / 6f;
        for (int i = 0; i < 6; i++)
        {
            int  charId     = i + 1;
            bool isSelected = selectedChar == charId;
            bool unlocked   = CharUnlocked[i];
            float iconY     = y + iconH * i;

            if (isSelected)
            {
                GUI.color = new Color(0.3f, 0.55f, 0.9f, 0.85f);
                GUI.DrawTexture(new Rect(x, iconY, w, iconH), Texture2D.whiteTexture);
            }

            float pad    = 10f;
            float boxSize = iconH - pad * 2f;
            float boxX   = x + (w - boxSize) / 2f;
            float boxY   = iconY + pad;

            Sprite iconSprite = (characterSprites != null && i < characterSprites.Length) ? characterSprites[i] : null;
            bool hasSprite = iconSprite != null;
            if (hasSprite)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(boxX, boxY, boxSize, boxSize),
                    iconSprite.texture, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.color = unlocked ? new Color(0.4f, 0.4f, 0.5f, 1f) : new Color(0.2f, 0.2f, 0.25f, 1f);
                GUI.DrawTexture(new Rect(boxX, boxY, boxSize, boxSize), Texture2D.whiteTexture);

                var numStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.color = unlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
                GUI.Label(new Rect(boxX, boxY, boxSize, boxSize), unlocked ? charId.ToString() : "?", numStyle);
            }

            GUI.color = Color.white;
            if (GUI.Button(new Rect(x, iconY, w, iconH), GUIContent.none, GUIStyle.none))
                selectedChar = charId;

            if (i < 5)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.1f);
                GUI.DrawTexture(new Rect(x, iconY + iconH - 1f, w, 1f), Texture2D.whiteTexture);
            }
        }

        GUI.color = Color.white;
    }

    void DrawDetailPanel(float x, float y, float w, float h)
    {
        GUI.color = new Color(0.08f, 0.1f, 0.18f, 0.95f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        int  idx         = selectedChar - 1;
        bool curUnlocked = CharUnlocked[idx];

        // 캐릭터 이미지
        float imgSize = Mathf.Min(w * 0.38f, h * 0.38f);
        float imgX    = x + (w - imgSize) / 2f;
        float imgY    = y + h * 0.06f;

        Sprite detailSprite = (characterSprites != null && idx < characterSprites.Length) ? characterSprites[idx] : null;
        bool hasSprite = detailSprite != null;
        if (hasSprite)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(imgX, imgY, imgSize, imgSize),
                detailSprite.texture, ScaleMode.ScaleToFit, true);
        }
        else
        {
            GUI.color = new Color(0.25f, 0.25f, 0.35f, 1f);
            GUI.DrawTexture(new Rect(imgX, imgY, imgSize, imgSize), Texture2D.whiteTexture);
            var lockStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 40,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.color = new Color(0.5f, 0.5f, 0.55f, 1f);
            GUI.Label(new Rect(imgX, imgY, imgSize, imgSize), "?", lockStyle);
        }

        float ty = imgY + imgSize + h * 0.04f;

        // 이름
        var nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(x, ty, w, 32f), CharNames[idx], nameStyle);
        ty += 36f;

        // 공격 타입
        var typeStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = new Color(1f, 0.85f, 0.3f, 1f);
        GUI.Label(new Rect(x, ty, w, 26f), CharTypes[idx], typeStyle);
        ty += 30f;

        // 구분선
        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUI.DrawTexture(new Rect(x + 24f, ty, w - 48f, 1f), Texture2D.whiteTexture);
        ty += 12f;

        // 설명
        var descStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.UpperLeft,
            wordWrap  = true
        };
        GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        GUI.Label(new Rect(x + 24f, ty, w - 48f, 130f), CharDescs[idx], descStyle);

        // 선택하기 버튼
        float btnW = 200f, btnH = 50f;
        float btnX = x + (w - btnW) / 2f;
        float btnY = y + h - btnH - 24f;

        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 20,
            fontStyle = FontStyle.Bold
        };

        if (curUnlocked)
        {
            GUI.color = new Color(0.25f, 0.7f, 0.3f, 1f);
            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "선택하기", btnStyle))
                SelectCharacter(selectedChar);
        }
        else
        {
            GUI.color = new Color(0.35f, 0.35f, 0.35f, 1f);
            GUI.Button(new Rect(btnX, btnY, btnW, btnH), "준비 중", btnStyle);
        }

        GUI.color = Color.white;
    }

    void DrawTitleScreen()
    {
        float sw = Screen.width;
        float sh = Screen.height;

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

        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        GUI.Label(new Rect(0, sh * 0.3f, sw, 70f), "Maple Survivor", titleStyle);

        GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        GUI.Label(new Rect(0, sh * 0.3f + 72f, sw, 30f), "메이플스토리 세계에서 살아남아라!", subStyle);

        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Bold
        };
        float btnW = 200f, btnH = 54f;
        float btnX = (sw - btnW) / 2f;
        float btnY = sh * 0.72f;

        GUI.color = new Color(0.25f, 0.7f, 0.3f, 1f);
        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "시작", btnStyle))
            showingTitle = false;

        GUI.color = Color.white;
    }

    void SelectCharacter(int id)
    {
        if (autoAttack         != null) autoAttack.enabled         = (id == 1);
        if (swordAttack        != null) swordAttack.enabled        = (id == 2);
        if (iceOrbAttack       != null) iceOrbAttack.enabled       = (id == 3);
        if (throwingStarAttack != null) throwingStarAttack.enabled = (id == 4);
        if (cannonAttack       != null) cannonAttack.enabled       = (id == 5);
        if (bowAttack          != null) bowAttack.enabled          = (id == 6);

        // 선택한 캐릭터 스프라이트를 PlayerController에 적용
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null && charSpriteSets != null)
        {
            int idx = id - 1;
            if (idx >= 0 && idx < charSpriteSets.Length && charSpriteSets[idx] != null)
            {
                var set = charSpriteSets[idx];
                pc.rightSprites = set.rightSprites;
                pc.leftSprites  = set.leftSprites;
                pc.useFlipX     = (set.leftSprites == null || set.leftSprites.Length == 0);
            }
        }

        selected = true;
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}
