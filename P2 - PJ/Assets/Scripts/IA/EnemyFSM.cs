using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(EnemyAIController))]
[RequireComponent(typeof(FuzzyLogicController))]
public partial class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { Patrol, Investigate, Chase, Search, Caught }

    // Inspector
    [Header("State Thresholds")]
    [SerializeField, Range(0f, 100f)] public float investigateThreshold = 20f;
    [SerializeField, Range(0f, 100f)] public float chaseThreshold = 75f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    // State Accessors
    public EnemyState CurrentState  { get; private set; } = EnemyState.Patrol;
    public EnemyState PreviousState { get; private set; } = EnemyState.Patrol;
    private EnemyAIController  controller;
    private FuzzyLogicController fuzzy;

    private float chaseCooldown; 
    public bool animacionSearch = false;
    public bool isBlocked = false;

    private void Awake()
    {
        controller = GetComponent<EnemyAIController>();
        fuzzy      = GetComponent<FuzzyLogicController>();        
    }

    private void Start() => EnterState(EnemyState.Patrol);

    private void Update()
    {
        if(isBlocked)
        {
            StopAllCoroutines();
            
            return;
        }
        
        EvaluateTransitions();
        controller.ExecuteState(CurrentState);
        Debug.Log(CurrentState);
    }

    // Transition Logic 
    private void EvaluateTransitions()
    {
        float s = fuzzy.SuspicionLevel;
        bool  hasLOS = fuzzy.VisionValue >= 1f;

        switch (CurrentState)
        {
            case EnemyState.Patrol:
                if      (s >= chaseThreshold)       TransitionTo(EnemyState.Chase);
                else if (s >= investigateThreshold)  TransitionTo(EnemyState.Investigate);
                break;

            case EnemyState.Investigate:
                if      (s >= chaseThreshold)        TransitionTo(EnemyState.Chase);
                else if (s < investigateThreshold)   TransitionTo(EnemyState.Patrol);
                break;

            case EnemyState.Chase:
                if (!hasLOS && chaseCooldown <= 0f){
                    //Debug.Log("Cooldown reset");
                    chaseCooldown = 3f;
                }
                
                if(chaseCooldown > 0f){ 
                    //Debug.Log("Perdí visión, iniciando cooldown de persecución...");
                    chaseCooldown -= 1f * Time.deltaTime;
                    if(chaseCooldown <= 0f){
                        //Debug.Log("Cooldown terminado, perdiendo al jugador.");
                        TransitionTo(EnemyState.Search);
                    }
                }
                
                break;

            case EnemyState.Search:
                if (hasLOS)
                {
                    Debug.Log("JUGADOR VISTO.");
                    fuzzy.SetSuspicion(100f);
                    TransitionTo(EnemyState.Chase);
                }
                else if (s < chaseThreshold && s > investigateThreshold) TransitionTo(EnemyState.Investigate);
                else if (s < chaseThreshold && !animacionSearch) TransitionTo(EnemyState.Patrol);
                break;
        }
    }

    private void TransitionTo(EnemyState next)
    {
        if (next == CurrentState) return;

        ExitState(CurrentState);
        PreviousState = CurrentState;
        CurrentState  = next;        
        EnterState(next);

        /*if (showDebugLogs)
            Debug.Log($"[FSM] {PreviousState} → {CurrentState}  |  S={_fuzzy.SuspicionLevel:F1}");*/
    }

    private void EnterState(EnemyState state)  => controller.OnEnterState(state);
    private void ExitState(EnemyState state)   => controller.OnExitState(state);    
}

# if UNITY_EDITOR
public partial class EnemyFSM
{
    void OnDrawGizmos()
    {
        if (Application.isPlaying) Handles.Label(transform.position + Vector3.up * 3, $"Sospecha: {fuzzy.SuspicionLevel}");
    }
}
# endif