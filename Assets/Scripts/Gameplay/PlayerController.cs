using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.8f;

    private Rigidbody2D body;
    private Vector2 mobileDirection;
    private Vector3 spawnPosition;
    private bool inputEnabled = true;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        spawnPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!inputEnabled || Time.timeScale <= 0f)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 direction = keyboard.sqrMagnitude > 0.01f ? keyboard.normalized : mobileDirection.normalized;
        body.linearVelocity = direction * moveSpeed;
    }

    public void SetMobileDirection(Vector2 direction)
    {
        mobileDirection = direction;
    }

    public void ClearMobileDirection(Vector2 direction)
    {
        if (mobileDirection == direction) mobileDirection = Vector2.zero;
    }

    public void SetSpawn(Vector3 position)
    {
        spawnPosition = position;
        transform.position = position;
    }

    public void Respawn()
    {
        body.linearVelocity = Vector2.zero;
        transform.position = spawnPosition;
    }

    public void SetInputEnabled(bool value)
    {
        inputEnabled = value;
        if (!value && body != null) body.linearVelocity = Vector2.zero;
    }
}

