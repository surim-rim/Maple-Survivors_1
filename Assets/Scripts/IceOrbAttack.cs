using UnityEngine;

public class IceOrbAttack : MonoBehaviour
{
    public float  attackInterval  = 2f;
    public float  projectileSpeed = 3f;   // 천천히 날아감
    public float  damageInterval  = 0.6f;
    public float  damageRange     = 1.5f;
    public float  lifetime        = 4f;
    public Sprite orbSprite;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            FireOrb();
        }
    }

    void FireOrb()
    {
        var target = FindNearestEnemy();
        Vector2 dir = target != null
            ? ((Vector2)target.position - (Vector2)transform.position).normalized
            : Vector2.right;

        int dmg = PlayerStats.Instance != null ? Mathf.Max(1, PlayerStats.Instance.damage / 3) : 5;

        var go = new GameObject("IceOrb");
        go.transform.position   = transform.position;
        go.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = orbSprite;
        sr.color        = new Color(0.7f, 0.95f, 1f, 1f);
        sr.sortingOrder = 4;

        var proj = go.AddComponent<IceOrbProjectile>();
        proj.direction      = dir;
        proj.speed          = projectileSpeed;
        proj.damageInterval = damageInterval;
        proj.damageRange    = damageRange;
        proj.damage         = dmg;
        proj.lifetime       = lifetime;
    }

    Transform FindNearestEnemy()
    {
        Transform nearest = null;
        float     minDist = float.MaxValue;
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist) { minDist = d; nearest = e.transform; }
        }
        return nearest;
    }
}

public class IceOrbProjectile : MonoBehaviour
{
    public Vector2 direction;
    public float   speed          = 3f;
    public float   damageInterval = 0.6f;
    public float   damageRange    = 1.5f;
    public int     damage         = 5;
    public float   lifetime       = 4f;

    private float lifeTimer;
    private float damageTimer;

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.Rotate(0f, 0f, 360f * Time.deltaTime);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime) { Destroy(gameObject); return; }

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            foreach (var col in Physics2D.OverlapCircleAll(transform.position, damageRange))
            {
                if (!col.CompareTag("Enemy")) continue;
                col.GetComponent<Enemy>()?.TakeDamage(damage);
                col.GetComponent<BossEnemy>()?.TakeDamage(damage);
            }
        }
    }
}
