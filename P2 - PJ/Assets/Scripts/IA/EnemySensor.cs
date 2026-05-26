using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] private Transform eyePoint;
    [SerializeField, Range(1f, 50f)] public float visionRange = 20f;
    [SerializeField, Range(10f, 180f)] public float visionAngle = 90f;
    [SerializeField, Range(0f, 1f)] private float peripheralFraction = 0.5f;
    [Tooltip("Recomendado 0.5.")]
    [SerializeField, Range(0f, 180f)] public float peripheralAngle = 60f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Hearing")]
    [SerializeField] public float hearingRadiusWalk = 5f;
    [SerializeField] public float hearingRadiusRun = 10f;
    [Tooltip("Layer mask for the player object.")]
    [SerializeField] private LayerMask playerLayerHearing;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    private Transform playerTransform;
    private PlayerController playerController; 

    public float LastVisionValue { get; private set; }
    public float LastNoiseValue { get; private set; }
    private void Awake()
    {
        if (eyePoint == null) eyePoint = transform;

        GameObject player = FindObjectOfType<PlayerController>().gameObject;
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("[EnemySensor] No GameObject with tag 'Player' found.");
        }
    }

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

    public Vector3 GetPlayerPosition() =>
        playerTransform != null ? playerTransform.position : Vector3.zero;

    private float ComputeVision()
    {
        if (playerTransform == null) {
            //Debug.Log("0 SIN PLAYER TRANSFORM");
            return 0f;
        }

        Vector3 toPlayer = playerTransform.position - eyePoint.position;
        float distance = toPlayer.magnitude;

        if (distance > visionRange)
        {
            //Debug.Log("0 por estar muy lejos");
            return 0f;
        }

        float angle = Vector3.Angle(eyePoint.forward, toPlayer);

        if (angle > visionAngle * 0.5f)
        {
            //Debug.Log("0 por estar fuera del fov");
            return 0f;
        }

        if (!HasLineOfSight(toPlayer, distance))
        {
            //Debug.Log("0 por lineofsight");
            return 0f;
        }

        if (angle > peripheralAngle * 0.5f)
        {
            //Debug.Log("0.5, te ve de reojo");
            return 0.5f;
        }

        if (distance > visionRange * peripheralFraction)
        {
            //Debug.Log("0.5 estas lejos");
            return 0.5f;
        }

        //Debug.Log("TE ESTA VIENDO");
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
        if (playerTransform == null) return 0f;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        PlayerController.MoveState moveState = playerController != null
            ? playerController.CurrentMoveState
            : PlayerController.MoveState.Idle;

        if (moveState == PlayerController.MoveState.Idle ||
            moveState == PlayerController.MoveState.Crouch)
            return 0f;

        if (moveState == PlayerController.MoveState.Run)
        {
            if (dist <= hearingRadiusWalk) return 1f;  
            if (dist <= hearingRadiusRun) return 1f;   
            return 0f;
        }

        if (moveState == PlayerController.MoveState.Walk)
        {
            if (dist <= hearingRadiusWalk) return 0.5f; 
            return 0f;
        }

        return 0f;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Transform origin = eyePoint != null ? eyePoint : transform;

        Gizmos.color = Color.yellow;
        DrawCone(origin, visionRange, visionAngle);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        DrawCone(origin, visionRange, peripheralAngle);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hearingRadiusWalk);
        Gizmos.color = Color.green;
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
