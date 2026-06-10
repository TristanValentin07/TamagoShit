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

    [Header("Références")]
    public Transform[] waypoints;
    [Tooltip("L'Animation Clip de marche.")]
    public AnimationClip walkClip;

    private NavMeshAgent agent;
    private Animator animator;
    private int lastWaypointIndex = -1;

    // Variables API Playables
    private PlayableGraph playableGraph;
    private AnimationClipPlayable clipPlayable;

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

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Aucun waypoint assigné à la taupe !");
            return;
        }

        StartCoroutine(MoleBehaviorRoutine());
    }

    private void SetupAnimationGraph()
    {
        if (walkClip == null) return;

        playableGraph = PlayableGraph.Create(gameObject.name + "_PlayableGraph");
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        clipPlayable = AnimationClipPlayable.Create(playableGraph, walkClip);
        
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play();
    }

    private IEnumerator MoleBehaviorRoutine()
    {
        while (true)
        {
            // Définir la destination
            Transform currentTarget = GetRandomWaypoint();
            agent.SetDestination(currentTarget.position);
            agent.isStopped = false;

            if (playableGraph.IsValid())
            {
                clipPlayable.SetSpeed(1f);
            }

            // Attendre d'arriver à destination
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                // SÉCURITÉ : Force l'animation à boucler manuellement si le clip ne le fait pas de base
                if (playableGraph.IsValid() && walkClip != null)
                {
                    if (clipPlayable.GetTime() >= walkClip.length)
                    {
                        clipPlayable.SetTime(0); // Rembobine l'animation au début instantanément
                    }
                }

                yield return null; 
            }

            // Arrêt net (IA + Animation)
            agent.isStopped = true;
            if (playableGraph.IsValid())
            {
                clipPlayable.SetSpeed(0f); 
            }

            // Pause
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
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
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }
}