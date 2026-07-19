using UnityEngine;

namespace LocalAI.Scene
{
    public sealed class SceneInterestPointMarker : MonoBehaviour
    {
        [Header("AI Identity")]
        [SerializeField] private string id = "interest_point";
        [SerializeField] private string displayName = "point d'intérêt";
        [SerializeField] private string aliases = "";
        [TextArea(2, 5)]
        [SerializeField] private string description = "";
        [SerializeField] private string allowedInteractions = "look_at, describe, move_to";

        public string Id => id;
        public string DisplayName => displayName;
        public string Aliases => aliases;
        public string Description => description;
        public string AllowedInteractions => allowedInteractions;
        public Vector3 FocusPosition => transform.position;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.75f, 0.05f, 0.85f);
            Gizmos.DrawSphere(transform.position, 0.35f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }
    }
}
