using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalAI.Ollama
{
    public sealed class OllamaClient
    {
        private const string GenerateEndpoint = "http://localhost:11434/api/generate";

        private readonly string modelName;
        private readonly int timeoutSeconds;

        public OllamaClient(string modelName, int timeoutSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));
            }

            this.modelName = modelName;
            this.timeoutSeconds = timeoutSeconds;
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));
            }

            var requestBody = new OllamaRequest(modelName, prompt);
            var json = JsonUtility.ToJson(requestBody);
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(GenerateEndpoint, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = timeoutSeconds
            };

            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Ollama request failed: {request.error}");
            }

            var responseText = request.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                throw new InvalidOperationException("Ollama returned an empty HTTP response.");
            }

            var ollamaResponse = JsonUtility.FromJson<OllamaResponse>(responseText);

            if (ollamaResponse == null)
            {
                throw new InvalidOperationException($"Could not parse Ollama response: {responseText}");
            }

            if (string.IsNullOrWhiteSpace(ollamaResponse.Response))
            {
                throw new InvalidOperationException($"Ollama returned an empty model response: {responseText}");
            }

            return ollamaResponse.Response;
        }
    }
}