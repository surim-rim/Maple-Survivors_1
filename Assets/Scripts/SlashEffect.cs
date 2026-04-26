using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private float timer;
    private const float Lifetime = 0.2f;

    void Start() => sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        timer += Time.deltaTime;
        if (sr != null)
            sr.color = new Color(1f, 0.9f, 0.3f, (1f - timer / Lifetime) * 0.55f);
        if (timer >= Lifetime)
            Destroy(gameObject);
    }
}
