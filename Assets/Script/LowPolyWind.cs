using UnityEngine;

public class LowPolyWindManager : MonoBehaviour
{
    [Header("Paramètres du Vent")]
    [Tooltip("La force/amplitude du balancement (en degrés).")]
    public float windStrength = 5f;
    [Tooltip("La vitesse à laquelle le vent souffle.")]
    public float windSpeed = 1.5f;
    [Tooltip("Direction principale du vent. Modifiez X et Z pour changer l'axe d'inclinaison.")]
    public Vector3 windDirection = new Vector3(1f, 0f, 0.5f);

    [Header("Entités à animer")]
    [Tooltip("Glissez ici tous les objets (ex: palmiers) que vous souhaitez animer.")]
    public Transform[] targets;

    // Structure privée pour mémoriser l'état unique de chaque arbre
    private struct WindEntity
    {
        public Transform transform;
        public Quaternion initialRotation;
        public float randomOffset;
    }

    // Tableau qui contiendra les données traitées de nos arbres
    private WindEntity[] entitiesData;

    void Start()
    {
        // On normalise la direction du vent une seule fois au début
        windDirection = windDirection.normalized;

        // On initialise le tableau de données avec la même taille que le tableau des cibles
        if (targets != null)
        {
            entitiesData = new WindEntity[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    // On enregistre les données individuelles pour chaque entité
                    entitiesData[i] = new WindEntity
                    {
                        transform = targets[i],
                        initialRotation = targets[i].rotation,
                        randomOffset = Random.Range(0f, 1000f)
                    };
                }
            }
        }
    }

    void Update()
    {
        if (entitiesData == null || entitiesData.Length == 0) return;

        // Optimisation : On stocke Time.time une seule fois par frame pour éviter
        // d'appeler l'API Unity des centaines de fois dans la boucle.
        float currentTime = Time.time;

        // On boucle sur toutes nos entités
        for (int i = 0; i < entitiesData.Length; i++)
        {
            // Sécurité au cas où un objet aurait été détruit pendant la partie
            if (entitiesData[i].transform == null) continue;

            // Le temps est additionné à l'offset aléatoire SPÉCIFIQUE à cet arbre
            float time = (currentTime + entitiesData[i].randomOffset) * windSpeed;
            
            // Onde principale
            float wave1 = Mathf.Sin(time);
            // Onde secondaire
            float wave2 = Mathf.Sin(time * 0.7f) * 0.5f; 
            
            float combinedWave = wave1 + wave2;

            // Calcul de l'inclinaison
            float swayX = windDirection.x * combinedWave * windStrength;
            float swayZ = windDirection.z * combinedWave * windStrength;

            // Création de la nouvelle rotation
            Quaternion windRotation = Quaternion.Euler(swayX, 0f, swayZ);

            // Application de la rotation
            entitiesData[i].transform.rotation = entitiesData[i].initialRotation * windRotation;
        }
    }
}