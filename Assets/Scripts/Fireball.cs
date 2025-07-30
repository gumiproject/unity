using UnityEngine;

[RequireComponent(typeof(AudioSource))] // AudioSource 컴포넌트가 항상 있도록 보장
public class Fireball : MonoBehaviour
{
    [Header("설정")]
    public float forwardSpeed = 8f;
    public float upwardForce = 6f;
    public float lifetime = 3f;
    public int maxBounces = 1;
    [Range(0, 1)]
    public float bounceFactor = 0.8f;

    [Header("사운드")]
    public AudioClip launchSound; // 발사 시 재생할 사운드

    // --- Private 변수 ---
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private int bounceCount = 0;
    private Vector2 lastVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트를 찾음
    }

    void Update()
    {
        lastVelocity = rb.linearVelocity;
    }

    public void Launch(bool isFacingRight)
    {
        // [추가] 발사 시 사운드 재생
        if (launchSound != null)
        {
            audioSource.PlayOneShot(launchSound);
        }

        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 initialVelocity = (direction * forwardSpeed) + (Vector2.up * upwardForce);
        rb.linearVelocity = initialVelocity;

        if (!isFacingRight)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
            return;
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && bounceCount < maxBounces)
        {
            bounceCount++;
            Vector2 surfaceNormal = collision.contacts[0].normal;
            Vector2 reflectDirection = Vector2.Reflect(lastVelocity.normalized, surfaceNormal);
            rb.linearVelocity = reflectDirection * lastVelocity.magnitude * bounceFactor;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}