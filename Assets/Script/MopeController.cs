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

    [Header("Références")]
    [Tooltip("L'Animation Clip de marche.")]
    public AnimationClip walkClip;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isWalkingFromCommand;

    // Variables API Playables
    private PlayableGraph playableGraph;
    private AnimationClipPlayable clipPlayable;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.radius = agentRadius;

        animator = GetComponent<Animator>();
        SetupAnimationGraph();

        // Ancienne routine idle retirée volontairement : Momo ne se balade plus tout seul.
    }

    private void Update()
    {
        if (!isWalkingFromCommand)
        {
            return;
        }

        LoopWalkAnimationIfNeeded();

        if (agent == null || !agent.isOnNavMesh)
        {
            StopWalkAnimation();
            isWalkingFromCommand = false;
            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        var arrived = agent.remainingDistance <= agent.stoppingDistance &&
                      (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.01f);

        if (!arrived)
        {
            return;
        }

        StopWalkAnimation();
        isWalkingFromCommand = false;
    }

    /// <summary>
    /// À appeler depuis le script d'IA/chat dès que le joueur envoie une commande.
    /// Stoppe la destination courante pour éviter les conflits avec la commande suivante.
    /// </summary>
    public void NotifyUserCommand()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        StopWalkAnimation();
        isWalkingFromCommand = false;
    }

    /// <summary>
    /// Déplacement commandé par l'utilisateur/l'IA. Remplace l'ancienne promenade idle.
    /// </summary>
    public bool MoveToUserCommand(Vector3 destination)
    {
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
        isWalkingFromCommand = true;

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

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }
}
