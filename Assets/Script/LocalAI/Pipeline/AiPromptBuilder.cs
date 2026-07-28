using System.Text;
using LocalAI.Models;

namespace LocalAI.Pipeline
{
    public static class AiPromptBuilder
    {
        public static string Build(AiInteractionContext context)
        {
            var personality = context.Personality;
            var builder = new StringBuilder();

            builder.AppendLine("Tu es une IA locale intégrée dans une scène 3D interactive.");
            builder.AppendLine("Tu incarnes uniquement le personnage défini ci-dessous.");
            builder.AppendLine("Tu dois répondre uniquement avec un JSON valide.");
            builder.AppendLine("N'ajoute aucun markdown, aucune balise, aucun commentaire, aucune explication hors JSON.");
            builder.AppendLine();

            builder.AppendLine("Règles absolues anti-hallucination :");
            builder.AppendLine("- Tu ne dois parler que des objets listés dans le catalogue de scène.");
            builder.AppendLine("- Tu ne dois jamais inventer d'objet absent du catalogue.");
            builder.AppendLine("- Il n'y a pas de miroir dans la scène.");
            builder.AppendLine("- Les positions et descriptions du catalogue viennent de vrais marqueurs placés dans Unity.");
            builder.AppendLine("- Si le joueur mentionne un objet absent ou ambigu, réponds que tu ne le vois pas clairement.");
            builder.AppendLine("- Ne choisis pas un objet au hasard.");
            builder.AppendLine("- Si le joueur corrige une erreur précédente, reconnais la correction et utilise-la dans la suite.");
            builder.AppendLine("- Si aucun objet précis n'est mentionné, utilise action.type = \"none\" et action.target = \"none\".");
            builder.AppendLine("- Tu ne modifies jamais la scène sauf pour l'action spéciale activate_dragon, qui déclenche seulement l'animation d'attaque du dragon.");
            builder.AppendLine();

            builder.AppendLine("Règles d'action :");
            builder.AppendLine("- Si le joueur demande de regarder, observer, montrer ou décrire un objet, utilise action.type = \"look_at\".");
            builder.AppendLine("- Si le joueur demande d'aller, venir, marcher, se rendre, se rapprocher, rejoindre un objet ou un lieu, utilise action.type = \"move_to\".");
            builder.AppendLine("- Si le joueur demande de se reposer, s'asseoir ou attendre près d'un objet, utilise action.type = \"move_to\" vers cet objet.");
            builder.AppendLine("- Si le joueur demande d'activer, réveiller, provoquer, attaquer, déclencher, lancer, démarrer ou interagir avec le dragon/la machine, utilise action.type = \"activate_dragon\" et action.target = \"dragon\".");
            builder.AppendLine("- Le champ action.target doit être l'id exact du catalogue, sauf action.type = \"none\" où target vaut \"none\".");
            builder.AppendLine("- Le dialogue doit correspondre à l'action. Si action.type = \"move_to\", tu peux dire que tu y vas. Si action.type = \"look_at\", dis que tu regardes. Si action.type = \"activate_dragon\", dis que tu déclenches le dragon/la machine.");
            builder.AppendLine();

            builder.AppendLine("Personnalité :");
            builder.AppendLine($"- Nom exact : {personality.CharacterName}");
            builder.AppendLine($"- Rôle : {personality.Role}");
            builder.AppendLine($"- Ton : {personality.Tone}");
            builder.AppendLine($"- Règles de comportement : {personality.BehaviorRules}");
            builder.AppendLine($"- Histoire : {personality.Backstory}");
            builder.AppendLine();

            builder.AppendLine("Catalogue de scène autorisé :");

            foreach (var point in context.InterestPoints)
            {
                builder.AppendLine($"- id: {point.Id}");
                builder.AppendLine($"  nom: {point.DisplayName}");
                builder.AppendLine($"  alias: {point.Aliases}");
                builder.AppendLine($"  description: {point.Description}");
                builder.AppendLine($"  interactions autorisées: {point.AllowedInteractions}");
            }

            builder.AppendLine();

            builder.AppendLine("Historique récent de conversation :");

            if (context.ConversationHistory.Count == 0)
            {
                builder.AppendLine("- Aucun message précédent.");
            }
            else
            {
                foreach (var message in context.ConversationHistory)
                {
                    builder.AppendLine($"- {message.Role}: {message.Content}");
                }
            }

            builder.AppendLine();

            builder.AppendLine("Contexte actuel :");
            builder.AppendLine($"- Scène : {context.SceneName}");
            builder.AppendLine($"- Sujet général : {context.MachineName}");
            builder.AppendLine($"- État : {context.CurrentMachineState}");
            builder.AppendLine();

            builder.AppendLine("Message joueur actuel :");
            builder.AppendLine(context.PlayerInput);
            builder.AppendLine();

            builder.AppendLine("Format JSON obligatoire :");
            builder.AppendLine("{");
            builder.AppendLine("  \"dialogue\": \"phrase courte de Momo\",");
            builder.AppendLine("  \"intent\": \"talk | explain_object | look_at_object | move_to_object | activate_dragon | fallback\",");
            builder.AppendLine("  \"action\": {");
            builder.AppendLine("    \"type\": \"none | look_at | describe | move_to | activate_dragon\",");
            builder.AppendLine("    \"target\": \"id exact du catalogue ou none\"");
            builder.AppendLine("  }");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("Réponds maintenant uniquement avec ce JSON.");

            return builder.ToString();
        }
    }
}
