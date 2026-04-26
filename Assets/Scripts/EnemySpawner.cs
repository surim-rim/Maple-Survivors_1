using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRadius = 13f;
    public float initialInterval = 1.5f;
    public float minInterval = 0.3f;
    public int maxEnemies = 150;

    private float timer;
    private float currentInterval;
    private Transform player;

    void Start()
    {
        currentInterval = initialInterval;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null || enemyPrefab == null) return;

        // 시간이 지날수록 스폰 간격 감소 (난이도 상승)
        currentInterval = Mathf.Max(minInterval, initialInterval - Time.time * 0.02f);

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
        Instantiate(enemyPrefab, player.position + (Vector3)offset, Quaternion.identity);
    }
}
