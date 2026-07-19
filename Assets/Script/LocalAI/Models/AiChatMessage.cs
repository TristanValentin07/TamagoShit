using System;
using UnityEngine;

namespace LocalAI.Models
{
    [Serializable]
    public sealed class AiChatMessage
    {
        [SerializeField] private string role;
        [SerializeField] private string content;

        public string Role => role;
        public string Content => content;

        public AiChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}
