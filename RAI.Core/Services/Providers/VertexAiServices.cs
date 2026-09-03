/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.RAI.Core.Auth;
using AvePoint.RAI.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SKChatCompletion = Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService;

namespace AvePoint.RAI.Core.Services.Providers
{
    /// <summary>
    /// Google Vertex AI service implementation for chat completion
    /// </summary>
    public class VertexAiChatCompletionService : BaseChatCompletionService
    {
        private readonly Kernel _kernel;
        private readonly SKChatCompletion _chatService;

        public VertexAiChatCompletionService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null) : base(provider, logger, httpClient)
        {
            if (provider.AiType != AiProviderType.VertexAI)
            {
                throw new ArgumentException($"Provider type must be VertexAI, but was {provider.AiType}");
            }

            var kernelBuilder = Kernel.CreateBuilder();

            // Use AddVertexAIGeminiChatCompletion for Vertex AI
            kernelBuilder.AddVertexAIGeminiChatCompletion(
                modelId: _apiService.ModelId,
                bearerTokenProvider: async () => await GoogleCloudAuth.GetVertexAIAccessTokenAsync(
                    provider.ServiceAccountEmail,
                    provider.PrivateKey,
                    GoogleCloudAuth.GetVertexAIScopes()),
                projectId: provider.ProjectId ?? throw new ArgumentException("ProjectId is required for Vertex AI"),
                location: provider.Location ?? "us-central1",
                httpClient: _httpClient);

            _kernel = kernelBuilder.Build();
            _chatService = _kernel.GetRequiredService<SKChatCompletion>();

            _logger.LogInfo("Vertex AI chat completion service initialized for provider {0} with model {1}", _provider.Code, _apiService.ModelId);
        }

        protected override async Task<ChatCompletionResponse> GetChatCompletionInternalAsync(
            IEnumerable<ChatMessage> messages, ChatCompletionSettings settings, string modelId)
        {
            return await AuthenticationRetryHelper.ExecuteWithAuthRetryAsync(async () =>
            {
                // Convert our ChatMessage to Semantic Kernel ChatHistory
                var chatHistory = new ChatHistory();
                foreach (var message in messages)
                {
                    switch (message.Role.ToLowerInvariant())
                    {
                        case "system":
                            chatHistory.AddSystemMessage(message.Content);
                            break;
                        case "user":
                            chatHistory.AddUserMessage(message.Content);
                            break;
                        case "assistant":
                            chatHistory.AddAssistantMessage(message.Content);
                            break;
                        default:
                            // Default to user message for unknown roles
                            chatHistory.AddUserMessage(message.Content);
                            break;
                    }
                }

                // Configure execution settings for Vertex AI using provided settings
                var executionSettings = new GeminiPromptExecutionSettings
                {
                    ModelId = modelId,
                    MaxTokens = null,
                    Temperature = (float)settings.Temperature,
                    TopP = (float)settings.TopP,
                    StopSequences = settings.StopSequences?.ToList()
                };

                // Get completion
                var result = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings);

                // Calculate token usage (approximation since Vertex AI might not provide exact counts)
                var tokenUsage = EstimateTokenUsage(messages, result.Content ?? string.Empty);

                return new ChatCompletionResponse(
                    Content: result.Content ?? string.Empty,
                    ModelId: modelId,
                    TokensUsed: tokenUsage,
                    FinishReason: result.Metadata?.GetValueOrDefault("finish_reason")?.ToString()
                );
            }, "Vertex AI chat completion");
        }

        protected override void DisposeInternal()
        {
            // Vertex AI service cleanup if needed
            // The Semantic Kernel services are managed by the kernel lifecycle
        }

        private static int EstimateTokenUsage(IEnumerable<ChatMessage> messages, string response)
        {
            // Simple token estimation (approximately 4 characters per token)
            var inputLength = messages.Sum(m => m.Content.Length);
            var outputLength = response?.Length ?? 0;
            return (inputLength + outputLength) / 4;
        }
    }

    /// <summary>
    /// Google Vertex AI service implementation for text embeddings
    /// </summary>
    public class VertexAiTextEmbeddingService : BaseTextEmbeddingService
    {
        private readonly string _projectId;
        private readonly string _location;
        private readonly string _modelId;
        private readonly string _saEmail;
        private readonly string _saKey;

        public VertexAiTextEmbeddingService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null) : base(provider, logger, httpClient)
        {
            if (provider.AiType != AiProviderType.VertexAI)
                throw new ArgumentException($"Provider type must be VertexAI, but was {provider.AiType}");

            _projectId = provider.ProjectId ?? throw new ArgumentException("ProjectId is required for Vertex AI");
            _location = provider.Location ?? "us-central1";
            _modelId = _apiService.ModelId;
            _saEmail = provider.ServiceAccountEmail!;
            _saKey = provider.PrivateKey!;

            _logger.LogInfo("Vertex AI REST embedding service initialized for provider {0} with model {1}",
                _provider.Code, _apiService.ModelId);
        }


        protected override async Task<EmbeddingResponse> GetEmbeddingsInternalAsync(
            IEnumerable<string> texts, string modelId)
        {
            return await AuthenticationRetryHelper.ExecuteWithAuthRetryAsync(async () =>
            {
                var textList = texts.ToList();
                _logger.LogDebug("Starting Vertex AI REST embeddings for {0} texts with model {1}",
                    textList.Count, modelId);

                var embeddings = new List<float[]>(capacity: textList.Count);

                foreach (var text in textList)
                {
                    var vec = await GetTextEmbeddingWithTaskTypeAsync(text, "SEMANTIC_SIMILARITY");
                    embeddings.Add(vec);
                }

                var tokenUsage = textList.Sum(text => EstimateTokenCount(text));

                _logger.LogDebug("Vertex AI REST embeddings done. Estimated tokens used: {0}", tokenUsage);

                return new EmbeddingResponse(
                    Embeddings: embeddings,
                    ModelId: modelId,
                    TokensUsed: tokenUsage
                );
            }, "Vertex AI embeddings (REST)");
        }

        private async Task<float[]> GetTextEmbeddingWithTaskTypeAsync(string text, string taskType = "SEMANTIC_SIMILARITY")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentException("Text cannot be null or empty.", nameof(text));

                if (string.IsNullOrWhiteSpace(taskType))
                    taskType = "SEMANTIC_SIMILARITY";

                _logger?.LogDebug("VertexAI Embedding start. len={0}, taskType={1}, model={2}", text.Length, taskType, _modelId);

                string accessToken;
                try
                {
                    accessToken = await GoogleCloudAuth.GetVertexAIAccessTokenAsync(
                        _saEmail, _saKey, GoogleCloudAuth.GetVertexAIScopes());
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Failed to obtain Vertex AI access token: {0}", ex);
                    throw new InvalidOperationException("Failed to obtain Vertex AI access token.", ex);
                }

                var url =
                    $"https://{_location}-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/{_modelId}:predict";

                // Payload
                var payload = new
                {
                    instances = new[]
                    {
                        new
                        {
                            content = text,
                            task_type = taskType
                        }
                    }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.UserAgent.ParseAdd("YourApp/1.0");

                using var resp = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger?.LogError("Vertex AI call failed");
                    throw new HttpRequestException($"Vertex AI call failed: {(int)resp.StatusCode} {resp.StatusCode} - {body}");
                }

                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("predictions", out var predictions) ||
                        predictions.ValueKind != JsonValueKind.Array ||
                        predictions.GetArrayLength() == 0)
                    {
                        throw new InvalidOperationException("Vertex AI response missing 'predictions'.");
                    }

                    var first = predictions[0];

                    if (!first.TryGetProperty("embeddings", out var embeddings) ||
                        !embeddings.TryGetProperty("values", out var valuesJson) ||
                        valuesJson.ValueKind != JsonValueKind.Array)
                    {
                        throw new InvalidOperationException("Vertex AI response missing 'embeddings.values'.");
                    }

                    var values = valuesJson
                        .EnumerateArray()
                        .Where(v => v.ValueKind == JsonValueKind.Number)
                        .Select(v => v.GetSingle())
                        .ToArray();

                    if (values.Length == 0)
                        throw new InvalidOperationException("Empty embedding returned from Vertex AI.");

                    _logger?.LogDebug("VertexAI Embedding success. dims={0}", values.Length);
                    return values;
                }
                catch (JsonException jex)
                {
                    _logger?.LogError("Failed to parse Vertex AI response JSON: {0} | Payload: {1}", jex, body);
                    throw new InvalidOperationException("Failed to parse Vertex AI response JSON.", jex);
                }
            }
            catch (TaskCanceledException tce) when (!tce.CancellationToken.IsCancellationRequested)
            {
                _logger?.LogError("Vertex AI request timed out: {0}", tce);
                throw new TimeoutException("Vertex AI request timed out.", tce);
            }
            catch (HttpRequestException hrex)
            {
                _logger?.LogError("HTTP error calling Vertex AI: {0}", hrex);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Unexpected error generating embedding: {0}", ex);
                throw;
            }
        }



        protected override void DisposeInternal()
        {
            // Vertex AI service cleanup if needed
            // The Semantic Kernel services are managed by the kernel lifecycle
        }
    }
}
