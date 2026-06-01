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
    [SerializeField] private AudioClip mmmAudio;
    [SerializeField] private AudioClip heyAudio;
    [SerializeField] private AudioClip pilladoAudio;    


    private NavMeshAgent navMesh;
    private EnemySensor sensor;
    private EnemyFSM fsm;
    private FuzzyLogicController fuzzy;

    private int waypointIndex;
    private bool vaHaciaWaypoint;

    private Vector3 ultimaPosicionPlayer;
    private bool llegoUltPosPlayer;     

    // Audio sources
    private AudioSource pitch1;    

    private void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
        sensor = GetComponent<EnemySensor>();
        fsm = GetComponent<EnemyFSM>();
        fuzzy = GetComponent<FuzzyLogicController>();
        animator = GetComponent<Animator>();

        List<AudioSource> audios = GetComponents<AudioSource>().ToList();
        pitch1 = audios[0];        
    }
    
    public void OnEnterState(EnemyFSM.EnemyState state)
    {
        StopAllCoroutines();
        vaHaciaWaypoint = false;        
        llegoUltPosPlayer = false;

        //Debug.Log($"Entrado a {state}");
        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol:
                navMesh.speed = patrolSpeed;
                GoToNextWaypoint();

                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Investigate:
                GameManager.gameM.ReproducirSonido(pitch1, mmmAudio, -1);
                navMesh.speed = investigateSpeed;
                ultimaPosicionPlayer = sensor.GetPlayerPosition();
                navMesh.SetDestination(ultimaPosicionPlayer);

                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Chase:
                if (fsm.estadoAnterior != EnemyFSM.EnemyState.Search) GameManager.gameM.ReproducirSonido(pitch1, heyAudio, -1);
                navMesh.speed = chaseSpeed;

                animator.SetBool("Walk", true);
                animator.SetBool("Search", false);
                break;

            case EnemyFSM.EnemyState.Search:
                navMesh.speed = searchSpeed;
                ultimaPosicionPlayer = sensor.GetPlayerPosition();
                llegoUltPosPlayer = false;
                
                navMesh.SetDestination(ultimaPosicionPlayer);
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
        sensor.anguloDeVision = 50f;
        sensor.peripheralAngle = 90f;
        if (waypoints == null || waypoints.Length == 0) return;
        if (vaHaciaWaypoint) return;

        if (HaLlegadoDestino())
        {
            StartCoroutine(IdleAtWaypoint());
        }
    }

    private IEnumerator IdleAtWaypoint()
    {
        vaHaciaWaypoint = true;
        navMesh.ResetPath();
        yield return new WaitForSeconds(waypointIdleTime);
        GoToNextWaypoint();
        vaHaciaWaypoint = false;
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        navMesh.SetDestination(waypoints[waypointIndex].position);
    }

    // INVESTIGATE 
    private void ExecuteInvestigate()
    {
        sensor.anguloDeVision = 50f;
        sensor.peripheralAngle = 90f;        

        float v = sensor.GetVisionValue();
        float r = sensor.GetNoiseValue();
        bool hasStimulus = v > 0f || r > 0.49f;

        if (!HaLlegadoDestino())
        {
            if (hasStimulus)
            {
                ultimaPosicionPlayer = sensor.GetPlayerPosition();
                navMesh.SetDestination(ultimaPosicionPlayer);
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
        sensor.anguloDeVision = 50f;
        sensor.peripheralAngle = 90f;
        navMesh.SetDestination(sensor.GetPlayerPosition());
    }

    // SEARCH
    private void ExecuteSearch()
    {
        sensor.anguloDeVision = 180f;
        sensor.peripheralAngle = 180f;

        if (!llegoUltPosPlayer)
        {
            if (HaLlegadoDestino())
            {
                llegoUltPosPlayer = true;
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

        GameManager.gameM.ReproducirSonido(pitch1, pilladoAudio, -1);
        FindObjectOfType<PlayerController>().isBlocked = true;
        fsm.isBlocked = true;
        navMesh.SetDestination(navMesh.transform.position); // Detener movimiento
        StartCoroutine(GameManager.gameM.CambiarEscena(1, 1f));
    }

    // HELPERS
    private bool HaLlegadoDestino()
    {
        if (navMesh.pathPending) return false;
        return navMesh.remainingDistance <= arrivalThreshold;
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
    
    public void NotificarSonido(Vector3 sourcePosition)
    {
        ultimaPosicionPlayer = sourcePosition;
        if (fsm.estadoActual != EnemyFSM.EnemyState.Chase)
        {
            //Debug.Log("INVESTIGA POR RUIDO");
            fuzzy.SetSuspicion(fsm.umbralPerseguir-1);
            ExecuteInvestigate();
        }
        /*if (_fsm.CurrentState == EnemyFSM.EnemyState.Patrol ||
            _fsm.CurrentState == EnemyFSM.EnemyState.Investigate)
        {
            _agent.SetDestination(sourcePosition);
        }*/
    }
}
