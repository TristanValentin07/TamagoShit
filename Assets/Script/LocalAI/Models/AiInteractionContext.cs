using System;
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

        public string SceneName => sceneName;
        public string MachineName => machineName;
        public string PlayerInput => playerInput;
        public string CurrentMachineState => currentMachineState;
        public AiPersonality Personality => personality;

        public AiInteractionContext(
            string sceneName,
            string machineName,
            string playerInput,
            string currentMachineState,
            AiPersonality personality)
        {
            this.sceneName = sceneName;
            this.machineName = machineName;
            this.playerInput = playerInput;
            this.currentMachineState = currentMachineState;
            this.personality = personality ?? AiPersonality.Default();
        }
    }
}