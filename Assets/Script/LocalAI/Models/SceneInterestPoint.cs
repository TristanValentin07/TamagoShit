using System;
using UnityEngine;

namespace LocalAI.Models
{
    [Serializable]
    public sealed class SceneInterestPoint
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string aliases;
        [SerializeField] private string description;
        [SerializeField] private string allowedInteractions;
        [SerializeField] private Vector3 focusPosition;

        public string Id => id;
        public string DisplayName => displayName;
        public string Aliases => aliases;
        public string Description => description;
        public string AllowedInteractions => allowedInteractions;
        public Vector3 FocusPosition => focusPosition;

        public SceneInterestPoint(
            string id,
            string displayName,
            string aliases,
            string description,
            string allowedInteractions,
            Vector3 focusPosition)
        {
            this.id = id;
            this.displayName = displayName;
            this.aliases = aliases;
            this.description = description;
            this.allowedInteractions = allowedInteractions;
            this.focusPosition = focusPosition;
        }
    }
}
