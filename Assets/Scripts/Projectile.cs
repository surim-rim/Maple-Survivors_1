using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 20;
    public float lifetime = 3f;

    [Header("Animation")]
    public Sprite[] sprites;
    public float animFrameRate = 8f;

    private Vector2 direction;
    private SpriteRenderer sr;
    private float animTimer;
    private int animFrame;
    private bool initialized;

    public void Init(Vector2 dir)
    {
        sr = GetComponent<SpriteRenderer>();
        direction = dir.normalized;

        // PlayerStats 에서 데미지 반영
        if (PlayerStats.Instance != null)
            damage = PlayerStats.Instance.damage;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (sr != null && sprites != null && sprites.Length > 0)
            sr.sprite = sprites[0];

        initialized = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!initialized) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        AnimateSprite();
    }

    void AnimateSprite()
    {
        if (sr == null || sprites == null || sprites.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= 1f / animFrameRate)
        {
            animTimer = 0f;
            animFrame = (animFrame + 1) % sprites.Length;
            sr.sprite = sprites[animFrame];
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        if (other.CompareTag("Enemy"))
        {
            bool hit = false;
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null) { enemy.TakeDamage(damage); hit = true; }
            var boss = other.GetComponent<BossEnemy>();
            if (boss != null) { boss.TakeDamage(damage); hit = true; }
            if (hit) Destroy(gameObject);
        }
        else if (other.CompareTag("Box"))
        {
            other.GetComponent<RandomBox>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
