using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;      // grace time after leaving a ledge
    [SerializeField] private float jumpBufferTime = 0.1f;  // grace time for pressing jump slightly early
    [SerializeField] private float lowJumpMultiplier = 2f; // shorter hop when releasing jump early

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool hasJumped; // Ny variabel til at holde styr på, om vi allerede har hoppet

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // Stops the player from tilting 
    }

    private void Update()
    {
        // 1. SIKRING MOD FEJL: Hvis du glemmer at trække GroundCheck ind i Inspectoren, stopper vi her
        if (groundCheck == null)
        {
            Debug.LogError("Husk at trække dit GroundCheck-objekt ind i Inspectoren på " + gameObject.name);
            return;
        }

        // Left/right input (A/D or arrow keys)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Håndtering af Coyote Time
        if (isGrounded)
        {
            // Vi nulstiller kun timeren, hvis vi ikke lige har trykket på hop
            if (!hasJumped)
            {
                coyoteTimer = coyoteTime;
            }
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            hasJumped = false; // Når vi er i luften, nulstilles låsen, så vi kan hoppe næste gang vi lander
        }

        // Jump buffer: remember a jump press for a short moment
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Perform jump
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            hasJumped = true; // Låser for uendelige hop i samme frame
        }

        // Variable jump height: cut the jump short if the button is released
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / lowJumpMultiplier);
        }

        // Flip sprite to face movement direction
        if (moveInput > 0f) transform.localScale = new Vector3(1f, 1f, 1f);
        else if (moveInput < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // Shows the ground check circle in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}