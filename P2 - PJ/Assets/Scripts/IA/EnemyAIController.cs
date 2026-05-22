using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Drives NavMeshAgent locomotion and executes per-state behaviours.
/// Called by EnemyFSM every frame via ExecuteState().
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemySensor))]
public class EnemyAIController : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float investigateSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float searchSpeed = 5f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Range(0f, 5f)] private float waypointIdleTime = 2f;
    [SerializeField] private float waypointReachThreshold = 0.5f;

    [Header("Investigate / Search")]
    [SerializeField, Range(1f, 15f)] public float searchWaitTime = 5f;
    [SerializeField] private float lookAroundDuration = 3f;
    [SerializeField] private float arrivalThreshold = 0.6f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    // ── Private ────────────────────────────────────────────────────────────────
    private NavMeshAgent _agent;
    private EnemySensor _sensor;
    private EnemyFSM _fsm;

    private int _waypointIndex;
    private bool _isIdlingAtWaypoint;

    private Vector3 _lastKnownPosition;
    private bool _reachedLKP;
    private float _searchTimer;
    private bool _isLookingAround;

    private Coroutine _idleCoroutine;

    // ── Unity ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _sensor = GetComponent<EnemySensor>();
        _fsm = GetComponent<EnemyFSM>();
        animator = GetComponent<Animator>();
    }

    // ── FSM Callbacks ──────────────────────────────────────────────────────────
    public void OnEnterState(EnemyFSM.EnemyState state)
    {
        StopAllCoroutines();
        _isIdlingAtWaypoint = false;
        _isLookingAround = false;
        _reachedLKP = false;

        Debug.Log($"Entrado a {state}");
        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol:
                _agent.speed = patrolSpeed;                
                GoToNextWaypoint();
                break;

            case EnemyFSM.EnemyState.Investigate:
                _agent.speed = investigateSpeed;
                _lastKnownPosition = _sensor.GetPlayerPosition();
                _agent.SetDestination(_lastKnownPosition);
                break;

            case EnemyFSM.EnemyState.Chase:
                _agent.speed = chaseSpeed;
                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Search:
                _agent.speed = searchSpeed;
                _lastKnownPosition = _sensor.GetPlayerPosition();
                _reachedLKP = false;
                _searchTimer = 0f;
                _agent.SetDestination(_lastKnownPosition);
                break;
        }
    }

    public void OnExitState(EnemyFSM.EnemyState state) { /* Reserved for exit logic */ }

    /// <summary>Called by EnemyFSM every Update() after transition evaluation.</summary>
    public void ExecuteState(EnemyFSM.EnemyState state)
    {
        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol: ExecutePatrol(); break;
            case EnemyFSM.EnemyState.Investigate: ExecuteInvestigate(); break;
            case EnemyFSM.EnemyState.Chase: ExecuteChase(); break;
            case EnemyFSM.EnemyState.Search: ExecuteSearch(); break;
        }
    }

    // ── State Execution ────────────────────────────────────────────────────────

    // PATROL – walk between waypoints, idle briefly at each one
    private void ExecutePatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (_isIdlingAtWaypoint) return;

        if (HasReachedDestination())
        {
            _idleCoroutine = StartCoroutine(IdleAtWaypoint());
        }
    }

    private IEnumerator IdleAtWaypoint()
    {
        _isIdlingAtWaypoint = true;
        _agent.ResetPath();
        yield return new WaitForSeconds(waypointIdleTime);
        GoToNextWaypoint();
        _isIdlingAtWaypoint = false;
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
        _agent.SetDestination(waypoints[_waypointIndex].position);
    }

    // INVESTIGATE – fast-walk to last stimulus, look around, let suspicion decay
    private void ExecuteInvestigate()
    {
        if (_isLookingAround) return;

        // Refresh LKP while player is still visible/audible and we haven't arrived yet
        float v = _sensor.GetVisionValue();
        float r = _sensor.GetNoiseValue();
        bool hasStimulus = v > 0f || r > 0.49f;

        if (!HasReachedDestination())
        {
            // Update destination if stimulus still active
            if (hasStimulus)
            {
                _lastKnownPosition = _sensor.GetPlayerPosition();
                _agent.SetDestination(_lastKnownPosition);
            }
        }
        else
        {
            StartCoroutine(RutinaSearch());
        }
    }

    // CHASE – continuously pursue player's real-time position
    private void ExecuteChase()
    {
        _agent.SetDestination(_sensor.GetPlayerPosition());
    }

    // SEARCH – run to LKP, look around, let FSM handle decay back to Investigate
    private void ExecuteSearch()
    {
        if (!_reachedLKP)
        {
            if (HasReachedDestination())
            {
                _reachedLKP = true;
                StartCoroutine(RutinaSearch());
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private bool HasReachedDestination()
    {
        if (_agent.pathPending) return false;
        return _agent.remainingDistance <= arrivalThreshold;
    }

    private IEnumerator LookAround(float degrees, float totalDuration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, degrees, 0f);

        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalDuration);

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.rotation = targetRotation; // snap final exacto
    }

    private IEnumerator RutinaSearch()
    {        
        _fsm.animacionSearch = true;   

        animator.SetBool("Walk", false);
        animator.SetBool("Search", true);
        
        yield return StartCoroutine(LookAround(80f, 3f));
        yield return StartCoroutine(LookAround(-120f, 2f));
        yield return StartCoroutine(LookAround(120f, 3f));
        yield return StartCoroutine(LookAround(-170f, 2f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(LookAround(80f, 2f));

        animator.SetBool("Walk", true);
        animator.SetBool("Search", false);
        
        _fsm.animacionSearch = false;
    }

    // ── Public Utility ─────────────────────────────────────────────────────────
    /// <summary>Called externally (e.g. by ObjectPicker) to notify a loud item drop.</summary>
    public void NotifyLoudNoise(Vector3 sourcePosition)
    {
        _lastKnownPosition = sourcePosition;
        if (_fsm.CurrentState == EnemyFSM.EnemyState.Patrol ||
            _fsm.CurrentState == EnemyFSM.EnemyState.Investigate)
        {
            _agent.SetDestination(sourcePosition);
        }
    }
}
