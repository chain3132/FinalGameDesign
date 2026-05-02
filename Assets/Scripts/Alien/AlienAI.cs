using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AlienAI : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState = State.Patrol;

    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    [Header("Vision")]
    public float visionRange = 10f;

    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource alienAudioSource;
    [SerializeField] private AudioClip alienLoopClip;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float patrolFootstepInterval = 0.75f;
    [SerializeField] private float chaseFootstepInterval = 0.4f;
    [SerializeField] private AudioSource roarAudioSource;
    [SerializeField] private AudioClip[] randomRoarClips;
    [SerializeField] private AudioClip attackRoarClip;
    [SerializeField] private float minRandomRoarDelay = 6f;
    [SerializeField] private float maxRandomRoarDelay = 14f;
    [SerializeField] private float minHearingDistance = 2f;
    [SerializeField] private float maxHearingDistance = 18f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.7f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private UnityEvent onPlayerAttacked;

    public bool isAlerted = false;

    private float nextAttackTime;
    private float nextFootstepTime;
    private float nextRoarTime;
    private bool hasAttackedPlayer;
    private string currentAnimationState;

    private const string WalkAnimationState = "Walk";
    private const string RunAnimationState = "Run";
    private const string AttackAnimationState = "Attack";

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        SetupAudioSources();
    }

    private void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case State.Patrol:
                PatrolBehavior();
                break;
            case State.Chase:
                ChaseBehavior();
                break;
        }

        CheckVision();
        UpdateAnimationLoop();
        UpdateRandomRoar();
    }

    private void PatrolBehavior()
    {
        PlayAnimation(WalkAnimationState, 0.2f);

        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        PlayFootstepIfMoving(patrolFootstepInterval);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void ChaseBehavior()
    {
        if (agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            agent.isStopped = true;
            FacePlayer();
            TryAttackPlayer();
            return;
        }

        agent.isStopped = false;
        PlayAnimation(RunAnimationState, 0.15f);
        agent.SetDestination(player.position);
        PlayFootstepIfMoving(chaseFootstepInterval);
    }

    private void CheckVision()
    {
        var cabinet = FindObjectOfType<HidingCabinet>();
        if (cabinet != null && cabinet.IsHiding)
        {
            if (currentState == State.Chase) currentState = State.Patrol;
            if (agent != null) agent.isStopped = false;
            return;
        }

        if (isAlerted) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < visionRange)
        {
            Vector3 eyeLevel = transform.position + Vector3.up * 1.5f;
            Vector3 directionToPlayer = (player.position - eyeLevel).normalized;

            if (Physics.Raycast(eyeLevel, directionToPlayer, out RaycastHit hit, visionRange))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    currentState = State.Chase;
                    if (agent != null) agent.isStopped = false;
                    return;
                }
            }
        }

        if (currentState == State.Chase) currentState = State.Patrol;
        if (agent != null) agent.isStopped = false;
    }

    public void ForceChase()
    {
        isAlerted = true;
        currentState = State.Chase;
        if (agent != null) agent.isStopped = false;
    }

    private void TryAttackPlayer()
    {
        PlayAnimation(AttackAnimationState, 0.05f);

        if (Time.time < nextAttackTime || hasAttackedPlayer) return;

        nextAttackTime = Time.time + attackCooldown;
        hasAttackedPlayer = true;
        PlayAttackRoar();
        onPlayerAttacked?.Invoke();
        GameFlowManager.Instance?.HandleAlienAttack();
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 10f
        );
    }

    private void PlayAnimation(string stateName, float fadeTime)
    {
        if (animator == null || currentAnimationState == stateName) return;

        currentAnimationState = stateName;
        animator.CrossFade(stateName, fadeTime);
    }

    private void UpdateAnimationLoop()
    {
        if (animator == null) return;
        if (currentAnimationState != WalkAnimationState && currentAnimationState != RunAnimationState) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(currentAnimationState)) return;

        if (stateInfo.normalizedTime >= 0.98f)
            animator.Play(currentAnimationState, 0, 0f);
    }

    private void SetupAudioSources()
    {
        if (alienAudioSource == null)
            alienAudioSource = GetComponent<AudioSource>();

        if (alienAudioSource == null)
            alienAudioSource = gameObject.AddComponent<AudioSource>();

        if (footstepAudioSource == null)
            footstepAudioSource = gameObject.AddComponent<AudioSource>();

        if (roarAudioSource == null)
            roarAudioSource = gameObject.AddComponent<AudioSource>();

        Configure3DAudioSource(alienAudioSource, true);
        Configure3DAudioSource(footstepAudioSource, false);
        Configure3DAudioSource(roarAudioSource, false);

        if (alienLoopClip != null)
            alienAudioSource.clip = alienLoopClip;

        if (alienAudioSource.clip != null && !alienAudioSource.isPlaying)
            alienAudioSource.Play();

        nextRoarTime = Time.time + Random.Range(minRandomRoarDelay, maxRandomRoarDelay);
    }

    private void Configure3DAudioSource(AudioSource source, bool loop)
    {
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minHearingDistance;
        source.maxDistance = maxHearingDistance;
    }

    private void PlayFootstepIfMoving(float interval)
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        if (footstepAudioSource == null || Time.time < nextFootstepTime) return;
        if (agent == null || agent.isStopped || agent.velocity.sqrMagnitude < 0.05f) return;

        footstepAudioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
        nextFootstepTime = Time.time + interval;
    }

    private void UpdateRandomRoar()
    {
        if (randomRoarClips == null || randomRoarClips.Length == 0) return;
        if (roarAudioSource == null || Time.time < nextRoarTime) return;

        roarAudioSource.PlayOneShot(randomRoarClips[Random.Range(0, randomRoarClips.Length)]);
        nextRoarTime = Time.time + Random.Range(minRandomRoarDelay, maxRandomRoarDelay);
    }

    private void PlayAttackRoar()
    {
        if (attackRoarClip == null || roarAudioSource == null) return;

        roarAudioSource.PlayOneShot(attackRoarClip);
    }
}
