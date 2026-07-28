using UnityEngine;

namespace LocalAI.Config
{
    public static class AiPersonalityMode
    {
        private const string UseCustomPersonalityKey = "LOCAL_AI_USE_CUSTOM_PERSONALITY";

        public static bool UseCustomPersonality => PlayerPrefs.GetInt(UseCustomPersonalityKey, 0) == 1;

        public static void UseDefault()
        {
            PlayerPrefs.SetInt(UseCustomPersonalityKey, 0);
            PlayerPrefs.Save();
        }

        public static void UseCustom()
        {
            PlayerPrefs.SetInt(UseCustomPersonalityKey, 1);
            PlayerPrefs.Save();
        }
    }
}
