using System;
using System.Threading;
using System.Threading.Tasks;
using LocalAI.Models;
using LocalAI.Ollama;

namespace LocalAI.Pipeline
{
    public sealed class AiPipeline
    {
        private readonly OllamaClient client;

        public AiPipeline(string modelName)
        {
            client = new OllamaClient(modelName);
        }

        public async Task<AiResponse> RunAsync(
            AiInteractionContext context,
            CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var prompt = AiPromptBuilder.Build(context);
            var rawResponse = await client.GenerateAsync(prompt, cancellationToken);

            return AiResponseParser.Parse(rawResponse);
        }
    }
}