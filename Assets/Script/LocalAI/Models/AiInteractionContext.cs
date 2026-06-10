using System;
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

        public string SceneName => sceneName;
        public string MachineName => machineName;
        public string PlayerInput => playerInput;
        public string CurrentMachineState => currentMachineState;

        public AiInteractionContext(
            string sceneName,
            string machineName,
            string playerInput,
            string currentMachineState)
        {
            this.sceneName = sceneName;
            this.machineName = machineName;
            this.playerInput = playerInput;
            this.currentMachineState = currentMachineState;
        }
    }
}