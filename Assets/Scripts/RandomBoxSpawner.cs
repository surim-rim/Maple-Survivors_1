using UnityEngine;

public class RandomBoxSpawner : MonoBehaviour
{
    public GameObject boxPrefab;
    public float spawnInterval = 20f;
    public float minDist  = 8f;
    public float maxDist  = 14f;
    public int   maxBoxes = 5;
    public float mapBound = 74f;

    private float     timer;
    private Transform player;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        timer = spawnInterval * 0.5f;
    }

    void Update()
    {
        if (player == null || boxPrefab == null) return;
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;
        if (FindObjectsOfType<RandomBox>().Length < maxBoxes)
            SpawnBox();
    }

    void SpawnBox()
    {
        float   angle    = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float   dist     = Random.Range(minDist, maxDist);
        Vector2 offset   = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        Vector3 spawnPos = player.position + (Vector3)offset;
        spawnPos.x = Mathf.Clamp(spawnPos.x, -mapBound, mapBound);
        spawnPos.y = Mathf.Clamp(spawnPos.y, -mapBound, mapBound);
        Instantiate(boxPrefab, spawnPos, Quaternion.identity);
    }
}
