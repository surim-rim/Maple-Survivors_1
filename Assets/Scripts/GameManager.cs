using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State { Playing, GameOver }
    public State CurrentState { get; private set; } = State.Playing;

    public float ElapsedTime { get; private set; }

    // 시간 기반 난이도 배율 (Enemy.Start()에서 읽음)
    public float EnemyHPMultiplier    => 1f + ElapsedTime / 60f;  // 1분마다 +100%
    public float EnemySpeedMultiplier => 1f + ElapsedTime / 120f; // 2분마다 +100%

    [Header("Boss")]
    public GameObject[] bossPrefabs;
    public float mapBound      = 74f;
    private int   bossIndex    = 0;
    private float nextBossTime = 180f;
    private bool  lastBossSpawned   = false;
    private float lastBossSpawnTime = 0f;

    void Awake() => Instance = this;

    void Update()
    {
        if (CurrentState != State.Playing) return;

        ElapsedTime += Time.deltaTime;

        if (bossPrefabs != null && bossIndex < bossPrefabs.Length && ElapsedTime >= nextBossTime)
        {
            SpawnBoss();
            bossIndex++;
            nextBossTime += 180f;
            if (bossIndex >= bossPrefabs.Length)
            {
                lastBossSpawned   = true;
                lastBossSpawnTime = ElapsedTime;
            }
        }

        if (lastBossSpawned && ElapsedTime >= lastBossSpawnTime + 60f)
        {
            lastBossSpawned = false;
            OnGameClear();
        }
    }

    void SpawnBoss()
    {
        if (bossPrefabs == null || bossIndex >= bossPrefabs.Length) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 15f;
        Vector3 spawnPos = player.transform.position + (Vector3)offset;
        spawnPos.x = Mathf.Clamp(spawnPos.x, -mapBound, mapBound);
        spawnPos.y = Mathf.Clamp(spawnPos.y, -mapBound, mapBound);
        Instantiate(bossPrefabs[bossIndex], spawnPos, Quaternion.identity);
        Debug.Log($"[GameManager] 보스 {bossIndex + 1} 스폰!");
    }

    public void OnPlayerDied()
    {
        CurrentState = State.GameOver;
        Time.timeScale = 0f;
        GameUI.Instance?.ShowGameOver();
    }

    public void OnGameClear()
    {
        CurrentState = State.GameOver;
        Time.timeScale = 0f;
        GameUI.Instance?.ShowGameClear();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
