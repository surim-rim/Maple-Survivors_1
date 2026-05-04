using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    private bool isPaused;
    private bool showSettings;
    private bool showExitConfirm;

    // 설정 탭 (0=디스플레이, 1=사운드, 2=게임플레이)
    private int  settingsTab     = 0;

    // 디스플레이
    private static readonly string[] Resolutions = { "1280 x 720", "1600 x 900", "1920 x 1080", "2560 x 1440" };
    private static readonly int[]    ResW        = { 1280, 1600, 1920, 2560 };
    private static readonly int[]    ResH        = {  720,  900, 1080, 1440 };
    private int  resolutionIndex = 2;
    private bool isFullscreen    = false;

    // 사운드 (UI only)
    private float sfxVolume = 1f;
    private bool  sfxMuted  = false;
    private float bgmVolume = 1f;
    private bool  bgmMuted  = false;

    void Awake() => Instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 게임오버 또는 업그레이드 선택 중에는 ESC 무시
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.State.GameOver) return;

            // 다른 UI(타이틀/캐릭터선택/레벨업)가 이미 정지 중이면 무시
            if (!isPaused && Time.timeScale == 0f) return;

            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        isPaused        = true;
        showSettings    = false;
        showExitConfirm = false;
        Time.timeScale  = 0f;
    }

    public void Resume()
    {
        isPaused        = false;
        showSettings    = false;
        showExitConfirm = false;
        Time.timeScale  = 1f;
    }

    void OnGUI()
    {
        if (!isPaused) return;

        // 반투명 배경
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        if (showExitConfirm)
            DrawExitConfirm();
        else if (showSettings)
            DrawSettings();
        else
            DrawPausePanel();
    }

    void DrawPausePanel()
    {
        float pw = 340f, ph = 600f;
        float px = (Screen.width  - pw) / 2f;
        float py = (Screen.height - ph) / 2f;

        // 패널 배경
        GUI.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);

        // 테두리
        GUI.color = new Color(1f, 0.9f, 0.2f, 0.6f);
        GUI.DrawTexture(new Rect(px,        py,      pw, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px,        py+ph-2, pw, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px,        py,      2f, ph), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px+pw-2,   py,      2f, ph), Texture2D.whiteTexture);

        // 타이틀
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        GUI.Label(new Rect(px, py + 20f, pw, 36f), "일시정지", titleStyle);

        // 구분선
        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUI.DrawTexture(new Rect(px + 20f, py + 62f, pw - 40f, 1f), Texture2D.whiteTexture);

        // ── 현재 스탯 ──
        DrawStats(px + 20f, py + 74f, pw - 40f);

        // 구분선
        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUI.DrawTexture(new Rect(px + 20f, py + 410f, pw - 40f, 1f), Texture2D.whiteTexture);

        float btnW = pw - 60f;
        float btnX = px + 30f;
        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };

        // 계속하기 버튼
        GUI.color = new Color(0.25f, 0.65f, 0.3f, 1f);
        if (GUI.Button(new Rect(btnX, py + 426f, btnW, 44f), "계속하기", btnStyle))
            Resume();

        // 설정 버튼
        GUI.color = new Color(0.3f, 0.45f, 0.7f, 1f);
        if (GUI.Button(new Rect(btnX, py + 480f, btnW, 44f), "설정", btnStyle))
            showSettings = true;

        // 나가기 버튼 (확인창 열기)
        GUI.color = new Color(0.6f, 0.25f, 0.25f, 1f);
        if (GUI.Button(new Rect(btnX, py + 534f, btnW, 44f), "나가기", btnStyle))
            showExitConfirm = true;

        GUI.color = Color.white;
    }

    void DrawStats(float x, float y, float w)
    {
        var ps = PlayerStats.Instance;
        var pc = FindObjectOfType<PlayerController>();
        var gm = GameManager.Instance;
        var sa = FindObjectOfType<SwordAttack>();

        int elapsed = Mathf.FloorToInt(gm?.ElapsedTime ?? 0f);
        int min = elapsed / 60, sec = elapsed % 60;

        var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        var valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            alignment = TextAnchor.MiddleRight
        };

        GUI.color = new Color(0.85f, 0.85f, 1f, 1f);
        float lineH = 26f;
        float vy = y;

        DrawStatRow(x, vy, w, lineH, "레벨",         $"{ps?.level ?? 1}",            labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "생존 시간",   $"{min:00}:{sec:00}",           labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "처치 수",     $"{ps?.killCount ?? 0}",        labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "골드",        $"{ps?.gold ?? 0}",             labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "HP",          $"{pc?.CurrentHP ?? 0} / {pc?.maxHP ?? 0}", labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "공격력",      $"{ps?.damage ?? 0}",           labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "방어력",      $"{ps?.defense ?? 0}",          labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "공격 속도",   $"{(ps != null ? (1f / ps.attackInterval).ToString("F1") : "-")} /s", labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "이동 속도",   $"{pc?.moveSpeed ?? 0:F1}",     labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "젬 흡수 범위",$"{ps?.gemPickupRadius ?? 0:F1}", labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "경험치 배율", $"{(ps != null ? ps.xpMultiplier * 100f : 100f):F0}%", labelStyle, valueStyle); vy += lineH;
        DrawStatRow(x, vy, w, lineH, "공격 범위",   sa != null ? $"{sa.attackRange:F1}" : "-",              labelStyle, valueStyle); vy += lineH;
        string reviveStatus = pc == null ? "없음" : pc.hasRevive ? "있음" : pc.reviveConsumed ? "사용함" : "없음";
        DrawStatRow(x, vy, w, lineH, "부활", reviveStatus, labelStyle, valueStyle);
    }

    void DrawStatRow(float x, float y, float w, float h,
                     string label, string value,
                     GUIStyle labelStyle, GUIStyle valueStyle)
    {
        GUI.color = new Color(0.75f, 0.75f, 0.9f, 1f);
        GUI.Label(new Rect(x,     y, w * 0.6f, h), label, labelStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(x,     y, w,        h), value, valueStyle);
    }

    void DrawExitConfirm()
    {
        float pw = 300f, ph = 160f;
        float px = (Screen.width  - pw) / 2f;
        float py = (Screen.height - ph) / 2f;

        // 배경
        GUI.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);

        // 테두리
        GUI.color = new Color(1f, 0.9f, 0.2f, 0.6f);
        GUI.DrawTexture(new Rect(px,      py,      pw, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px,      py+ph-2, pw, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px,      py,      2f, ph), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px+pw-2, py,      2f, ph), Texture2D.whiteTexture);

        var msgStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 15 };

        GUI.color = Color.white;
        GUI.Label(new Rect(px, py + 34f, pw, 30f), "정말 나가시겠습니까?", msgStyle);

        float btnW = 100f, btnH = 40f;
        float gap  = 20f;
        float totalBtnW = btnW * 2 + gap;
        float btnX = px + (pw - totalBtnW) / 2f;
        float btnY = py + 96f;

        // 예
        GUI.color = new Color(0.6f, 0.25f, 0.25f, 1f);
        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "예", btnStyle))
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // 아니요
        GUI.color = new Color(0.3f, 0.45f, 0.3f, 1f);
        if (GUI.Button(new Rect(btnX + btnW + gap, btnY, btnW, btnH), "아니요", btnStyle))
            showExitConfirm = false;

        GUI.color = Color.white;
    }

    void DrawSettings()
    {
        float pw = 460f, ph = 420f;
        float px = (Screen.width  - pw) / 2f;
        float py = (Screen.height - ph) / 2f;

        // 패널 배경
        GUI.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);

        // 테두리
        GUI.color = new Color(1f, 0.9f, 0.2f, 0.6f);
        GUI.DrawTexture(new Rect(px,      py,      pw, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px,      py+ph-2, pw, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px,      py,      2f, ph), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(px+pw-2, py,      2f, ph), Texture2D.whiteTexture);

        // 타이틀
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        GUI.Label(new Rect(px, py + 16f, pw, 32f), "설정", titleStyle);

        // 구분선
        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUI.DrawTexture(new Rect(px + 20f, py + 54f, pw - 40f, 1f), Texture2D.whiteTexture);

        // ── 탭 버튼 ──
        string[] tabNames = { "디스플레이", "사운드", "게임플레이" };
        float tabW  = (pw - 40f) / 3f;
        float tabH  = 34f;
        float tabY  = py + 62f;
        var tabStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold };

        for (int i = 0; i < 3; i++)
        {
            GUI.color = (settingsTab == i)
                ? new Color(0.3f, 0.5f, 0.8f, 1f)
                : new Color(0.22f, 0.22f, 0.28f, 1f);
            if (GUI.Button(new Rect(px + 20f + i * tabW, tabY, tabW, tabH), tabNames[i], tabStyle))
                settingsTab = i;
        }

        // 콘텐츠 영역 배경
        GUI.color = new Color(0.07f, 0.07f, 0.12f, 1f);
        GUI.DrawTexture(new Rect(px + 20f, py + 100f, pw - 40f, ph - 160f), Texture2D.whiteTexture);

        // ── 탭 콘텐츠 ──
        float cx = px + 36f, cw = pw - 72f, cy = py + 116f;
        switch (settingsTab)
        {
            case 0: DrawDisplayTab(cx, cy, cw);   break;
            case 1: DrawSoundTab(cx, cy, cw);     break;
            case 2: DrawGameplayTab(cx, cy, cw);  break;
        }

        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 15 };

        // 적용 버튼 (디스플레이 탭일 때만)
        if (settingsTab == 0)
        {
            GUI.color = new Color(0.3f, 0.55f, 0.7f, 1f);
            if (GUI.Button(new Rect(px + 30f, py + ph - 104f, pw - 60f, 40f), "적용", btnStyle))
                Screen.SetResolution(ResW[resolutionIndex], ResH[resolutionIndex], isFullscreen);
        }

        // 돌아가기 버튼
        GUI.color = new Color(0.5f, 0.5f, 0.55f, 1f);
        if (GUI.Button(new Rect(px + 30f, py + ph - 56f, pw - 60f, 40f), "돌아가기", btnStyle))
            showSettings = false;

        GUI.color = Color.white;
    }

    void DrawDisplayTab(float x, float y, float w)
    {
        var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        var btnStyle   = new GUIStyle(GUI.skin.button) { fontSize = 12 };

        // 해상도
        GUI.color = new Color(0.85f, 0.85f, 1f, 1f);
        GUI.Label(new Rect(x, y, w, 24f), "해상도", labelStyle);

        float arrowW = 28f, dispW = w - arrowW * 2 - 8f;
        GUI.color = new Color(0.3f, 0.45f, 0.7f, 1f);
        if (GUI.Button(new Rect(x, y + 28f, arrowW, 30f), "◀", btnStyle))
            resolutionIndex = Mathf.Max(0, resolutionIndex - 1);

        GUI.color = new Color(0.18f, 0.18f, 0.24f, 1f);
        GUI.DrawTexture(new Rect(x + arrowW + 4f, y + 28f, dispW, 30f), Texture2D.whiteTexture);
        var resStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(x + arrowW + 4f, y + 28f, dispW, 30f), Resolutions[resolutionIndex], resStyle);

        GUI.color = new Color(0.3f, 0.45f, 0.7f, 1f);
        if (GUI.Button(new Rect(x + arrowW + 4f + dispW + 4f, y + 28f, arrowW, 30f), "▶", btnStyle))
            resolutionIndex = Mathf.Min(Resolutions.Length - 1, resolutionIndex + 1);

        // 윈도우 모드
        GUI.color = new Color(0.85f, 0.85f, 1f, 1f);
        GUI.Label(new Rect(x, y + 80f, w, 24f), "윈도우 모드", labelStyle);

        float modeW = (w - 10f) / 2f;
        GUI.color = !isFullscreen ? new Color(0.3f, 0.55f, 0.3f, 1f) : new Color(0.22f, 0.22f, 0.28f, 1f);
        if (GUI.Button(new Rect(x, y + 108f, modeW, 34f), "창 모드", btnStyle))
            isFullscreen = false;

        GUI.color = isFullscreen ? new Color(0.3f, 0.55f, 0.3f, 1f) : new Color(0.22f, 0.22f, 0.28f, 1f);
        if (GUI.Button(new Rect(x + modeW + 10f, y + 108f, modeW, 34f), "전체 화면", btnStyle))
            isFullscreen = true;

        GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        var noteStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
        GUI.Label(new Rect(x, y + 152f, w, 22f), "※ 디스플레이 설정은 추후 적용 예정입니다.", noteStyle);

        GUI.color = Color.white;
    }

    void DrawSoundTab(float x, float y, float w)
    {
        var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        var muteStyle  = new GUIStyle(GUI.skin.button) { fontSize = 11 };
        var valStyle   = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 12,
            alignment = TextAnchor.MiddleRight
        };

        float sliderW = w - 90f;
        float muteW   = 62f;

        // 효과음
        GUI.color = new Color(0.85f, 0.85f, 1f, 1f);
        GUI.Label(new Rect(x, y, w, 24f), "효과음", labelStyle);

        GUI.color = sfxMuted ? new Color(0.6f, 0.25f, 0.25f, 1f) : new Color(0.25f, 0.5f, 0.25f, 1f);
        if (GUI.Button(new Rect(x + sliderW + 8f, y + 28f, muteW, 26f), sfxMuted ? "음소거" : "켜짐", muteStyle))
            sfxMuted = !sfxMuted;

        GUI.color = sfxMuted ? new Color(0.4f, 0.4f, 0.4f, 1f) : Color.white;
        sfxVolume = GUI.HorizontalSlider(new Rect(x, y + 32f, sliderW, 18f), sfxVolume, 0f, 1f);

        GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        GUI.Label(new Rect(x, y + 32f, sliderW, 18f), $"{Mathf.RoundToInt(sfxVolume * 100f)}%", valStyle);

        // 배경음악
        GUI.color = new Color(0.85f, 0.85f, 1f, 1f);
        GUI.Label(new Rect(x, y + 76f, w, 24f), "배경음악", labelStyle);

        GUI.color = bgmMuted ? new Color(0.6f, 0.25f, 0.25f, 1f) : new Color(0.25f, 0.5f, 0.25f, 1f);
        if (GUI.Button(new Rect(x + sliderW + 8f, y + 104f, muteW, 26f), bgmMuted ? "음소거" : "켜짐", muteStyle))
            bgmMuted = !bgmMuted;

        GUI.color = bgmMuted ? new Color(0.4f, 0.4f, 0.4f, 1f) : Color.white;
        bgmVolume = GUI.HorizontalSlider(new Rect(x, y + 108f, sliderW, 18f), bgmVolume, 0f, 1f);

        GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        GUI.Label(new Rect(x, y + 108f, sliderW, 18f), $"{Mathf.RoundToInt(bgmVolume * 100f)}%", valStyle);

        GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        var noteStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
        GUI.Label(new Rect(x, y + 148f, w, 22f), "※ 사운드 설정은 추후 적용 예정입니다.", noteStyle);

        GUI.color = Color.white;
    }

    void DrawGameplayTab(float x, float y, float w)
    {
        var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        var btnStyle   = new GUIStyle(GUI.skin.button) { fontSize = 12 };

        GUI.color = new Color(0.85f, 0.85f, 1f, 1f);
        GUI.Label(new Rect(x, y, w, 24f), "피해량 표시", labelStyle);

        float btnW = (w - 10f) / 2f;
        GUI.color = DamageTextManager.ShowDamage
            ? new Color(0.3f, 0.55f, 0.3f, 1f)
            : new Color(0.22f, 0.22f, 0.28f, 1f);
        if (GUI.Button(new Rect(x, y + 32f, btnW, 34f), "표시", btnStyle))
            DamageTextManager.ShowDamage = true;

        GUI.color = !DamageTextManager.ShowDamage
            ? new Color(0.6f, 0.25f, 0.25f, 1f)
            : new Color(0.22f, 0.22f, 0.28f, 1f);
        if (GUI.Button(new Rect(x + btnW + 10f, y + 32f, btnW, 34f), "숨김", btnStyle))
            DamageTextManager.ShowDamage = false;

        GUI.color = Color.white;
    }
}
