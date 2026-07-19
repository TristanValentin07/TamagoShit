using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class MolePlayableAI : MonoBehaviour
{
    [Header("Paramètres NavMesh (Collisions avec les murs)")]
    [Tooltip("La largeur de la taupe pour l'IA. Agrandissez cette valeur (ex: 1 ou 1.5) pour qu'elle s'éloigne des murs.")]
    public float agentRadius = 0.5f;

    [Header("Paramètres de déplacement")]
    public float moveSpeed = 1.5f;
    public float stoppingDistance = 0.2f;

    [Header("Paramètres de pause")]
    public float minWaitTime = 3f;
    public float maxWaitTime = 10f;

    [Header("Idle")]
    [Tooltip("Temps sans commande utilisateur avant que la taupe recommence à se promener toute seule.")]
    public float idleDelayAfterUserCommand = 30f;

    [Tooltip("Si true, une commande utilisateur stoppe immédiatement la promenade idle en cours.")]
    public bool stopIdleMovementOnUserCommand = true;

    [Header("Références")]
    public Transform[] waypoints;
    [Tooltip("L'Animation Clip de marche.")]
    public AnimationClip walkClip;

    private NavMeshAgent agent;
    private Animator animator;
    private int lastWaypointIndex = -1;

    private float lastUserCommandTime;
    private Coroutine idleRoutine;

    // Variables API Playables
    private PlayableGraph playableGraph;
    private AnimationClipPlayable clipPlayable;

    public bool IsInUserCommandCooldown => Time.time - lastUserCommandTime < idleDelayAfterUserCommand;

    void Start()
    {
        // 1. Initialisation de l'IA (NavMesh)
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.radius = agentRadius; // Applique le rayon pour éviter les murs

        // 2. Initialisation de l'Animation (Playables)
        animator = GetComponent<Animator>();
        SetupAnimationGraph();

        // Empêche l'idle de partir instantanément au lancement si une commande IA arrive très vite.
        lastUserCommandTime = Time.time;

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Aucun waypoint assigné à la taupe !");
            return;
        }

        idleRoutine = StartCoroutine(MoleBehaviorRoutine());
    }

    /// <summary>
    /// À appeler depuis le script d'IA/chat dès que le joueur envoie une commande
    /// ou dès que l'IA applique une action utilisateur.
    /// </summary>
    public void NotifyUserCommand()
    {
        lastUserCommandTime = Time.time;

        if (!stopIdleMovementOnUserCommand || agent == null)
        {
            return;
        }

        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        StopWalkAnimation();
    }

    /// <summary>
    /// Option pratique : à utiliser pour envoyer Momo quelque part depuis l'IA
    /// sans que l'idle ne reprenne avant idleDelayAfterUserCommand secondes.
    /// </summary>
    public bool MoveToUserCommand(Vector3 destination)
    {
        NotifyUserCommand();

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning("Commande ignorée : NavMeshAgent absent ou pas sur le NavMesh.");
            return false;
        }

        if (!NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"Commande ignorée : destination hors NavMesh ou trop loin du NavMesh : {destination}");
            return false;
        }

        agent.isStopped = false;
        agent.SetDestination(hit.position);
        StartWalkAnimation();

        return true;
    }

    private void SetupAnimationGraph()
    {
        if (walkClip == null) return;

        playableGraph = PlayableGraph.Create(gameObject.name + "_PlayableGraph");
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        clipPlayable = AnimationClipPlayable.Create(playableGraph, walkClip);

        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play();
        StopWalkAnimation();
    }

    private IEnumerator MoleBehaviorRoutine()
    {
        while (true)
        {
            // Tant qu'une commande utilisateur est récente, l'idle ne choisit aucune nouvelle destination.
            while (IsInUserCommandCooldown)
            {
                yield return null;
            }

            Transform currentTarget = GetRandomWaypoint();

            if (currentTarget == null || agent == null || !agent.isOnNavMesh)
            {
                yield return null;
                continue;
            }

            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
            StartWalkAnimation();

            // Attendre d'arriver à destination, sauf si une commande utilisateur arrive entre temps.
            while (!IsInUserCommandCooldown && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                LoopWalkAnimationIfNeeded();
                yield return null;
            }

            // Si une commande utilisateur arrive, on laisse le script de commande reprendre la main.
            if (IsInUserCommandCooldown)
            {
                StopWalkAnimation();
                yield return null;
                continue;
            }

            // Arrêt net idle.
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            StopWalkAnimation();

            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            float waited = 0f;

            // Pause idle interruptible par commande utilisateur.
            while (waited < waitTime && !IsInUserCommandCooldown)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void StartWalkAnimation()
    {
        if (playableGraph.IsValid())
        {
            clipPlayable.SetSpeed(1f);
        }
    }

    private void StopWalkAnimation()
    {
        if (playableGraph.IsValid())
        {
            clipPlayable.SetSpeed(0f);
        }
    }

    private void LoopWalkAnimationIfNeeded()
    {
        if (!playableGraph.IsValid() || walkClip == null)
        {
            return;
        }

        if (clipPlayable.GetTime() >= walkClip.length)
        {
            clipPlayable.SetTime(0);
        }
    }

    private Transform GetRandomWaypoint()
    {
        if (waypoints.Length == 1) return waypoints[0];

        int randomIndex = lastWaypointIndex;
        while (randomIndex == lastWaypointIndex)
        {
            randomIndex = Random.Range(0, waypoints.Length);
        }

        lastWaypointIndex = randomIndex;
        return waypoints[randomIndex];
    }

    private void OnDestroy()
    {
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }

        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }
}
