using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] private Transform origenVision;
    [SerializeField, Range(1f, 50f)] public float rangoDeVision = 20f;
    [SerializeField, Range(10f, 180f)] public float anguloDeVision = 90f;
    [SerializeField, Range(0f, 1f)] private float peripheralFraction = 0.5f;
    [SerializeField, Range(0f, 180f)] public float peripheralAngle = 60f;
    [SerializeField] private LayerMask layerObstaculo;
    [SerializeField] private LayerMask layerPlayer;

    [Header("Hearing")]
    [SerializeField] public float radioEscuchaWalk = 5f;
    [SerializeField] public float radioEscuchaRun = 10f;
    [Tooltip("Layer mask for the player object.")]
    [SerializeField] private LayerMask playerLayerHearing;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    private Transform playerTransform;
    private PlayerController playerController; 

    public float ultimaVision { get; private set; }
    public float ultimaEscucha { get; private set; }
    private void Awake()
    {
        if (origenVision == null) origenVision = transform;

        GameObject player = FindObjectOfType<PlayerController>().gameObject;
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }
        /*else
        {
            Debug.LogWarning("PLAYER NO ENCONTRADO");
        }*/
    }

    public float GetVisionValue()
    {
        ultimaVision = ComputeVision();
        return ultimaVision;
    }

    public float GetNoiseValue()
    {
        ultimaEscucha = ComputeNoise();
        return ultimaEscucha;
    }

    public Vector3 GetPlayerPosition() =>
        playerTransform != null ? playerTransform.position : Vector3.zero;

    private float ComputeVision()
    {
        if (playerTransform == null) {
            //Debug.Log("0 SIN PLAYER TRANSFORM");
            return 0f;
        }

        Vector3 toPlayer = playerTransform.position - origenVision.position;
        float distance = toPlayer.magnitude;

        if (distance > rangoDeVision)
        {
            //Debug.Log("0 por estar muy lejos");
            return 0f;
        }

        float angle = Vector3.Angle(origenVision.forward, toPlayer);

        if (angle > anguloDeVision * 0.5f)
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

        if (distance > rangoDeVision * peripheralFraction)
        {
            //Debug.Log("0.5 estas lejos");
            return 0.5f;
        }

        //Debug.Log("TE ESTA VIENDO");
        return 1f;
    }

    private bool HasLineOfSight(Vector3 toPlayer, float distance)
    {
        Ray ray = new Ray(origenVision.position, toPlayer.normalized);
        return !Physics.Raycast(ray, distance, layerObstaculo);
    }

    // Calcular sonido
    private float ComputeNoise()
    {
        if (playerTransform == null) return 0f;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        PlayerController.MoveState moveState = playerController != null
            ? playerController.estadoPlayer
            : PlayerController.MoveState.Idle;

        if (moveState == PlayerController.MoveState.Idle ||
            moveState == PlayerController.MoveState.Crouch)
            return 0f;

        if (moveState == PlayerController.MoveState.Run)
        {
            if (dist <= radioEscuchaWalk) return 1f;  
            if (dist <= radioEscuchaRun) return 1f;   
            return 0f;
        }

        if (moveState == PlayerController.MoveState.Walk)
        {
            if (dist <= radioEscuchaWalk) return 0.5f; 
            return 0f;
        }

        return 0f;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Transform origin = origenVision != null ? origenVision : transform;

        Gizmos.color = Color.yellow;
        DrawCone(origin, rangoDeVision, anguloDeVision);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        DrawCone(origin, rangoDeVision, peripheralAngle);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioEscuchaWalk);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioEscuchaRun);
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
