using System.Text;
using LocalAI.Models;

namespace LocalAI.Pipeline
{
    public static class AiPromptBuilder
    {
        public static string Build(AiInteractionContext context)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Tu es une IA intégrée dans une scène 3D interactive.");
            builder.AppendLine("Tu contrôles uniquement des intentions simples. Tu ne dois jamais inventer de fonctions techniques.");
            builder.AppendLine("Tu dois répondre uniquement avec un JSON valide.");
            builder.AppendLine("N'ajoute aucun markdown, aucune balise, aucun commentaire, aucune explication.");
            builder.AppendLine();
            builder.AppendLine("Format obligatoire :");
            builder.AppendLine("{");
            builder.AppendLine("  \"dialogue\": \"phrase courte affichable au joueur\",");
            builder.AppendLine("  \"intent\": \"talk | explain_machine | suggest_action | fallback\",");
            builder.AppendLine("  \"action\": {");
            builder.AppendLine("    \"type\": \"none | highlight_machine | start_machine | stop_machine\",");
            builder.AppendLine("    \"target\": \"nom de la cible\"");
            builder.AppendLine("  }");
            builder.AppendLine("}");
            builder.AppendLine();
            builder.AppendLine("Règles :");
            builder.AppendLine("- Le champ dialogue doit être court.");
            builder.AppendLine("- Le champ intent doit être une des valeurs autorisées.");
            builder.AppendLine("- Le champ action.type doit être une des valeurs autorisées.");
            builder.AppendLine("- Si aucune action n'est utile, utilise action.type = \"none\".");
            builder.AppendLine("- Si la demande utilisateur est incompréhensible, utilise intent = \"fallback\".");
            builder.AppendLine();
            builder.AppendLine("Contexte de la scène :");
            builder.AppendLine($"- Scène : {context.SceneName}");
            builder.AppendLine($"- Machine : {context.MachineName}");
            builder.AppendLine($"- État machine : {context.CurrentMachineState}");
            builder.AppendLine();
            builder.AppendLine("Message joueur :");
            builder.AppendLine(context.PlayerInput);
            builder.AppendLine();
            builder.AppendLine("Réponds maintenant uniquement en JSON valide.");

            return builder.ToString();
        }
    }
}