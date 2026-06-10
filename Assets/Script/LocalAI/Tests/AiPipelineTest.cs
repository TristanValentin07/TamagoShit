using System;
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
        [SerializeField] private string machineName = "MachineSteampunk";
        [SerializeField] private string currentMachineState = "inactive";
        [SerializeField] private string playerInput = "Explique-moi ce que fait cette machine.";

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

            var context = new AiInteractionContext(
                sceneName,
                machineName,
                playerInput,
                currentMachineState,
                personality
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