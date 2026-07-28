using UnityEngine;

namespace LocalAI.Config
{
    public sealed class MolePersonalityInstaller : MonoBehaviour
    {
        [SerializeField] private bool forceMolePersonalityOnStart = true;

        private void Awake()
        {
            if (!forceMolePersonalityOnStart)
            {
                return;
            }

            if (AiPersonalityMode.UseCustomPersonality)
            {
                Debug.Log("Custom AI personality enabled: MolePersonalityInstaller will not overwrite it.");
                return;
            }

            var personality = MoleDefaultPersonality.Create();
            AiPersonalityStorage.Save(personality);

            Debug.Log("Default mole personality installed: Momo");
        }
    }
}
