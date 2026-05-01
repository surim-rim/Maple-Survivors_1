using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
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
        if (player == null || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // 시간이 지날수록 스폰 간격 감소 (난이도 상승)
        float elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
        currentInterval = Mathf.Max(minInterval, initialInterval - elapsed * 0.02f);

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
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, player.position + (Vector3)offset, Quaternion.identity);
    }
}
