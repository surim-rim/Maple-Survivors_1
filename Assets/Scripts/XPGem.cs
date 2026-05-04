using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpValue = 50;
    public float attractSpeed = 8f;

    private Transform player;
    private Rigidbody2D rb;
    private bool attracting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        float radius = PlayerStats.Instance != null ? PlayerStats.Instance.gemPickupRadius : 3f;

        if (!attracting && Vector2.Distance(rb.position, (Vector2)player.position) <= radius)
            attracting = true;

        if (attracting)
            rb.MovePosition(Vector2.MoveTowards(rb.position, player.position, attractSpeed * Time.fixedDeltaTime));
    }

    public void AttractNow() => attracting = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats.Instance?.AddXP(xpValue);
            Destroy(gameObject);
        }
    }
}
