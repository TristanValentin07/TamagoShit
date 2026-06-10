using System;
using UnityEngine;

namespace LocalAI.Models
{
    [Serializable]
    public sealed class AiResponse
    {
        [SerializeField] private string dialogue;
        [SerializeField] private string intent;
        [SerializeField] private AiAction action;

        public string Dialogue => dialogue;
        public string Intent => intent;
        public AiAction Action => action;

        public static AiResponse Fallback(string reason)
        {
            var json = JsonUtility.ToJson(new AiResponseFallbackDto
            {
                dialogue = $"Je n'ai pas bien compris. {reason}",
                intent = "fallback",
                action = null
            });

            return JsonUtility.FromJson<AiResponse>(json);
        }

        [Serializable]
        private sealed class AiResponseFallbackDto
        {
            public string dialogue;
            public string intent;
            public AiAction action;
        }
    }
}