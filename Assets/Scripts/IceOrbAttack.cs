using UnityEngine;

public class IceOrbAttack : MonoBehaviour
{
    public float  attackInterval  = 2f;
    public float  projectileSpeed = 3f;   // 천천히 날아감
    public float  damageInterval  = 0.6f;
    public float  damageRange     = 1.5f;
    public float  lifetime        = 4f;
    public Sprite orbSprite;
    public int    weaponLevel      = 1;
    public int    orbCount         = 1;
    public float  damageMultiplier = 1f;
    public float  orbScale         = 0.35f;
    public int    extraAttackCount = 0;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            FireOrbs();
        }
    }

    void FireOrbs()
    {
        var target = FindNearestEnemy();
        Vector2 baseDir = target != null
            ? ((Vector2)target.position - (Vector2)transform.position).normalized
            : Vector2.right;

        int dmg = PlayerStats.Instance != null
            ? Mathf.Max(1, (int)(PlayerStats.Instance.damage / 2f * damageMultiplier))
            : 5;

        int   total       = orbCount + extraAttackCount;
        float spreadAngle = total > 1 ? 20f : 0f;
        float startAngle  = -(total - 1) * spreadAngle / 2f;

        for (int i = 0; i < total; i++)
        {
            float   angle = startAngle + i * spreadAngle;
            Vector2 dir   = Quaternion.Euler(0f, 0f, angle) * baseDir;
            SpawnOrb(dir, dmg);
        }
    }

    void SpawnOrb(Vector2 dir, int dmg)
    {
        var go = new GameObject("IceOrb");
        go.transform.position   = transform.position;
        go.transform.localScale = new Vector3(orbScale, orbScale, 1f);

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
                if (col.CompareTag("Enemy"))
                {
                    col.GetComponent<Enemy>()?.TakeDamage(damage);
                    col.GetComponent<BossEnemy>()?.TakeDamage(damage);
                }
                else if (col.CompareTag("Box"))
                    col.GetComponent<RandomBox>()?.TakeDamage(damage);
            }
        }
    }
}
