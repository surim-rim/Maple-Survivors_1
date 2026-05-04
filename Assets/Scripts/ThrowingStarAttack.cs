using UnityEngine;
using System.Collections;

public class ThrowingStarAttack : MonoBehaviour
{
    public float  attackInterval  = 2f;
    public int    burstCount      = 2;
    public float  burstDelay      = 0.12f;
    public float  projectileSpeed = 16f;
    public Sprite starSprite;
    public int    weaponLevel     = 1;
    public float  starScale       = 0.2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            var target = FindNearestEnemy();
            if (target != null)
                StartCoroutine(BurstFire(target));
        }
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

    IEnumerator BurstFire(Transform target)
    {
        int dmg = PlayerStats.Instance != null ? PlayerStats.Instance.damage : 15;
        // 첫 발 기준으로 방향을 고정 — 도중 타겟이 사라져도 방향 유지
        Vector2 dir = target != null
            ? ((Vector2)target.position - (Vector2)transform.position).normalized
            : Vector2.right;

        for (int i = 0; i < burstCount; i++)
        {
            SpawnStar(dir, dmg);
            yield return new WaitForSeconds(burstDelay);
        }
    }

    void SpawnStar(Vector2 dir, int dmg)
    {
        var go = new GameObject("ThrowingStar");
        go.transform.position   = transform.position;
        go.transform.localScale = new Vector3(starScale, starScale, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = starSprite;
        sr.sortingOrder = 4;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.4f;

        var proj = go.AddComponent<StarProjectile>();
        proj.direction = dir;
        proj.speed     = projectileSpeed;
        proj.damage    = dmg;
    }
}

public class StarProjectile : MonoBehaviour
{
    public Vector2 direction;
    public float   speed    = 16f;
    public int     damage   = 15;
    public float   lifetime = 3f;

    private float timer;

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.Rotate(0f, 0f, 720f * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            col.GetComponent<Enemy>()?.TakeDamage(damage);
            col.GetComponent<BossEnemy>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (col.CompareTag("Box"))
        {
            col.GetComponent<RandomBox>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
