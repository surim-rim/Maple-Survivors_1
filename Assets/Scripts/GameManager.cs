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
    public GameObject bossPrefab;
    private float nextBossTime = 60f;

    void Awake() => Instance = this;

    void Update()
    {
        if (CurrentState != State.Playing) return;

        ElapsedTime += Time.deltaTime;

        if (bossPrefab != null && ElapsedTime >= nextBossTime)
        {
            SpawnBoss();
            nextBossTime += 60f;
        }
    }

    void SpawnBoss()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 15f;
        Instantiate(bossPrefab, player.transform.position + (Vector3)offset, Quaternion.identity);
        Debug.Log("[GameManager] 보스 스폰!");
    }

    public void OnPlayerDied()
    {
        CurrentState = State.GameOver;
        Time.timeScale = 0f;
        GameUI.Instance?.ShowGameOver();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
