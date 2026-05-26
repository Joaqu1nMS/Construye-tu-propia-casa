using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemySensor))]
public class EnemyAIController : MonoBehaviour
{
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

    [Header("Otros")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask puertaLayer;
    [SerializeField] private AudioClip mmm;
    [SerializeField] private AudioClip hey;
    [SerializeField] private AudioClip pillado;    

    // ── Private ────────────────────────────────────────────────────────────────
    private NavMeshAgent agent;
    private EnemySensor sensor;
    private EnemyFSM fsm;
    private FuzzyLogicController fuzzy;

    private int waypointIndex;
    private bool isIdlingAtWaypoint;

    private Vector3 lastKnownPosition;
    private bool reachedLKP;
    private float searchTimer;
    private bool isLookingAround;

    private Coroutine idleCoroutine;    

    // Audio sources
    private AudioSource pitch1;    

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        sensor = GetComponent<EnemySensor>();
        fsm = GetComponent<EnemyFSM>();
        fuzzy = GetComponent<FuzzyLogicController>();
        animator = GetComponent<Animator>();

        List<AudioSource> audios = GetComponents<AudioSource>().ToList();
        pitch1 = audios[0];        
    }
    //FSM Callbacks 
    public void OnEnterState(EnemyFSM.EnemyState state)
    {
        StopAllCoroutines();
        isIdlingAtWaypoint = false;
        isLookingAround = false;
        reachedLKP = false;

        Debug.Log($"Entrado a {state}");
        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol:
                agent.speed = patrolSpeed;
                GoToNextWaypoint();
                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Investigate:
                GameManager.gameM.ReproducirSonido(pitch1, mmm, -1);
                agent.speed = investigateSpeed;
                lastKnownPosition = sensor.GetPlayerPosition();
                agent.SetDestination(lastKnownPosition);
                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Chase:
            
                if (fsm.PreviousState != EnemyFSM.EnemyState.Search) GameManager.gameM.ReproducirSonido(pitch1, hey, -1);
                agent.speed = chaseSpeed;
                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Search:
                agent.speed = searchSpeed;
                lastKnownPosition = sensor.GetPlayerPosition();
                reachedLKP = false;
                searchTimer = 0f;
                agent.SetDestination(lastKnownPosition);
                animator.SetBool("Walk", false);
                animator.SetBool("Search", true);
                break;
        }
    }

    public void OnExitState(EnemyFSM.EnemyState state) { }

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

    // PATROL 
    private void ExecutePatrol()
    {
        sensor.visionAngle = 50f; // visión omnidireccional durante búsqueda
        sensor.peripheralAngle = 90f;
        if (waypoints == null || waypoints.Length == 0) return;
        if (isIdlingAtWaypoint) return;

        if (HasReachedDestination())
        {
            idleCoroutine = StartCoroutine(IdleAtWaypoint());
        }
    }

    private IEnumerator IdleAtWaypoint()
    {
        isIdlingAtWaypoint = true;
        agent.ResetPath();
        yield return new WaitForSeconds(waypointIdleTime);
        GoToNextWaypoint();
        isIdlingAtWaypoint = false;
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    // INVESTIGATE 
    private void ExecuteInvestigate()
    {
        sensor.visionAngle = 50f; // visión omnidireccional durante búsqueda
        sensor.peripheralAngle = 90f;
        if (isLookingAround) return;

        float v = sensor.GetVisionValue();
        float r = sensor.GetNoiseValue();
        bool hasStimulus = v > 0f || r > 0.49f;

        if (!HasReachedDestination())
        {
            if (hasStimulus)
            {
                lastKnownPosition = sensor.GetPlayerPosition();
                agent.SetDestination(lastKnownPosition);
            }
        }
        else
        {
            StartCoroutine(RutinaSearch());
        }
    }

    // CHASE 
    private void ExecuteChase()
    {
        sensor.visionAngle = 50f; // visión omnidireccional durante búsqueda
        sensor.peripheralAngle = 90f;
        agent.SetDestination(sensor.GetPlayerPosition());
    }

    // SEARCH
    private void ExecuteSearch()
    {
        sensor.visionAngle = 180f; // visión omnidireccional durante búsqueda
        sensor.peripheralAngle = 180f;

        if (!reachedLKP)
        {
            if (HasReachedDestination())
            {
                reachedLKP = true;
                StartCoroutine(RutinaSearch());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {        
        if (((1 << other.gameObject.layer) & playerLayer) == 0)
        {
            if (((1 << other.gameObject.layer) & puertaLayer) == 0)
            {
                return; // No es ni el jugador ni la puerta, ignorar
            }
            other.GetComponent<DoorInteraction>()?.AbrePuertaVecino();
            return;
        }

        GameManager.gameM.ReproducirSonido(pitch1, pillado, -1);
        FindObjectOfType<PlayerController>().isBlocked = true;
        GetComponent<EnemyFSM>().isBlocked = true;        
        agent.SetDestination(agent.transform.position); // Detener movimiento
        StartCoroutine(GameManager.gameM.CambiarEscena(1, 1f));
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private bool HasReachedDestination()
    {
        if (agent.pathPending) return false;
        return agent.remainingDistance <= arrivalThreshold;
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
        fsm.animacionSearch = true;

        yield return StartCoroutine(LookAround(80f, 3f));
        yield return StartCoroutine(LookAround(-120f, 2f));
        yield return StartCoroutine(LookAround(120f, 3f));
        yield return StartCoroutine(LookAround(-170f, 2f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(LookAround(80f, 2f));

        fsm.animacionSearch = false;
    }    
    
    public void NotifyLoudNoise(Vector3 sourcePosition)
    {
        lastKnownPosition = sourcePosition;
        if (fsm.CurrentState != EnemyFSM.EnemyState.Chase)
        {
            Debug.Log("INVESTIGA POR RUIDO");
            fuzzy.SetSuspicion(fsm.chaseThreshold-1);
            ExecuteInvestigate();
        }
        /*if (_fsm.CurrentState == EnemyFSM.EnemyState.Patrol ||
            _fsm.CurrentState == EnemyFSM.EnemyState.Investigate)
        {
            _agent.SetDestination(sourcePosition);
        }*/
    }
}
