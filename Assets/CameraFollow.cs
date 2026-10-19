using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Distance")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 7f;
    [SerializeField] private float height = 4f;

    [Header("Mouse Orbit Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minYAngle = -10f;
    [SerializeField] private float maxYAngle = 60f;

    [Header("Smoothing (Sharpness)")]
    [SerializeField] private float sharpness = 5f; // Exponential smoothing[cite: 4]

    private float yaw = 0f;
    private float pitch = 20f;
    private Rigidbody targetRb;

    void Start()
    {
        if (target == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }

        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
        }

        LockCursor();
    }

    void Update()
    {
        // Toggle cursor lock with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
        }
    }

    void LateUpdate() // Must run in LateUpdate[cite: 4]
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Frame-rate independent exponential smoothing: 1 - e^(-k * dt)[cite: 4]
        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);

        transform.LookAt(targetPosition);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}