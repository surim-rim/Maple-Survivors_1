using UnityEngine;

public class BowAttack : MonoBehaviour
{
    public float  attackInterval  = 1.2f;
    public float  projectileSpeed = 14f;
    public Sprite arrowSprite;
    public int    weaponLevel     = 1;

    private float            timer;
    private PlayerController pc;

    void Start() => pc = GetComponent<PlayerController>();

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            FireArrow();
        }
    }

    void FireArrow()
    {
        Vector2 baseDir = pc != null ? pc.FacingDir : Vector2.right;
        int     dmg     = PlayerStats.Instance != null ? PlayerStats.Instance.damage : 20;

        float spreadAngle = 20f;
        float totalSpread = (weaponLevel - 1) * spreadAngle;
        float startAngle  = -totalSpread / 2f;

        for (int i = 0; i < weaponLevel; i++)
        {
            float   angle = startAngle + i * spreadAngle;
            Vector2 dir   = Quaternion.Euler(0f, 0f, angle) * baseDir;
            SpawnArrow(dir, dmg);
        }
    }

    void SpawnArrow(Vector2 dir, int dmg)
    {
        var go = new GameObject("BowArrow");
        go.transform.position   = transform.position;
        go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        go.transform.rotation   = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 50f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = arrowSprite;
        sr.sortingOrder = 4;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.3f;

        var proj = go.AddComponent<BowProjectile>();
        proj.direction = dir;
        proj.speed     = projectileSpeed;
        proj.damage    = dmg;
    }
}

public class BowProjectile : MonoBehaviour
{
    public Vector2 direction;
    public float   speed    = 14f;
    public int     damage   = 20;
    public float   lifetime = 4f;

    private float timer;

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;
        col.GetComponent<Enemy>()?.TakeDamage(damage);
        col.GetComponent<BossEnemy>()?.TakeDamage(damage);
        Destroy(gameObject);
    }
}
