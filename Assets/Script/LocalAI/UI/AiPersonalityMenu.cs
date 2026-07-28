using LocalAI.Config;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LocalAI.UI
{
    public sealed class AiPersonalityMenu : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string sceneToLoadAfterMenu = "SampleScene";
        [SerializeField] private string mainMenuScene = "MainMenuScene";

        private string characterName;
        private string role;
        private string tone;
        private string behaviorRules;
        private string backstory;

        private Vector2 scrollPosition;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle textAreaStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;
        private GUIStyle smallStyle;

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
            EnsureStyles();

            var width = Mathf.Min(760f, Screen.width - 60f);
            var height = Mathf.Min(700f, Screen.height - 50f);
            var x = (Screen.width - width) / 2f;
            var y = (Screen.height - height) / 2f;

            GUILayout.BeginArea(new Rect(x, y, width, height), panelStyle);

            GUILayout.Label("Personnalité IA personnalisée", titleStyle);
            GUILayout.Space(12f);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Nom du personnage", labelStyle);
            characterName = GUILayout.TextField(characterName, textAreaStyle, GUILayout.Height(34f));

            GUILayout.Space(10f);

            GUILayout.Label("Rôle", labelStyle);
            role = GUILayout.TextArea(role, textAreaStyle, GUILayout.Height(70f));

            GUILayout.Space(10f);

            GUILayout.Label("Ton", labelStyle);
            tone = GUILayout.TextArea(tone, textAreaStyle, GUILayout.Height(70f));

            GUILayout.Space(10f);

            GUILayout.Label("Règles de comportement", labelStyle);
            behaviorRules = GUILayout.TextArea(behaviorRules, textAreaStyle, GUILayout.Height(130f));

            GUILayout.Space(10f);

            GUILayout.Label("Backstory / personnalité", labelStyle);
            backstory = GUILayout.TextArea(backstory, textAreaStyle, GUILayout.Height(170f));

            GUILayout.EndScrollView();

            GUILayout.Space(12f);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Utiliser cette personnalité et jouer", buttonStyle, GUILayout.Height(46f)))
            {
                SaveCustomPersonality();
                StartExperience();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Personalité par défaut", buttonStyle, GUILayout.Height(42f)))
            {
                ResetToMomoDefault();
            }

            if (GUILayout.Button("Retour", buttonStyle, GUILayout.Height(42f)))
            {
                BackToMainMenu();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void SaveCustomPersonality()
        {
            var personality = new AiPersonality(
                SafeValue(characterName, "Momo"),
                SafeValue(role, "Petit guide de la scène interactive."),
                SafeValue(tone, "Curieux, doux et clair."),
                SafeValue(behaviorRules, "Répondre brièvement et rester dans le personnage."),
                SafeValue(backstory, "Une petite taupe qui accompagne le joueur dans le village.")
            );

            AiPersonalityMode.UseCustom();
            AiPersonalityStorage.Save(personality);
        }

        private static string SafeValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private void StartExperience()
        {
            var targetScene = sceneToLoadAfterMenu;

            // Existing scene instances from older patches may still have AiTestScene serialized.
            if (string.IsNullOrWhiteSpace(targetScene) || targetScene == "AiTestScene")
            {
                targetScene = "SampleScene";
            }

            SceneManager.LoadScene(targetScene);
        }

        private void BackToMainMenu()
        {
            SceneManager.LoadScene(mainMenuScene);
        }

        private void ResetToMomoDefault()
        {
            var personality = MoleDefaultPersonality.Create();

            characterName = personality.CharacterName;
            role = personality.Role;
            tone = personality.Tone;
            behaviorRules = personality.BehaviorRules;
            backstory = personality.Backstory;

            AiPersonalityMode.UseDefault();
            AiPersonalityStorage.Save(personality);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.78f, 0.08f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.white;

            textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = 15,
                wordWrap = true
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            smallStyle.normal.textColor = new Color(1f, 1f, 1f, 0.78f);

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.05f, 0.08f, 0.16f, 0.90f));
            texture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(28, 28, 24, 24)
            };
            panelStyle.normal.background = texture;
        }
    }
}
