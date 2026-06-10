using System;
using System.IO;
using UnityEngine;

namespace LocalAI.Config
{
    public static class AiPersonalityStorage
    {
        private const string FileName = "ai_personality.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static AiPersonality LoadOrCreateDefault()
        {
            if (!File.Exists(FilePath))
            {
                var defaultPersonality = AiPersonality.Default();
                Save(defaultPersonality);
                return defaultPersonality;
            }

            try
            {
                var json = File.ReadAllText(FilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    var fallback = AiPersonality.Default();
                    Save(fallback);
                    return fallback;
                }

                var personality = JsonUtility.FromJson<AiPersonality>(json);

                if (personality == null || string.IsNullOrWhiteSpace(personality.CharacterName))
                {
                    var fallback = AiPersonality.Default();
                    Save(fallback);
                    return fallback;
                }

                return personality;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not load AI personality file: {exception.Message}");

                var fallback = AiPersonality.Default();
                Save(fallback);
                return fallback;
            }
        }

        public static void Save(AiPersonality personality)
        {
            if (personality == null)
            {
                throw new ArgumentNullException(nameof(personality));
            }

            var json = JsonUtility.ToJson(personality, true);
            File.WriteAllText(FilePath, json);

            Debug.Log($"AI personality saved to: {FilePath}");
        }
    }
}