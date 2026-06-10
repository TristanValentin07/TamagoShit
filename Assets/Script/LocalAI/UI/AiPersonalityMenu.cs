using LocalAI.Config;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LocalAI.UI
{
    public sealed class AiPersonalityMenu : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string sceneToLoadAfterMenu = "";

        private string characterName;
        private string role;
        private string tone;
        private string behaviorRules;
        private string backstory;

        private Vector2 scrollPosition;

        private void Start()
        {
            var personality = AiPersonalityStorage.LoadOrCreateDefault();

            characterName = personality.CharacterName;
            role = personality.Role;
            tone = personality.Tone;
            behaviorRules = personality.BehaviorRules;
            backstory = personality.Backstory;
        }

        private void OnGUI()
        {
            const int width = 720;
            const int height = 620;

            var x = (Screen.width - width) / 2;
            var y = (Screen.height - height) / 2;

            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);

            GUILayout.Label("Créer la personnalité de l'IA");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Nom du personnage");
            characterName = GUILayout.TextField(characterName);

            GUILayout.Space(10);

            GUILayout.Label("Rôle");
            role = GUILayout.TextArea(role, GUILayout.Height(60));

            GUILayout.Space(10);

            GUILayout.Label("Ton");
            tone = GUILayout.TextArea(tone, GUILayout.Height(60));

            GUILayout.Space(10);

            GUILayout.Label("Règles de comportement");
            behaviorRules = GUILayout.TextArea(behaviorRules, GUILayout.Height(120));

            GUILayout.Space(10);

            GUILayout.Label("Backstory / personnalité");
            backstory = GUILayout.TextArea(backstory, GUILayout.Height(160));

            GUILayout.EndScrollView();

            GUILayout.Space(15);

            if (GUILayout.Button("Sauvegarder la personnalité"))
            {
                SavePersonality();
            }

            if (GUILayout.Button("Sauvegarder et commencer"))
            {
                SavePersonality();
                StartExperience();
            }

            if (GUILayout.Button("Réinitialiser"))
            {
                ResetToDefault();
            }

            GUILayout.EndArea();
        }

        private void SavePersonality()
        {
            var personality = new AiPersonality(
                characterName,
                role,
                tone,
                behaviorRules,
                backstory
            );

            AiPersonalityStorage.Save(personality);
        }

        private void StartExperience()
        {
            if (string.IsNullOrWhiteSpace(sceneToLoadAfterMenu))
            {
                Debug.LogWarning("No scene configured. Staying in current scene.");
                return;
            }

            SceneManager.LoadScene(sceneToLoadAfterMenu);
        }

        private void ResetToDefault()
        {
            var personality = AiPersonality.Default();

            characterName = personality.CharacterName;
            role = personality.Role;
            tone = personality.Tone;
            behaviorRules = personality.BehaviorRules;
            backstory = personality.Backstory;

            AiPersonalityStorage.Save(personality);
        }
    }
}