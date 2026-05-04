using UnityEngine;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    public Sprite[] upgradeIcons = new Sprite[5]; // id 순서대로 (0~4)

    private List<UpgradeManager.UpgradeOption> upgradeChoices;
    private bool  showingUpgrades;
    private bool  gameOver;
    private int   selectedUpgradeIndex = 0;
    private bool  upgradeConfirmMode   = false;
    private float reviveMessageTimer   = 0f;

    void Awake() => Instance = this;

    public void ShowUpgradePanel(List<UpgradeManager.UpgradeOption> choices)
    {
        upgradeChoices       = choices;
        showingUpgrades      = true;
        selectedUpgradeIndex = 0;
        upgradeConfirmMode   = false;
    }

    void Update()
    {
        if (reviveMessageTimer > 0f)
            reviveMessageTimer -= Time.deltaTime;

        if (!showingUpgrades || upgradeChoices == null || upgradeChoices.Count == 0) return;

        int count    = upgradeChoices.Count;
        bool navRight = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        bool navLeft  = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);

        if (navRight || navLeft)
        {
            upgradeConfirmMode   = false;
            int next = selectedUpgradeIndex + (navRight ? 1 : -1);
            if (next < 0)      next = count - 1;
            if (next >= count) next = 0;
            selectedUpgradeIndex = next;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))  { upgradeConfirmMode = false; return; }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!upgradeConfirmMode) upgradeConfirmMode = true;
            else UpgradeManager.Instance?.ApplyUpgrade(upgradeChoices[selectedUpgradeIndex].id);
        }
    }

    public void HideUpgradePanel()
    {
        showingUpgrades = false;
        upgradeChoices  = null;
    }

    void OnGUI()
    {
        // 게임오버는 pc가 비활성화된 이후에도 반드시 표시
        if (gameOver) { DrawGameOver(); return; }

        var ps = PlayerStats.Instance;
        var pc = FindObjectOfType<PlayerController>();
        if (ps == null || pc == null) return;

        DrawXPBar(ps);
        DrawStats(ps);
        DrawWeaponSlots();
        DrawStatSlots();
        if (reviveMessageTimer > 0f) DrawReviveMessage();
        if (showingUpgrades) DrawUpgradePanel();
    }

    void DrawHPBar(PlayerController pc)
    {
        float ratio = pc.maxHP > 0 ? (float)pc.CurrentHP / pc.maxHP : 0f;
        float barW  = Screen.width * 0.6f;
        float barH  = 20f;
        float barX  = (Screen.width - barW) / 2f;
        float barY  = Screen.height - 62f;

        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

        GUI.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        GUI.DrawTexture(new Rect(barX, barY, barW * ratio, barH), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(barX, barY, barW, barH), $"  HP {pc.CurrentHP} / {pc.maxHP}");
    }

    void DrawXPBar(PlayerStats ps)
    {
        float ratio = ps.xpToNextLevel > 0 ? (float)ps.currentXP / ps.xpToNextLevel : 0f;
        float barW  = Screen.width * 0.6f;
        float barH  = 20f;
        float barX  = (Screen.width - barW) / 2f;
        float barY  = Screen.height - 36f;

        // 레벨 표시 — XP바 왼쪽
        var lvStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight
        };
        GUI.color = new Color(1f, 0.9f, 0.2f, 1f);
        GUI.Label(new Rect(barX - 64f, barY, 60f, barH), $"Lv.{ps.level}", lvStyle);

        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

        GUI.color = new Color(0.2f, 0.75f, 0.2f, 1f);
        GUI.DrawTexture(new Rect(barX, barY, barW * ratio, barH), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(barX, barY, barW, barH), $"  XP {ps.currentXP} / {ps.xpToNextLevel}");
    }

    void DrawStats(PlayerStats ps)
    {
        int elapsed = Mathf.FloorToInt(GameManager.Instance?.ElapsedTime ?? 0f);
        int min = elapsed / 60, sec = elapsed % 60;

        var boldStyle   = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
        var centerStyle = new GUIStyle(boldStyle) { alignment = TextAnchor.MiddleCenter };
        var rightStyle  = new GUIStyle(boldStyle) { alignment = TextAnchor.MiddleRight };

        // 생존 시간 — 최상단 중앙
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width / 2f - 80f, 8f, 160f, 26f), $"{min:00}:{sec:00}", centerStyle);

        // 처치 / 골드 — 최상단 우측
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 170f, 8f,  160f, 26f), $"처치: {ps.killCount}", rightStyle);
        GUI.color = new Color(1f, 0.85f, 0.1f, 1f);
        GUI.Label(new Rect(Screen.width - 170f, 30f, 160f, 26f), $"골드: {ps.gold}",     rightStyle);
        GUI.color = Color.white;
    }

    public void ShowReviveMessage() => reviveMessageTimer = 2f;

    public void ShowGameOver() => gameOver = true;

    void DrawGameOver()
    {
        var ps = PlayerStats.Instance;
        int elapsed = Mathf.FloorToInt(GameManager.Instance?.ElapsedTime ?? 0f);
        int min = elapsed / 60, sec = elapsed % 60;

        float pw = 480f, ph = 300f;
        float px = (Screen.width  - pw) / 2f;
        float py = (Screen.height - ph) / 2f;

        // 배경
        GUI.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);

        GUI.color = new Color(1f, 0.25f, 0.25f, 1f);
        GUI.Label(new Rect(px, py + 20f, pw, 40f), "<b><size=26>   GAME OVER</size></b>");

        GUI.color = Color.white;
        GUI.Label(new Rect(px + 60f, py + 80f,  pw, 30f), $"생존 시간   {min:00}:{sec:00}");
        GUI.Label(new Rect(px + 60f, py + 114f, pw, 30f), $"최종 레벨   {ps?.level ?? 1}");
        GUI.Label(new Rect(px + 60f, py + 148f, pw, 30f), $"총 처치 수  {ps?.killCount ?? 0}");

        GUI.color = new Color(0.3f, 0.65f, 0.3f, 1f);
        if (GUI.Button(new Rect(px + pw / 2f - 90f, py + ph - 68f, 180f, 44f), "다시 시작"))
            GameManager.Instance?.Restart();

        GUI.color = Color.white;
    }

    void DrawWeaponSlots()
    {
        var sa = FindObjectOfType<SwordAttack>();
        var io = FindObjectOfType<IceOrbAttack>();
        var ta = FindObjectOfType<ThrowingStarAttack>();
        var ca = FindObjectOfType<CannonAttack>();
        var ba = FindObjectOfType<BowAttack>();

        bool[] owned  = { sa != null && sa.enabled, io != null && io.enabled, ta != null && ta.enabled, ca != null && ca.enabled, ba != null && ba.enabled };
        int[]  levels = { sa != null ? sa.weaponLevel : 1, io != null ? io.weaponLevel : 1, ta != null ? ta.weaponLevel : 1, ca != null ? ca.weaponLevel : 1, ba != null ? ba.weaponLevel : 1 };
        int[]  icons  = { 5, 6, 7, 8, 9 };

        float boxSize = 48f;
        float gap     = 5f;
        float startX  = 10f;
        float startY  = 10f;

        var lvStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerRight
        };

        for (int i = 0; i < 5; i++)
        {
            float bx = startX + i * (boxSize + gap);
            float by = startY;

            // 배경
            GUI.color = owned[i]
                ? new Color(0.15f, 0.25f, 0.45f, 0.92f)
                : new Color(0.10f, 0.10f, 0.13f, 0.85f);
            GUI.DrawTexture(new Rect(bx, by, boxSize, boxSize), Texture2D.whiteTexture);

            // 테두리
            float bw = 1.5f;
            GUI.color = owned[i]
                ? new Color(0.45f, 0.75f, 1f, 0.9f)
                : new Color(0.28f, 0.28f, 0.32f, 0.8f);
            GUI.DrawTexture(new Rect(bx,                 by,                 boxSize, bw),      Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(bx,                 by + boxSize - bw,  boxSize, bw),      Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(bx,                 by,                 bw,      boxSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(bx + boxSize - bw,  by,                 bw,      boxSize), Texture2D.whiteTexture);

            // 무기 아이콘
            Sprite icon = (upgradeIcons != null && icons[i] < upgradeIcons.Length) ? upgradeIcons[icons[i]] : null;
            if (icon != null)
            {
                float pad  = 6f;
                float imgS = boxSize - pad * 2f;
                GUI.color = owned[i] ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                GUI.DrawTexture(new Rect(bx + pad, by + pad, imgS, imgS), icon.texture, ScaleMode.ScaleToFit, true);
            }

            // 레벨 뱃지 (보유 시)
            if (owned[i])
            {
                GUI.color = new Color(1f, 0.95f, 0.35f, 1f);
                GUI.Label(new Rect(bx + 1f, by + 1f, boxSize - 3f, boxSize - 3f), $"Lv.{levels[i]}", lvStyle);
            }
        }

        GUI.color = Color.white;
    }

    void DrawStatSlots()
    {
        var um = UpgradeManager.Instance;
        if (um == null) return;

        float boxSize = 48f;
        float gap     = 5f;
        float startX  = 10f;
        float startY  = 68f; // 무기 슬롯(y=10, h=48) + 여백 10

        var lvStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerRight
        };

        for (int i = 0; i < 5; i++)
        {
            float bx = startX + i * (boxSize + gap);
            float by = startY;

            bool owned  = i < um.ownedStatIds.Count;
            int  statId = owned ? um.ownedStatIds[i] : -1;
            int  level  = (owned && um.statLevels.ContainsKey(statId)) ? um.statLevels[statId] : 0;

            // 배경
            GUI.color = owned
                ? new Color(0.25f, 0.15f, 0.42f, 0.92f)
                : new Color(0.10f, 0.10f, 0.13f, 0.85f);
            GUI.DrawTexture(new Rect(bx, by, boxSize, boxSize), Texture2D.whiteTexture);

            // 테두리
            float bw = 1.5f;
            GUI.color = owned
                ? new Color(0.75f, 0.45f, 1f, 0.9f)
                : new Color(0.28f, 0.28f, 0.32f, 0.8f);
            GUI.DrawTexture(new Rect(bx,                 by,                 boxSize, bw),      Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(bx,                 by + boxSize - bw,  boxSize, bw),      Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(bx,                 by,                 bw,      boxSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(bx + boxSize - bw,  by,                 bw,      boxSize), Texture2D.whiteTexture);

            if (owned)
            {
                // 아이콘 (upgradeIcons 배열의 statId 인덱스 직접 사용)
                Sprite icon = (upgradeIcons != null && statId < upgradeIcons.Length) ? upgradeIcons[statId] : null;
                if (icon != null)
                {
                    float pad  = 6f;
                    float imgS = boxSize - pad * 2f;
                    GUI.color = Color.white;
                    GUI.DrawTexture(new Rect(bx + pad, by + pad, imgS, imgS), icon.texture, ScaleMode.ScaleToFit, true);
                }

                // 레벨 뱃지
                GUI.color = new Color(0.95f, 0.75f, 1f, 1f);
                GUI.Label(new Rect(bx + 1f, by + 1f, boxSize - 3f, boxSize - 3f), $"Lv.{level}", lvStyle);
            }
        }

        GUI.color = Color.white;
    }

    void DrawReviveMessage()
    {
        float alpha = Mathf.Clamp01(reviveMessageTimer);
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.color = new Color(1f, 0.85f, 0.1f, alpha);
        GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 70f), "부활!", style);
        GUI.color = Color.white;
    }

    void DrawUpgradePanel()
    {
        if (upgradeChoices == null) return;

        float panelW = 620f;
        float panelH = 340f;
        float panelX = (Screen.width  - panelW) / 2f;
        float panelY = (Screen.height - panelH) / 2f;

        // 배경
        GUI.color = new Color(0.08f, 0.08f, 0.12f, 0.97f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(panelX, panelY + 14f, panelW, 30f),
            "<b><size=17>   레벨 업! 업그레이드를 선택하세요</size></b>");

        float cardW  = 165f;
        float cardH  = 230f;
        float gap    = 18f;
        float totalW = upgradeChoices.Count * cardW + (upgradeChoices.Count - 1) * gap;
        float startX = panelX + (panelW - totalW) / 2f;
        float cardY  = panelY + 58f;

        for (int i = 0; i < upgradeChoices.Count; i++)
        {
            var opt = upgradeChoices[i];
            float cx = startX + i * (cardW + gap);

            bool isSelected = (i == selectedUpgradeIndex);
            bool isConfirm  = isSelected && upgradeConfirmMode;

            // 카드 배경
            if (isConfirm)
                GUI.color = new Color(0.45f, 0.35f, 0.08f, 1f);
            else if (isSelected)
                GUI.color = new Color(0.28f, 0.42f, 0.68f, 1f);
            else
                GUI.color = new Color(0.18f, 0.28f, 0.48f, 1f);
            GUI.DrawTexture(new Rect(cx, cardY, cardW, cardH), Texture2D.whiteTexture);

            // 이름
            GUI.color = Color.white;
            GUI.Label(new Rect(cx + 10f, cardY + 10f, cardW - 20f, 26f), $"<b><size=14>{opt.name}</size></b>");

            // 아이콘 이미지 (id 20~24: 무기 획득 → 동일 무기 아이콘 재사용, 20→5, 21→6, ...)
            int iconIdx = opt.id >= 20 ? opt.id - 15 : opt.id;
            Sprite icon = (upgradeIcons != null && iconIdx < upgradeIcons.Length) ? upgradeIcons[iconIdx] : null;
            if (icon != null)
            {
                float imgSize = 64f;
                float imgX    = cx + (cardW - imgSize) / 2f;
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(imgX, cardY + 40f, imgSize, imgSize),
                    icon.texture, ScaleMode.ScaleToFit, true);
            }

            // 설명
            var descStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 11 };
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            GUI.Label(new Rect(cx + 10f, cardY + 112f, cardW - 20f, 72f), opt.description, descStyle);

            // 선택 버튼
            if (isConfirm)
            {
                GUI.color = new Color(1f, 0.8f, 0.1f, 1f);
                if (GUI.Button(new Rect(cx + 12f, cardY + cardH - 46f, cardW - 24f, 36f), "확인 [Enter]"))
                    UpgradeManager.Instance?.ApplyUpgrade(opt.id);
            }
            else
            {
                GUI.color = new Color(0.25f, 0.6f, 0.25f, 1f);
                if (GUI.Button(new Rect(cx + 12f, cardY + cardH - 46f, cardW - 24f, 36f), "선택"))
                    UpgradeManager.Instance?.ApplyUpgrade(opt.id);
            }
        }

        GUI.color = Color.white;
    }
}
