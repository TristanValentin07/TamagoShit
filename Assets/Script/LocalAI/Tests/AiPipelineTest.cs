using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalAI.Config;
using LocalAI.Models;
using LocalAI.Pipeline;
using UnityEngine;

namespace LocalAI.Tests
{
    public sealed class AiPipelineTest : MonoBehaviour
    {
        [Header("Ollama")]
        [SerializeField] private string modelName = "qwen3:4b-instruct";

        [Header("Test Context")]
        [SerializeField] private string sceneName = "SceneTestIA";
        [SerializeField] private string machineName = "Village insulaire avec plusieurs objets observables";
        [SerializeField] private string currentMachineState = "Le puits est au centre de la place. Aucun objet n'est ciblé par défaut.";
        [SerializeField] private string playerInput = "Regarde le puits.";

        private CancellationTokenSource cancellationTokenSource;

        private async void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await RunTestAsync();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("AI pipeline test cancelled.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"AI pipeline test failed: {exception.Message}");
            }
        }

        private async Task RunTestAsync()
        {
            var personality = AiPersonalityStorage.LoadOrCreateDefault();
            var pipeline = new AiPipeline(modelName);

            var history = new List<AiChatMessage>
            {
                new AiChatMessage("joueur", "Où est le puits ?"),
                new AiChatMessage("momo", "Je crois qu'il est près du centre."),
                new AiChatMessage("joueur", "Oui, il est au centre de l'île.")
            };

            var points = new List<SceneInterestPoint>
            {
                new SceneInterestPoint(
                    "well",
                    "le puits",
                    "puits, centre, place centrale",
                    "Un puits gris situé au centre de la place. Ce n'est pas un miroir.",
                    "look_at, describe",
                    Vector3.zero
                )
            };

            var context = new AiInteractionContext(
                sceneName,
                machineName,
                playerInput,
                currentMachineState,
                personality,
                history,
                points
            );

            Debug.Log($"Loaded AI personality: {personality.CharacterName}");
            Debug.Log("Sending request to local AI...");

            var response = await pipeline.RunAsync(
                context,
                cancellationTokenSource.Token
            );

            Debug.Log($"AI dialogue: {response.Dialogue}");
            Debug.Log($"AI intent: {response.Intent}");

            if (response.Action == null)
            {
                Debug.Log("AI action: null");
                return;
            }

            Debug.Log($"AI action: {response.Action.Type} -> {response.Action.Target}");
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
