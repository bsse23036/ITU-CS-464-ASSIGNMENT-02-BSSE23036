using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.8f; // metres

    [Header("Fairness Windows")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float fallMultiplier = 2.2f;

    [Header("Ground Check & Juice")]
    [SerializeField] private Transform feet;
    [SerializeField] private float checkRadius = 0.35f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform bodyMesh;

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera; // Explicitly assign your camera here if needed

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;

    private float lastGroundedTime;
    private float lastPressedTime;
    private float airborneTimer;
    private bool wasGroundedLastFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Find main camera if not explicitly assigned in inspector
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindAnyObjectByType<Camera>();
            }
        }
    }

    void Update()
    {
        // Fallback check in case camera was instantiated later
        if (mainCamera == null) mainCamera = Camera.main;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // 1. Calculate camera-relative movement directions
        if (mainCamera != null)
        {
            // Get camera forward and right directions
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;

            // Zero out vertical component so tilting camera doesn't slow movement or push into ground
            camForward.y = 0f;
            camRight.y = 0f;

            // Normalize to keep movement speed consistent regardless of camera angle
            camForward.Normalize();
            camRight.Normalize();

            // Project horizontal & vertical inputs onto camera's orientation
            moveInput = (camForward * moveZ + camRight * moveX);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        }
        else
        {
            // Fallback to absolute world coordinates if camera is missing
            moveInput = new Vector3(moveX, 0f, moveZ).normalized;
        }

        // 2. Track jump input press for buffering
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastPressedTime = Time.time;
        }

        // Track airborne timer for squash/stretch juice
        if (!isGrounded) airborneTimer += Time.deltaTime;
        else airborneTimer = 0f;
    }

    void FixedUpdate()
    {
        // Ground check
        bool groundedThisFrame = Physics.CheckSphere(feet.position, checkRadius, groundMask);

        if (groundedThisFrame)
        {
            lastGroundedTime = Time.time;
            
            // Trigger landing juice if airborne > 0.1s
            if (!wasGroundedLastFrame && airborneTimer > 0.1f)
            {
                StartCoroutine(LandSquash());
            }
        }
        isGrounded = groundedThisFrame;
        wasGroundedLastFrame = isGrounded;

        // Apply camera-relative velocity while keeping vertical velocity intact
        Vector3 currentVel = rb.linearVelocity;
        Vector3 targetVelocity = moveInput * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, currentVel.y, targetVelocity.z);

        // Check Coyote Time and Jump Buffer conditions
        bool canJump = (Time.time - lastGroundedTime) <= coyoteTime;
        bool wantJump = (Time.time - lastPressedTime) <= jumpBuffer;

        if (canJump && wantJump)
        {
            float g = Mathf.Abs(Physics.gravity.y);
            float v0 = Mathf.Sqrt(2f * g * jumpHeight); // v0 = sqrt(2gh)

            Vector3 vel = rb.linearVelocity;
            vel.y = 0f; // Reset downward velocity before applying impulse
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);

            lastGroundedTime = -1f;
            lastPressedTime = -1f;
        }

        // Apply fall gravity multiplier for a tight, snappy descent
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private IEnumerator LandSquash()
    {
        if (bodyMesh == null) yield break;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
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