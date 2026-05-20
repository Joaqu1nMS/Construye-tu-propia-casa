using System.Collections;
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
    [SerializeField] private float patrolSpeed     = 2f;
    [SerializeField] private float investigateSpeed = 3.5f;
    [SerializeField] private float chaseSpeed      = 6f;
    [SerializeField] private float searchSpeed     = 5f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Range(0f, 5f)] private float waypointIdleTime = 2f;
    [SerializeField] private float waypointReachThreshold = 0.5f;

    [Header("Investigate / Search")]
    [SerializeField, Range(1f, 15f)] public float searchWaitTime   = 5f;
    [SerializeField] private float lookAroundDuration              = 3f;
    [SerializeField] private float arrivalThreshold                = 0.6f;

    [Header("Animator (optional)")]
    [SerializeField] private Animator animator;
    private static readonly int SpeedHash  = Animator.StringToHash("Speed");
    private static readonly int StateHash  = Animator.StringToHash("State");

    // ── Private ────────────────────────────────────────────────────────────────
    private NavMeshAgent   _agent;
    private EnemySensor    _sensor;
    private EnemyFSM       _fsm;

    private int    _waypointIndex;
    private bool   _isIdlingAtWaypoint;

    private Vector3 _lastKnownPosition;
    private bool    _reachedLKP;
    private float   _searchTimer;
    private bool    _isLookingAround;

    private Coroutine _idleCoroutine;
    private Coroutine _lookAroundCoroutine;

    // ── Unity ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _agent  = GetComponent<NavMeshAgent>();
        _sensor = GetComponent<EnemySensor>();
        _fsm    = GetComponent<EnemyFSM>();
    }

    // ── FSM Callbacks ──────────────────────────────────────────────────────────
    public void OnEnterState(EnemyFSM.EnemyState state)
    {
        StopAllCoroutines();
        _isIdlingAtWaypoint  = false;
        _isLookingAround     = false;
        _reachedLKP          = false;

        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol:
                _agent.speed = patrolSpeed;
                SetAnimatorState(0);
                GoToNextWaypoint();
                break;

            case EnemyFSM.EnemyState.Investigate:
                _agent.speed     = investigateSpeed;
                _lastKnownPosition = _sensor.GetPlayerPosition();
                _agent.SetDestination(_lastKnownPosition);
                SetAnimatorState(1);
                break;

            case EnemyFSM.EnemyState.Chase:
                _agent.speed = chaseSpeed;
                SetAnimatorState(2);
                break;

            case EnemyFSM.EnemyState.Search:
                _agent.speed       = searchSpeed;
                _lastKnownPosition = _sensor.GetPlayerPosition();
                _reachedLKP        = false;
                _searchTimer       = 0f;
                _agent.SetDestination(_lastKnownPosition);
                SetAnimatorState(3);
                break;
        }
    }

    public void OnExitState(EnemyFSM.EnemyState state) { /* Reserved for exit logic */ }

    /// <summary>Called by EnemyFSM every Update() after transition evaluation.</summary>
    public void ExecuteState(EnemyFSM.EnemyState state)
    {
        UpdateAnimatorSpeed();

        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol:      ExecutePatrol();      break;
            case EnemyFSM.EnemyState.Investigate: ExecuteInvestigate(); break;
            case EnemyFSM.EnemyState.Chase:       ExecuteChase();       break;
            case EnemyFSM.EnemyState.Search:      ExecuteSearch();      break;
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
        bool  hasStimulus = v > 0f || r > 0.49f;

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
            _lookAroundCoroutine = StartCoroutine(LookAround(lookAroundDuration));
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
                _lookAroundCoroutine = StartCoroutine(LookAround(searchWaitTime));
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private bool HasReachedDestination()
    {
        if (_agent.pathPending) return false;
        return _agent.remainingDistance <= arrivalThreshold;
    }

    private IEnumerator LookAround(float duration)
    {
        _isLookingAround = true;
        _agent.ResetPath();

        float elapsed = 0f;
        float interval = 0.8f;
        float nextTurn = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= nextTurn)
            {
                // Rotate to a random yaw offset to simulate looking around
                float targetYaw = transform.eulerAngles.y + Random.Range(-120f, 120f);
                transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
                nextTurn = elapsed + interval;
            }
            yield return null;
        }

        _isLookingAround = false;
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;
        animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
    }

    private void SetAnimatorState(int stateIndex)
    {
        if (animator == null) return;
        animator.SetInteger(StateHash, stateIndex);
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
