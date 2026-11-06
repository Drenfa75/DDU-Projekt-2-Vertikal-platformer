using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 2f;

    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpPressed;
    private bool onRamp;
    private Vector2 rampNormal;

    private readonly HashSet<Collider2D> groundContacts = new();

    private bool isGrounded => groundContacts.Count > 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        if (jumpPressed)
        {
            if (isGrounded)
            {
                // Set a consistent upward jump speed.
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                groundContacts.Clear();
            }

            jumpPressed = false;
        }

        // Don't add ramp acceleration during a jump.
        if (onRamp && isGrounded)
        {
            Vector2 gravityAlongRamp =
                Physics2D.gravity -
                Vector2.Dot(Physics2D.gravity, rampNormal) * rampNormal;

            rb.AddForce(
                gravityAlongRamp * (speedMultiplier - 1f),
                ForceMode2D.Force
            );
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateContact(collision);
    }

    private void UpdateContact(Collision2D collision)
    {
        Collider2D other = collision.collider;

        if (!other.CompareTag("Ground") && !other.CompareTag("Ramp"))
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            // Ignore wall/underside contacts; count only surfaces beneath the player.
            if (Vector2.Dot(contact.normal, Vector2.up) > 0f)
            {
                groundContacts.Add(other);

                if (other.CompareTag("Ramp"))
                {
                    onRamp = true;
                    rampNormal = contact.normal;
                }

                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Collider2D other = collision.collider;

        groundContacts.Remove(other);

        if (other.CompareTag("Ramp"))
            onRamp = false;
    }
}
