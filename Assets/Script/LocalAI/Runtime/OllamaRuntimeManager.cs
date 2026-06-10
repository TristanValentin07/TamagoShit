using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LocalAI.Runtime
{
    public sealed class OllamaRuntimeManager : MonoBehaviour
    {
        [Header("Ollama")]
        [SerializeField] private bool launchOllamaOnStart = true;
        [SerializeField] private string ollamaExecutable = "ollama";
        [SerializeField] private string ollamaArguments = "serve";

        private Process ollamaProcess;

        private void Awake()
        {
            if (!launchOllamaOnStart)
            {
                return;
            }

            StartOllama();
        }

        private void StartOllama()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ollamaExecutable,
                    Arguments = ollamaArguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                ollamaProcess = Process.Start(startInfo);

                if (ollamaProcess == null)
                {
                    Debug.LogWarning("Ollama process could not be started.");
                    return;
                }

                Debug.Log("Ollama process started.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not start Ollama automatically: {exception.Message}");
                Debug.LogWarning("If Ollama is already running manually, this is not blocking.");
            }
        }

        private void OnDestroy()
        {
            if (ollamaProcess == null)
            {
                return;
            }

            if (ollamaProcess.HasExited)
            {
                ollamaProcess.Dispose();
                return;
            }

            try
            {
                ollamaProcess.Kill();
                ollamaProcess.Dispose();
                Debug.Log("Ollama process stopped.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not stop Ollama process: {exception.Message}");
            }
        }
    }
}