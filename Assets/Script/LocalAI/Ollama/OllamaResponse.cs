using System;
using UnityEngine;

namespace LocalAI.Ollama
{
    [Serializable]
    public sealed class OllamaResponse
    {
        [SerializeField] private string model;
        [SerializeField] private string created_at;
        [SerializeField] private string response;
        [SerializeField] private bool done;
        [SerializeField] private string done_reason;

        public string Model => model;
        public string CreatedAt => created_at;
        public string Response => response;
        public bool Done => done;
        public string DoneReason => done_reason;
    }
}