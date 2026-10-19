using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.8f;

    [Header("Fairness Windows")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float fallMultiplier = 2.2f;

    [Header("Ground Check & Juice")]
    [SerializeField] private Transform feet;
    [SerializeField] private float checkRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform bodyMesh; // Drag your child "Body" object here

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private Camera cam;

    // Timestamps for fairness windows
    private float lastGroundedTime;
    private float lastPressedTime;
    private float airborneTimer;
    private bool wasGroundedLastFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    void Update()
    {
        // 1. Camera-relative movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            moveInput = (camForward * moveZ + camRight * moveX);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        }
        else
        {
            moveInput = new Vector3(moveX, 0f, moveZ);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        }

        // 2. Track jump press for the Jump Buffer window
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastPressedTime = Time.time;
        }

        // Track airborne time for juice condition
        if (!isGrounded) airborneTimer += Time.deltaTime;
        else airborneTimer = 0f;
    }

    void FixedUpdate()
    {
        // Ground check
        bool groundedThisFrame = Physics.CheckSphere(feet.position, checkRadius, groundMask);

        // Track coyote time
        if (groundedThisFrame)
        {
            lastGroundedTime = Time.time;
            
            // Trigger juice on real landing (if airborne for more than 0.1s)
            if (!wasGroundedLastFrame && airborneTimer > 0.1f)
            {
                StartCoroutine(LandSquash());
            }
        }
        isGrounded = groundedThisFrame;
        wasGroundedLastFrame = isGrounded;

        // Apply horizontal movement, keeping vertical velocity
        Vector3 targetVelocity = new Vector3(moveInput.x * speed, rb.linearVelocity.y, moveInput.z * speed);
        rb.linearVelocity = targetVelocity;

        // Check if both Coyote Time and Jump Buffer conditions are met
        bool canJump = (Time.time - lastGroundedTime) <= coyoteTime;
        bool wantJump = (Time.time - lastPressedTime) <= jumpBuffer;

        if (canJump && wantJump)
        {
            float g = Mathf.Abs(Physics.gravity.y);
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);

            Vector3 vel = rb.linearVelocity;
            vel.y = 0f; // Clear downward velocity before jumping
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);

            // Reset windows so it doesn't double-jump
            lastGroundedTime = -1f;
            lastPressedTime = -1f;
        }

        // Apply extra fall gravity for a snappier descent
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    // Simple landing squash coroutine (Game Juice)
    private IEnumerator LandSquash()
    {
        if (bodyMesh == null) yield break;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            
            // Quick squash effect: flatten Y, widen X/Z slightly
            float squashY = Mathf.Lerp(1.2f, 1f, p);
            float scaleXZ = 1f / Mathf.Sqrt(squashY);
            
            bodyMesh.localScale = new Vector3(scaleXZ, squashY, scaleXZ);
            yield return null;
        }
        bodyMesh.localScale = Vector3.one;
    }

    private void OnDrawGizmosSelected()
    {
        if (feet != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(feet.position, checkRadius);
        }
    }
}