using UnityEngine;

public class RandomBox : MonoBehaviour
{
    public GameObject[] itemPrefabs;

    public void TakeDamage(int dmg)
    {
        SpawnItem();
        Destroy(gameObject);
    }

    void SpawnItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;
        int idx = Random.Range(0, itemPrefabs.Length);
        Instantiate(itemPrefabs[idx], transform.position, Quaternion.identity);
    }
}
