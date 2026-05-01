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

        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 160, 25), $"Lv. {ps.level}");
        GUI.Label(new Rect(10, 30, 160, 25), $"처치: {ps.killCount}");
        GUI.Label(new Rect(10, 50, 160, 25), $"시간: {min:00}:{sec:00}");
        GUI.color = new Color(1f, 0.85f, 0.1f, 1f);
        GUI.Label(new Rect(10, 70, 160, 25), $"골드: {ps.gold}");
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

            // 아이콘 이미지
            Sprite icon = (upgradeIcons != null && opt.id < upgradeIcons.Length) ? upgradeIcons[opt.id] : null;
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
