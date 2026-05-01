using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    public float barWidth    = 65f;
    public float barHeight   = 5f;
    public float worldOffset = 1.2f; // 캐릭터 중심 기준 아래 방향 오프셋 (월드 단위)
    public float offsetX     = -4.0f; // 좌우 보정값 (양수=오른쪽, 음수=왼쪽)

    private PlayerController pc;
    private Camera           mainCam;

    void Start()
    {
        pc      = GetComponent<PlayerController>();
        mainCam = Camera.main;
    }

    void OnGUI()
    {
        if (pc == null || mainCam == null) return;

        float ratio = pc.maxHP > 0 ? Mathf.Clamp01((float)pc.CurrentHP / pc.maxHP) : 0f;

        Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position - new Vector3(0, worldOffset, 0));
        float dirSign = pc.FacingDir.x >= 0 ? 1f : -1f;
        float barX = screenPos.x - barWidth / 2f + offsetX * dirSign;
        float barY = Screen.height - screenPos.y;

        // 배경
        GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        GUI.DrawTexture(new Rect(barX - 1f, barY - 1f, barWidth + 2f, barHeight + 2f), Texture2D.whiteTexture);

        // HP 바
        GUI.color = Color.Lerp(Color.red, new Color(0.15f, 0.85f, 0.15f, 1f), ratio);
        GUI.DrawTexture(new Rect(barX, barY, barWidth * ratio, barHeight), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }
}
