using System.Collections;
using UnityEngine;

namespace LocalAI.Scene
{
    public sealed class SceneObjectPulse : MonoBehaviour
    {
        [SerializeField] private float pulseScale = 1.28f;
        [SerializeField] private float pulseDuration = 0.55f;
        [SerializeField] private float spinSpeed = 70f;
        [SerializeField] private Light optionalLight;

        private Vector3 initialScale;
        private Coroutine pulseRoutine;
        private bool active;

        private void Awake()
        {
            initialScale = transform.localScale;
            optionalLight = optionalLight == null ? GetComponentInChildren<Light>() : optionalLight;
        }

        private void Update()
        {
            if (!active) return;
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        public void HighlightOnce()
        {
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
            }

            pulseRoutine = StartCoroutine(PulseRoutine());
        }

        public void SetActiveState(bool isActive)
        {
            active = isActive;

            if (optionalLight != null)
            {
                optionalLight.enabled = isActive;
                optionalLight.intensity = isActive ? 2.4f : 0.8f;
            }

            HighlightOnce();
        }

        private IEnumerator PulseRoutine()
        {
            var elapsed = 0f;

            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / pulseDuration;
                var wave = Mathf.Sin(t * Mathf.PI);
                transform.localScale = initialScale * Mathf.Lerp(1f, pulseScale, wave);
                yield return null;
            }

            transform.localScale = initialScale;
            pulseRoutine = null;
        }
    }
}
