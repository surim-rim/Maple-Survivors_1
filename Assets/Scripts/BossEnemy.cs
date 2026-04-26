using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public float moveSpeed    = 1.5f;
    public int   maxHP        = 500;
    public int   contactDamage = 25;
    public int   xpDropCount  = 10;
    public GameObject xpGemPrefab;

    private int         currentHP;
    private Transform   player;
    private Rigidbody2D rb;
    private Vector3     baseScale;

    void Start()
    {
        rb        = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        currentHP = Mathf.RoundToInt(maxHP * (GameManager.Instance?.EnemyHPMultiplier ?? 1f));

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (dir.x != 0)
        {
            float sx = dir.x > 0 ? Mathf.Abs(baseScale.x) : -Mathf.Abs(baseScale.x);
            transform.localScale = new Vector3(sx, baseScale.y, 1f);
        }
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
