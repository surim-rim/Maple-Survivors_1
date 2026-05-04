using UnityEngine;
using System.Collections;

public class SwordAttack : MonoBehaviour
{
    public float  attackInterval    = 0.8f;
    public float  attackRange       = 5f;
    public int    damage            = 20;
    public int    weaponLevel       = 1;
    public int    weaponDamageBonus = 0;
    public float  effectOffset      = 1.3f;
    public Sprite slashSprite;
    public int    extraAttackCount  = 0;

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
            StartCoroutine(MultiSlash());
        }
    }

    IEnumerator MultiSlash()
    {
        int total = 1 + extraAttackCount;
        for (int i = 0; i < total; i++)
        {
            PerformSlash();
            if (i < total - 1) yield return new WaitForSeconds(0.15f);
        }
    }

    void PerformSlash()
    {
        Vector2 facing = pc != null ? pc.FacingDir : Vector2.right;

        var hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var col in hits)
        {
            bool isEnemy = col.CompareTag("Enemy");
            bool isBox   = col.CompareTag("Box");
            if (!isEnemy && !isBox) continue;

            Vector2 toTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(facing, toTarget);
            if (angle > HalfAngle) continue;

            if (isEnemy)
            {
                col.GetComponent<Enemy>()?.TakeDamage(damage);
                col.GetComponent<BossEnemy>()?.TakeDamage(damage);
            }
            if (isBox) col.GetComponent<RandomBox>()?.TakeDamage(damage);
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
