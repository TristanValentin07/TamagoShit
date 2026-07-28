using UnityEngine;

public class FireTrigger : MonoBehaviour
{
    public ParticleSystem fireParticles;

    // Fonction appelée par la timeline au début du crachat
    public void StartFire()
    {
        if (fireParticles != null)
        {
            fireParticles.Play();
        }
    }

    // Fonction appelée par la timeline à la fin du crachat
    public void StopFire()
    {
        if (fireParticles != null)
        {
            fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
