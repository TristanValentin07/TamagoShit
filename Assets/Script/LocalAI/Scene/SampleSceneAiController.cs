using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalAI.Config;
using LocalAI.Models;
using LocalAI.Pipeline;
using LocalAI.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace LocalAI.Scene
{
    public sealed class SampleSceneAiController : MonoBehaviour
    {
        [Header("Ollama")]
        [SerializeField] private string modelName = "qwen3:4b-instruct";
        [SerializeField] private bool createOllamaRuntimeIfMissing = true;

        [Header("Scene references")]
        [SerializeField] private Transform moleCharacter;
        [SerializeField] private string moleObjectName = "Mole Rat@Walk Forward In Place";

        [Header("Dragon")]
        [SerializeField] private Animator dragonAnimator;
        [SerializeField] private string dragonObjectName = "dragon_attack";
        [SerializeField] private string dragonMarkerId = "dragon";
        [SerializeField] private string dragonAttackTrigger = "Attack";

        [Header("Interest points")]
        [SerializeField] private bool requireSceneMarkers = true;

        [Header("NavMesh movement")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float angularSpeed = 540f;
        [SerializeField] private float stoppingDistance = 0.45f;
        [SerializeField] private bool createNavMeshAgentIfMissing = true;

        [Header("Chat")]
        [SerializeField] private string defaultPlayerMessage = "Salut Momo.";
        [SerializeField] private string inputControlName = "MomoChatInput";

        [Header("Startup prompt")]
        [SerializeField] private bool launchStartupPrompt = true;
        [SerializeField] private string startupPrompt = "Salut, qui es-tu ?";
        [SerializeField] private float startupPromptDelaySeconds = 0.5f;

        [Header("Conversation")]
        [SerializeField] private int maxStoredMessages = 10;

        private readonly List<AiChatMessage> conversationHistory = new();
        private readonly List<SceneInterestPoint> interestPoints = new();
        private readonly Dictionary<string, Vector3> interestPointPositions = new();

        private AiPipeline pipeline;
        private AiPersonality personality;
        private CancellationTokenSource cancellationTokenSource;
        private NavMeshAgent navMeshAgent;
        private MolePlayableAI molePlayableAI;

        private bool isThinking;
        private bool isMoving;
        private bool startupPromptLaunched;
        private string playerMessage;
        private string dialogue = "Parle à Momo...";
        private string debugIntent = "idle";

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle inputStyle;
        private GUIStyle hintStyle;

        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (createOllamaRuntimeIfMissing && FindObjectOfType<OllamaRuntimeManager>() == null)
            {
                var runtimeObject = new GameObject("Ollama Runtime");
                runtimeObject.AddComponent<OllamaRuntimeManager>();
            }

            personality = AiPersonalityStorage.LoadOrCreateDefault();
            pipeline = new AiPipeline(modelName);
            cancellationTokenSource = new CancellationTokenSource();
            playerMessage = defaultPlayerMessage;

            ResolveSceneReferences();
            SetupNavMeshAgent();
            ResolveDragonReferences();
            BuildInterestPointCatalogFromMarkers();
        }

        private async void Start()
        {
            if (!launchStartupPrompt || string.IsNullOrWhiteSpace(startupPrompt) || startupPromptLaunched)
            {
                return;
            }

            startupPromptLaunched = true;

            if (startupPromptDelaySeconds > 0f)
            {
                var delayMilliseconds = Mathf.RoundToInt(startupPromptDelaySeconds * 1000f);
                await Task.Delay(delayMilliseconds);
            }

            if (this == null || cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            await AskMoleAsync(startupPrompt.Trim());
        }

        private void Update()
        {
            UpdateMovingState();
        }

        private void OnGUI()
        {
            EnsureStyles();
            HandleEnterSubmitBeforeTextField();

            var width = Mathf.Min(540f, Screen.width - 32f);
            var height = 190f;
            var rect = new Rect(16f, Screen.height - height - 16f, width, height);

            GUI.Box(rect, GUIContent.none, panelStyle);

            GUILayout.BeginArea(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, rect.height - 20f));

            GUILayout.BeginHorizontal();
            GUILayout.Label(personality.CharacterName, titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(isThinking ? "réfléchit..." : isMoving ? "se déplace..." : debugIntent, hintStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            GUILayout.Label(dialogue, textStyle, GUILayout.Height(62f));

            GUILayout.Space(8f);

            GUI.SetNextControlName(inputControlName);
            playerMessage = GUILayout.TextField(playerMessage, inputStyle, GUILayout.Height(34f));

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Entrée pour envoyer", hintStyle, GUILayout.Height(20f));
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void HandleEnterSubmitBeforeTextField()
        {
            var currentEvent = Event.current;

            if (currentEvent == null || currentEvent.type != EventType.KeyDown)
            {
                return;
            }

            if (currentEvent.keyCode != KeyCode.Return && currentEvent.keyCode != KeyCode.KeypadEnter)
            {
                return;
            }

            if (GUI.GetNameOfFocusedControl() != inputControlName)
            {
                return;
            }

            currentEvent.Use();

            if (isThinking || string.IsNullOrWhiteSpace(playerMessage))
            {
                return;
            }

            var submittedMessage = playerMessage.Trim();
            playerMessage = string.Empty;

            _ = AskMoleAsync(submittedMessage);
        }

        private async Task AskMoleAsync(string message)
        {
            isThinking = true;
            dialogue = "Momo réfléchit...";
            debugIntent = "thinking";
            molePlayableAI?.NotifyUserCommand();

            AddToHistory("joueur", message);

            try
            {
                var context = new AiInteractionContext(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    "Village insulaire avec points d'intérêt placés directement dans la scène Unity",
                    message,
                    "Les positions des objets viennent de vrais GameObjects marqueurs placés dans la scène. Le dragon peut être activé avec l'action activate_dragon, target dragon. Momo doit utiliser uniquement le catalogue fourni.",
                    personality,
                    new List<AiChatMessage>(conversationHistory),
                    interestPoints
                );

                var response = await pipeline.RunAsync(context, cancellationTokenSource.Token);

                dialogue = response.Dialogue;
                debugIntent = response.Intent;

                AddToHistory("momo", response.Dialogue);
                ApplyAction(response.Action);
            }
            catch (Exception exception)
            {
                dialogue = "Je crois que ma petite radio locale ne répond pas. Vérifie Ollama puis réessaie.";
                debugIntent = "error";
                Debug.LogWarning($"AI interaction failed: {exception.Message}");
            }
            finally
            {
                isThinking = false;
            }
        }

        private void AddToHistory(string role, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            conversationHistory.Add(new AiChatMessage(role, content));

            while (conversationHistory.Count > maxStoredMessages)
            {
                conversationHistory.RemoveAt(0);
            }
        }

        private void ApplyAction(AiAction action)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.Type))
            {
                return;
            }

            var actionType = action.Type.Trim().ToLowerInvariant();

            if (actionType == "none")
            {
                return;
            }

            molePlayableAI?.NotifyUserCommand();

            if (actionType == "activate_dragon")
            {
                ActivateDragon();
                return;
            }

            if (!TryGetInterestPointPosition(action.Target, out var targetPosition))
            {
                debugIntent = "target_unknown";
                Debug.LogWarning($"AI target not found in scene marker catalog: {action.Target}");
                return;
            }

            if (actionType == "move_to" || actionType == "go_to" || actionType == "rest_near")
            {
                MoveToPosition(targetPosition);
                return;
            }

            if (actionType == "look_at" || actionType == "describe")
            {
                LookAtPosition(targetPosition);
            }
        }

        private void ActivateDragon()
        {
            if (TryGetInterestPointPosition(dragonMarkerId, out var dragonPosition))
            {
                LookAtPosition(dragonPosition);
            }

            if (dragonAnimator == null)
            {
                ResolveDragonReferences();
            }

            if (dragonAnimator == null)
            {
                debugIntent = "dragon_missing";
                Debug.LogWarning("Cannot activate dragon: no Animator found. Assign dragonAnimator or check dragonObjectName.");
                return;
            }

            dragonAnimator.ResetTrigger(dragonAttackTrigger);
            dragonAnimator.SetTrigger(dragonAttackTrigger);
            debugIntent = "activate_dragon";
            Debug.Log("Dragon attack animation triggered by AI.");
        }

        private bool TryGetInterestPointPosition(string targetId, out Vector3 position)
        {
            position = default;

            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            return interestPointPositions.TryGetValue(targetId.Trim().ToLowerInvariant(), out position);
        }

        private void MoveToPosition(Vector3 targetPosition)
        {
            if (molePlayableAI != null && molePlayableAI.MoveToUserCommand(targetPosition))
            {
                isMoving = true;
                debugIntent = "move_to_object";
                return;
            }

            if (navMeshAgent == null)
            {
                Debug.LogWarning("Cannot move Momo: no NavMeshAgent found.");
                debugIntent = "no_agent";
                return;
            }

            if (!navMeshAgent.isOnNavMesh)
            {
                Debug.LogWarning("Cannot move Momo: NavMeshAgent is not on a baked NavMesh.");
                debugIntent = "not_on_navmesh";
                return;
            }

            if (!NavMesh.SamplePosition(targetPosition, out var hit, 4f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"Cannot move Momo: target is not near the NavMesh: {targetPosition}");
                debugIntent = "target_off_navmesh";
                return;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(hit.position);
            isMoving = true;
            debugIntent = "move_to_object";
        }

        private void UpdateMovingState()
        {
            if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
            {
                isMoving = false;
                return;
            }

            if (navMeshAgent.pathPending)
            {
                isMoving = true;
                return;
            }

            if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                isMoving = true;
                return;
            }

            if (navMeshAgent.hasPath && navMeshAgent.velocity.sqrMagnitude > 0.01f)
            {
                isMoving = true;
                return;
            }

            isMoving = false;
        }

        private void LookAtPosition(Vector3 position)
        {
            if (moleCharacter == null)
            {
                return;
            }

            var direction = position - moleCharacter.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            moleCharacter.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void ResolveSceneReferences()
        {
            if (moleCharacter == null)
            {
                var mole = GameObject.Find(moleObjectName);

                if (mole != null)
                {
                    moleCharacter = mole.transform;
                }
            }

            if (moleCharacter != null)
            {
                molePlayableAI = moleCharacter.GetComponent<MolePlayableAI>();
            }
        }

        private void ResolveDragonReferences()
        {
            if (dragonAnimator != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(dragonObjectName))
            {
                var dragonObject = GameObject.Find(dragonObjectName);

                if (dragonObject != null)
                {
                    dragonAnimator = dragonObject.GetComponent<Animator>();

                    if (dragonAnimator == null)
                    {
                        dragonAnimator = dragonObject.GetComponentInChildren<Animator>();
                    }

                    if (dragonAnimator == null)
                    {
                        dragonAnimator = dragonObject.GetComponentInParent<Animator>();
                    }
                }
            }

            if (dragonAnimator != null)
            {
                return;
            }

            var animators = FindObjectsOfType<Animator>(true);

            foreach (var animatorCandidate in animators)
            {
                if (animatorCandidate == null || animatorCandidate.runtimeAnimatorController == null)
                {
                    continue;
                }

                var controllerName = animatorCandidate.runtimeAnimatorController.name.ToLowerInvariant();
                var objectName = animatorCandidate.gameObject.name.ToLowerInvariant();

                if (controllerName.Contains("dragon") || objectName.Contains("dragon"))
                {
                    dragonAnimator = animatorCandidate;
                    return;
                }
            }
        }

        private void SetupNavMeshAgent()
        {
            if (moleCharacter == null)
            {
                Debug.LogWarning("Could not setup NavMeshAgent: Momo transform not found.");
                return;
            }

            navMeshAgent = moleCharacter.GetComponent<NavMeshAgent>();

            if (navMeshAgent == null && createNavMeshAgentIfMissing)
            {
                navMeshAgent = moleCharacter.gameObject.AddComponent<NavMeshAgent>();
            }

            if (navMeshAgent == null)
            {
                Debug.LogWarning("No NavMeshAgent found on Momo.");
                return;
            }

            navMeshAgent.speed = moveSpeed;
            navMeshAgent.acceleration = acceleration;
            navMeshAgent.angularSpeed = angularSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = true;
            navMeshAgent.updatePosition = true;
        }

        private void BuildInterestPointCatalogFromMarkers()
        {
            interestPoints.Clear();
            interestPointPositions.Clear();

            var markers = FindObjectsOfType<SceneInterestPointMarker>(true);

            foreach (var marker in markers)
            {
                if (marker == null || string.IsNullOrWhiteSpace(marker.Id))
                {
                    continue;
                }

                var id = marker.Id.Trim().ToLowerInvariant();

                if (interestPointPositions.ContainsKey(id))
                {
                    Debug.LogWarning($"Duplicate AI interest point id ignored: {id}");
                    continue;
                }

                var point = new SceneInterestPoint(
                    id,
                    marker.DisplayName,
                    marker.Aliases,
                    marker.Description,
                    marker.AllowedInteractions,
                    marker.FocusPosition
                );

                interestPoints.Add(point);
                interestPointPositions.Add(id, marker.FocusPosition);
            }

            if (interestPoints.Count == 0)
            {
                var message = "No SceneInterestPointMarker found. Add real marker GameObjects in SampleScene so Momo knows where objects actually are.";
                Debug.LogWarning(message);

                if (requireSceneMarkers)
                {
                    dialogue = "Je n'ai pas encore mes repères dans la scène.";
                    debugIntent = "no_markers";
                }

                return;
            }

            Debug.Log($"Loaded {interestPoints.Count} AI interest point markers from scene.");
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.05f, 0.76f));
            texture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = texture
                }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
            titleStyle.normal.textColor = Color.white;

            textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true
            };
            textStyle.normal.textColor = Color.white;

            inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 14
            };

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleRight
            };
            hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.68f);
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
