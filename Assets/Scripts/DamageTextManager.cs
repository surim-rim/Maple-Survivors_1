using UnityEngine;
using System.Collections.Generic;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    private class FloatingText
    {
        public Vector3 worldPos;
        public string  text;
        public Color   color;
        public float   age;
        public const float Lifetime = 0.9f;
    }

    private readonly List<FloatingText> texts = new List<FloatingText>();
    private Camera    cam;
    private GUIStyle  style;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public static bool ShowDamage = true;

    public void Show(Vector3 worldPos, int damage, Color color)
    {
        if (!ShowDamage) return;
        texts.Add(new FloatingText { worldPos = worldPos, text = damage.ToString(), color = color, age = 0f });
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        for (int i = texts.Count - 1; i >= 0; i--)
        {
            texts[i].age      += Time.deltaTime;
            texts[i].worldPos += Vector3.up * 1.8f * Time.deltaTime;
            if (texts[i].age >= FloatingText.Lifetime)
                texts.RemoveAt(i);
        }
    }

    void OnGUI()
    {
        if (cam == null) return;

        if (style == null)
            style = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        foreach (var t in texts)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(t.worldPos);
            if (screenPos.z < 0) continue;

            Color c = t.color;
            c.a = 1f - (t.age / FloatingText.Lifetime);
            GUI.color = c;
            GUI.Label(new Rect(screenPos.x - 40f, Screen.height - screenPos.y - 20f, 80f, 40f), t.text, style);
        }

        GUI.color = Color.white;
    }
}
