using UnityEngine;

/// <summary>
/// Converts raw world data into fuzzy vision (V) and noise (R) values.
/// Vision:  0.0=Null | 0.5=Partial | 1.0=Clear
/// Noise:   0.0=Silent | 0.5=Light | 1.0=Loud
/// </summary>
public class EnemySensor : MonoBehaviour
{
    // ── Inspector – Vision ─────────────────────────────────────────────────────
    [Header("Vision")]
    [SerializeField] private Transform eyePoint;
    [SerializeField, Range(1f, 50f)] public float visionRange = 20f;
    [SerializeField, Range(10f, 180f)] public float visionAngle = 90f;
    [SerializeField, Range(0f, 1f)] private float peripheralFraction = 0.5f;
    [Tooltip("Angle beyond which vision is considered peripheral (0.5).")]
    [SerializeField, Range(0f, 90f)] private float peripheralAngle = 60f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;

    // ── Inspector – Hearing ────────────────────────────────────────────────────
    [Header("Hearing")]
    [SerializeField] public float hearingRadiusWalk = 10f;
    [SerializeField] public float hearingRadiusRun = 20f;
    [Tooltip("Layer mask for the player object.")]
    [SerializeField] private LayerMask playerLayerHearing;

    // ── Inspector – Debug ──────────────────────────────────────────────────────
    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    // ── Cached References ──────────────────────────────────────────────────────
    private Transform _playerTransform;
    private PlayerController _playerController; // Expects a PlayerController component on Player

    // ── Output Cache ───────────────────────────────────────────────────────────
    public float LastVisionValue { get; private set; }
    public float LastNoiseValue { get; private set; }

    // ── Unity ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (eyePoint == null) eyePoint = transform;

        // Find player — tag-based so no hard scene reference needed
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("[EnemySensor] No GameObject with tag 'Player' found.");
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    public float GetVisionValue()
    {
        LastVisionValue = ComputeVision();
        return LastVisionValue;
    }

    public float GetNoiseValue()
    {
        LastNoiseValue = ComputeNoise();
        return LastNoiseValue;
    }

    /// <summary>Returns the player's last known position (used by AIController for LKP).</summary>
    public Vector3 GetPlayerPosition() =>
        _playerTransform != null ? _playerTransform.position : Vector3.zero;

    // ── Vision Computation ─────────────────────────────────────────────────────
    private float ComputeVision()
    {
        if (_playerTransform == null) return 0f;

        Vector3 toPlayer = _playerTransform.position - eyePoint.position;
        float distance = toPlayer.magnitude;

        if (distance > visionRange) return 0f;

        float angle = Vector3.Angle(eyePoint.forward, toPlayer);

        // Outside full FOV cone
        if (angle > visionAngle * 0.5f) return 0f;

        // Raycast for line-of-sight
        if (!HasLineOfSight(toPlayer, distance)) return 0f;

        // Peripheral zone → Partial
        if (angle > peripheralAngle * 0.5f) return 0.5f;

        // Far end of range → Partial
        if (distance > visionRange * peripheralFraction) return 0.5f;

        // Clear direct sight
        return 1f;
    }

    private bool HasLineOfSight(Vector3 toPlayer, float distance)
    {
        Ray ray = new Ray(eyePoint.position, toPlayer.normalized);
        return !Physics.Raycast(ray, distance, obstacleLayer);
    }

    // ── Noise Computation ──────────────────────────────────────────────────────
    private float ComputeNoise()
    {
        if (_playerTransform == null) return 0f;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        PlayerController.MoveState moveState = _playerController != null
            ? _playerController.CurrentMoveState
            : PlayerController.MoveState.Idle;

        // Crouched / Idle → Silent
        if (moveState == PlayerController.MoveState.Idle ||
            moveState == PlayerController.MoveState.Crouch)
            return 0f;

        if (moveState == PlayerController.MoveState.Run)
        {
            // Running close → Loud; running far → Light if still in hearing range
            if (dist <= hearingRadiusWalk) return 1f;   // very close run
            if (dist <= hearingRadiusRun) return 1f;   // run heard from afar
            return 0f;
        }

        if (moveState == PlayerController.MoveState.Walk)
        {
            if (dist <= hearingRadiusWalk) return 0.5f; // walking close → Light
            return 0f;
        }

        // Item drop or other loud events handled via DropNoise()
        return 0f;
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Transform origin = eyePoint != null ? eyePoint : transform;

        // Vision cone
        Gizmos.color = Color.yellow;
        DrawCone(origin, visionRange, visionAngle);

        // Peripheral cone
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        DrawCone(origin, visionRange, peripheralAngle);

        // Hearing radii
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hearingRadiusWalk);
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, hearingRadiusRun);
    }

    private void DrawCone(Transform origin, float range, float angle)
    {
        int steps = 20;
        float halfAngle = angle * 0.5f;
        float stepSize = angle / steps;

        Vector3 prev = origin.position +
            Quaternion.Euler(0f, -halfAngle, 0f) * origin.forward * range;

        for (int i = 1; i <= steps; i++)
        {
            float a = -halfAngle + stepSize * i;
            Vector3 next = origin.position +
                Quaternion.Euler(0f, a, 0f) * origin.forward * range;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
        Gizmos.DrawLine(origin.position,
            origin.position + Quaternion.Euler(0f, -halfAngle, 0f) * origin.forward * range);
        Gizmos.DrawLine(origin.position,
            origin.position + Quaternion.Euler(0f, halfAngle, 0f) * origin.forward * range);
    }
}
