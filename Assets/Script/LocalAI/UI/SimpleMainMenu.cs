using LocalAI.Config;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LocalAI.UI
{
    public sealed class SimpleMainMenu : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string sceneToLoad = "SampleScene";
        [SerializeField] private string customPersonalityScene = "AiPersonalityMenuScene";

        [Header("Look")]
        [SerializeField] private string title = "TamagoShit";
        [SerializeField] private string subtitle = "Une petite scène interactive avec IA locale";

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;
        private GUIStyle smallStyle;

        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnGUI()
        {
            EnsureStyles();

            var panelWidth = Mathf.Min(640f, Screen.width - 80f);
            var panelHeight = 470f;
            var panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight
            );

            GUI.Box(panelRect, GUIContent.none, panelStyle);

            GUILayout.BeginArea(new Rect(panelRect.x + 50f, panelRect.y + 45f, panelRect.width - 100f, panelRect.height - 90f));
            GUILayout.Label(title, titleStyle);
            GUILayout.Space(4f);
            GUILayout.Label(subtitle, subtitleStyle);
            GUILayout.Space(38f);

            if (GUILayout.Button("Jouer avec Momo par défaut", buttonStyle, GUILayout.Height(62f)))
            {
                PlayDefault();
            }

            GUILayout.Space(14f);

            if (GUILayout.Button("Jouer avec IA custom", buttonStyle, GUILayout.Height(56f)))
            {
                OpenCustomPersonalityMenu();
            }

            GUILayout.Space(14f);

            if (GUILayout.Button("Quitter", buttonStyle, GUILayout.Height(52f)))
            {
                Quit();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("IA locale : Ollama + qwen3:4b-instruct", smallStyle);
            GUILayout.EndArea();
        }

        private void PlayDefault()
        {
            AiPersonalityMode.UseDefault();
            AiPersonalityStorage.Save(MoleDefaultPersonality.Create());
            SceneManager.LoadScene(sceneToLoad);
        }

        private void OpenCustomPersonalityMenu()
        {
            SceneManager.LoadScene(customPersonalityScene);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            titleStyle.normal.textColor = new Color(1f, 0.78f, 0.08f);

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            subtitleStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 23,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            smallStyle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.05f, 0.08f, 0.16f, 0.88f));
            texture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = texture;
        }
    }
}
