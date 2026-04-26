using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int maxHP = 30;
    public int contactDamage = 10;
    public GameObject xpGemPrefab;

    private int currentHP;
    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 웨이브 난이도 반영
        currentHP = Mathf.RoundToInt(maxHP    * (GameManager.Instance?.EnemyHPMultiplier    ?? 1f));
        moveSpeed *= GameManager.Instance?.EnemySpeedMultiplier ?? 1f;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (dir.x != 0)
            transform.localScale = new Vector3(dir.x > 0 ? 1 : -1, 1, 1);
    }

    // isTrigger=false 이므로 OnCollisionStay2D 로 데미지 처리
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerController>()?.TakeDamage(contactDamage);
    }

    public void TakeDamage(int damage)
    {
        DamageTextManager.Instance?.Show(transform.position, damage, Color.red);
        currentHP -= damage;
        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        PlayerStats.Instance?.AddKill();
        if (xpGemPrefab != null)
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
