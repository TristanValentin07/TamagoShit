using System;
using System.Collections.Generic;
using LocalAI.Config;
using UnityEngine;

namespace LocalAI.Models
{
    [Serializable]
    public sealed class AiInteractionContext
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string machineName;
        [SerializeField] private string playerInput;
        [SerializeField] private string currentMachineState;
        [SerializeField] private AiPersonality personality;
        [SerializeField] private List<AiChatMessage> conversationHistory;
        [SerializeField] private List<SceneInterestPoint> interestPoints;

        public string SceneName => sceneName;
        public string MachineName => machineName;
        public string PlayerInput => playerInput;
        public string CurrentMachineState => currentMachineState;
        public AiPersonality Personality => personality;
        public IReadOnlyList<AiChatMessage> ConversationHistory => conversationHistory;
        public IReadOnlyList<SceneInterestPoint> InterestPoints => interestPoints;

        public AiInteractionContext(
            string sceneName,
            string machineName,
            string playerInput,
            string currentMachineState,
            AiPersonality personality,
            List<AiChatMessage> conversationHistory,
            List<SceneInterestPoint> interestPoints)
        {
            this.sceneName = sceneName;
            this.machineName = machineName;
            this.playerInput = playerInput;
            this.currentMachineState = currentMachineState;
            this.personality = personality ?? AiPersonality.Default();
            this.conversationHistory = conversationHistory ?? new List<AiChatMessage>();
            this.interestPoints = interestPoints ?? new List<SceneInterestPoint>();
        }
    }
}
