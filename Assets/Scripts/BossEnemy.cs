using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public float moveSpeed     = 1.5f;
    public int   maxHP         = 500;
    public int   contactDamage = 25;
    public int   xpDropCount   = 10;
    public GameObject xpGemPrefab;

    [Header("Animation")]
    public Sprite[] sprites;
    public float    animFrameRate = 6f;

    private int         currentHP;
    private Transform   player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform   visual;
    private Vector3     baseScale;
    private float       animTimer;
    private int         animFrame;

    void Start()
    {
        rb     = GetComponent<Rigidbody2D>();
        sr     = GetComponentInChildren<SpriteRenderer>();
        visual = sr != null ? sr.transform : transform;
        baseScale = visual.localScale;
        currentHP = Mathf.RoundToInt(maxHP * (GameManager.Instance?.EnemyHPMultiplier ?? 1f));

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
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

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (dir.x != 0)
            visual.localScale = new Vector3(
                dir.x > 0 ? -baseScale.x : baseScale.x,
                baseScale.y, baseScale.z);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerController>()?.TakeDamage(contactDamage);
    }

    public void TakeDamage(int damage)
    {
        DamageTextManager.Instance?.Show(transform.position, damage, Color.yellow);
        currentHP -= damage;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        PlayerStats.Instance?.AddKill();
        if (xpGemPrefab != null)
            for (int i = 0; i < xpDropCount; i++)
                Instantiate(xpGemPrefab, transform.position + (Vector3)(Random.insideUnitCircle * 1.5f), Quaternion.identity);
        Destroy(gameObject);
    }
}
