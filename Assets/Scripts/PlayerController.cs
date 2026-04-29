using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHP = 100;

    [Header("Map Boundary")]
    public Vector2 mapMin = new Vector2(-75f, -75f);
    public Vector2 mapMax = new Vector2( 75f,  75f);

    [Header("Animation")]
    public Sprite[] rightSprites;
    public Sprite[] leftSprites;
    public float animFrameRate = 6f;
    [HideInInspector] public bool useFlipX = false; // 별도 left 스프라이트 없을 때 flipX로 방향 표현

    public int CurrentHP { get; private set; }

    private Vector2 lastFacingDir = Vector2.right;
    public Vector2 FacingDir => lastFacingDir;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movement;
    private float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    private float animTimer;
    private int animFrame;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        CurrentHP = maxHP;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (movement.x > 0) facingRight = true;
        else if (movement.x < 0) facingRight = false;

        if (movement != Vector2.zero) lastFacingDir = movement;

        AnimateSprite();
    }

    void FixedUpdate()
    {
        Vector2 next = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        next.x = Mathf.Clamp(next.x, mapMin.x, mapMax.x);
        next.y = Mathf.Clamp(next.y, mapMin.y, mapMax.y);
        rb.MovePosition(next);
    }

    void AnimateSprite()
    {
        Sprite[] frames;
        if (useFlipX)
        {
            frames = rightSprites;
            if (sr != null) sr.flipX = !facingRight;
        }
        else
        {
            frames = facingRight ? rightSprites : leftSprites;
        }
        if (frames == null || frames.Length == 0) return;

        if (movement != Vector2.zero)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animFrameRate)
            {
                animTimer = 0f;
                animFrame = (animFrame + 1) % frames.Length;
            }
        }
        else
        {
            animFrame = 0;
        }

        if (sr != null) sr.sprite = frames[animFrame];
    }

    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        CurrentHP -= damage;
        if (CurrentHP <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, maxHP);
    }

    void Die()
    {
        gameObject.SetActive(false);
        GameManager.Instance?.OnPlayerDied();
    }
}
