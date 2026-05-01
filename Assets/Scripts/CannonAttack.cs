using UnityEngine;

public class CannonAttack : MonoBehaviour
{
    public float  attackInterval  = 3f;
    public float  attackRange     = 8f;
    public float  explosionRadius = 2.5f;
    public float  burnDuration    = 5f;
    public float  burnInterval    = 0.5f;
    public Sprite cannonballSprite;
    public int    weaponLevel      = 1;
    public float  damageMultiplier = 1f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            Fire();
        }
    }

    void Fire()
    {
        // 사정거리 내 랜덤 위치에 발사
        Vector2 offset    = Random.insideUnitCircle.normalized * Random.Range(2.2f, attackRange);
        Vector2 targetPos = (Vector2)transform.position + offset;

        int dmg    = PlayerStats.Instance != null ? (int)(PlayerStats.Instance.damage * 2 * damageMultiplier) : 40;
        int dotDmg = PlayerStats.Instance != null ? Mathf.Max(1, PlayerStats.Instance.damage / 2) : 10;

        var go = new GameObject("Cannonball");
        go.transform.position   = transform.position;
        go.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = cannonballSprite;
        sr.sortingOrder = 5;

        var proj = go.AddComponent<CannonProjectile>();
        proj.targetPos       = targetPos;
        proj.explosionRadius = explosionRadius;
        proj.burnDuration    = burnDuration;
        proj.burnInterval    = burnInterval;
        proj.damage          = dmg;
        proj.dotDamage       = dotDmg;
    }
}

public class CannonProjectile : MonoBehaviour
{
    public Vector2 targetPos;
    public float   travelTime    = 0.8f;
    public float   arcHeight     = 2.5f;
    public int     damage;
    public int     dotDamage;
    public float   explosionRadius;
    public float   burnDuration;
    public float   burnInterval;

    private Vector2 startPos;
    private float   timer;
    private bool    landed;

    void Start() => startPos = transform.position;

    void Update()
    {
        if (landed) return;
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / travelTime);

        Vector2 pos = Vector2.Lerp(startPos, targetPos, t);
        float   arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        transform.position = new Vector3(pos.x, pos.y + arc, 0f);
        transform.Rotate(0f, 0f, 400f * Time.deltaTime);

        if (t >= 1f) Land();
    }

    void Land()
    {
        landed = true;
        transform.position = new Vector3(targetPos.x, targetPos.y, 0f);

        // 즉발 폭발 데미지
        foreach (var col in Physics2D.OverlapCircleAll(targetPos, explosionRadius))
        {
            if (col.CompareTag("Enemy"))
            {
                col.GetComponent<Enemy>()?.TakeDamage(damage);
                col.GetComponent<BossEnemy>()?.TakeDamage(damage);
            }
            else if (col.CompareTag("Box"))
                col.GetComponent<RandomBox>()?.TakeDamage(damage);
        }

        // 화염 지역 생성
        var burnGO = new GameObject("BurnZone");
        burnGO.transform.position = targetPos;
        var bz = burnGO.AddComponent<BurnZone>();
        bz.radius         = explosionRadius;
        bz.duration       = burnDuration;
        bz.damageInterval = burnInterval;
        bz.dotDamage      = dotDamage;

        Destroy(gameObject);
    }
}

public class BurnZone : MonoBehaviour
{
    public float radius;
    public float duration;
    public float damageInterval;
    public int   dotDamage;

    private float          lifeTimer;
    private float          damageTimer;
    private SpriteRenderer sr;

    void Start()
    {
        // 원형 텍스처 생성
        int   size   = 64;
        float center = size / 2f;
        var   tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist <= center ? Color.white : Color.clear);
            }
        tex.Apply();

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite            = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sr.color             = new Color(1f, 0.4f, 0f, 0.4f);
        sr.sortingOrder      = 1;
        transform.localScale = Vector3.one * radius * 2f;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= duration) { Destroy(gameObject); return; }

        // 시간에 따라 점점 투명해짐
        if (sr != null)
            sr.color = new Color(1f, 0.4f, 0f, 0.4f * (1f - lifeTimer / duration));

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            foreach (var col in Physics2D.OverlapCircleAll(transform.position, radius))
            {
                if (col.CompareTag("Enemy"))
                {
                    col.GetComponent<Enemy>()?.TakeDamage(dotDamage);
                    col.GetComponent<BossEnemy>()?.TakeDamage(dotDamage);
                }
                else if (col.CompareTag("Box"))
                    col.GetComponent<RandomBox>()?.TakeDamage(dotDamage);
            }
        }
    }
}
