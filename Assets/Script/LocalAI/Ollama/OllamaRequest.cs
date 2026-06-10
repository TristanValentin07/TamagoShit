using System;
using UnityEngine;

namespace LocalAI.Ollama
{
    [Serializable]
    public sealed class OllamaRequest
    {
        [SerializeField] private string model;
        [SerializeField] private string prompt;
        [SerializeField] private bool stream;

        public OllamaRequest(string model, string prompt)
        {
            this.model = model;
            this.prompt = prompt;
            stream = false;
        }
    }
}