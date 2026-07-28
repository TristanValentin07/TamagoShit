using System;
using UnityEngine;

namespace LocalAI.Config
{
    [Serializable]
    public sealed class AiPersonality
    {
        [SerializeField] private string characterName;
        [SerializeField] private string role;
        [SerializeField] private string tone;
        [SerializeField] private string behaviorRules;
        [SerializeField] private string backstory;

        public string CharacterName => characterName;
        public string Role => role;
        public string Tone => tone;
        public string BehaviorRules => behaviorRules;
        public string Backstory => backstory;

        public AiPersonality(
            string characterName,
            string role,
            string tone,
            string behaviorRules,
            string backstory)
        {
            this.characterName = characterName;
            this.role = role;
            this.tone = tone;
            this.behaviorRules = behaviorRules;
            this.backstory = backstory;
        }

        public static AiPersonality Default()
        {
            return MoleDefaultPersonality.Create();
        }
    }
}
