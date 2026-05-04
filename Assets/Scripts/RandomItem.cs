using UnityEngine;

public class RandomItem : MonoBehaviour
{
    public enum ItemType { Heal, Magnet, Gold1, Gold2, Gold3, Gold4 }
    public ItemType itemType;

    private float       attractSpeed = 8f;
    private Transform   player;
    private Rigidbody2D rb;
    private bool        attracting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;
        float radius = PlayerStats.Instance != null ? PlayerStats.Instance.gemPickupRadius : 3f;
        if (!attracting && Vector2.Distance(rb.position, player.position) <= radius)
            attracting = true;
        if (attracting)
            rb.MovePosition(Vector2.MoveTowards(rb.position, player.position, attractSpeed * Time.fixedDeltaTime));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Apply();
        Destroy(gameObject);
    }

    void Apply()
    {
        var pc = FindObjectOfType<PlayerController>();
        var ps = PlayerStats.Instance;
        switch (itemType)
        {
            case ItemType.Heal:
                if (pc != null) pc.Heal(Mathf.RoundToInt(pc.maxHP * 0.2f));
                break;
            case ItemType.Magnet:
                foreach (var gem in FindObjectsOfType<XPGem>())
                    gem.AttractNow();
                break;
            case ItemType.Gold1: if (ps != null) ps.gold += 10;  break;
            case ItemType.Gold2: if (ps != null) ps.gold += 30;  break;
            case ItemType.Gold3: if (ps != null) ps.gold += 70;  break;
            case ItemType.Gold4: if (ps != null) ps.gold += 150; break;
        }
    }
}
