using System;
using LocalAI.Models;
using UnityEngine;

namespace LocalAI.Pipeline
{
    public static class AiResponseParser
    {
        public static AiResponse Parse(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                return AiResponse.Fallback("Réponse vide.");
            }

            var cleanedJson = ExtractJson(rawResponse);

            try
            {
                var parsed = JsonUtility.FromJson<AiResponse>(cleanedJson);

                if (parsed == null)
                {
                    return AiResponse.Fallback("JSON non lisible.");
                }

                if (string.IsNullOrWhiteSpace(parsed.Dialogue))
                {
                    return AiResponse.Fallback("Dialogue manquant.");
                }

                if (string.IsNullOrWhiteSpace(parsed.Intent))
                {
                    return AiResponse.Fallback("Intent manquant.");
                }

                return parsed;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AI parsing failed: {exception.Message}\nRaw response:\n{rawResponse}");
                return AiResponse.Fallback("Réponse mal formatée.");
            }
        }

        private static string ExtractJson(string text)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');

            if (start < 0 || end < start)
            {
                return text;
            }

            return text.Substring(start, end - start + 1);
        }
    }
}