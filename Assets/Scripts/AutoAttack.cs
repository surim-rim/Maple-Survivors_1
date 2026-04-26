using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float attackInterval = 0.8f;
    public float attackRange = 10f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < attackInterval) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        timer = 0f;
        Fire(target);
    }

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = attackRange;

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e.transform;
            }
        }
        return nearest;
    }

    void Fire(Transform target)
    {
        if (projectilePrefab == null) return;
        Vector2 dir = (target.position - transform.position).normalized;
        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Projectile>()?.Init(dir);
    }
}
