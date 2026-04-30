using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public float  attackInterval    = 0.8f;
    public float  attackRange       = 5f;
    public int    damage            = 20;
    public int    weaponLevel       = 1;
    public int    weaponDamageBonus = 0;
    public float  effectOffset   = 1.3f;
    public Sprite slashSprite;

    // 공격 판정 각도 (양쪽 합산 120°)
    private const float HalfAngle = 75f;

    private float timer;
    private PlayerController pc;

    void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (PlayerStats.Instance != null)
            damage = PlayerStats.Instance.damage + weaponDamageBonus;

        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            PerformSlash();
        }
    }

    void PerformSlash()
    {
        Vector2 facing = pc != null ? pc.FacingDir : Vector2.right;

        var hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;

            // 바라보는 방향 기준 ±60° 안에 있는 적만 피격
            Vector2 toEnemy = (col.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(facing, toEnemy);
            if (angle > HalfAngle) continue;

            var enemy = col.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);

            var boss = col.GetComponent<BossEnemy>();
            if (boss != null) boss.TakeDamage(damage);
        }

        SpawnSlashEffect(facing);
    }

    void SpawnSlashEffect(Vector2 facing)
    {
        var go = new GameObject("SlashEffect");
        go.transform.position   = transform.position + (Vector3)(facing * effectOffset);
        float scale = 0.5f * (attackRange / 5f);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg + 180f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = slashSprite;
        sr.sortingOrder = 5;

        go.AddComponent<SlashEffect>();
    }
}
