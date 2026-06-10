using System;
using UnityEngine;

namespace LocalAI.Models
{
    [Serializable]
    public sealed class AiAction
    {
        [SerializeField] private string type;
        [SerializeField] private string target;

        public string Type => type;
        public string Target => target;
    }
}