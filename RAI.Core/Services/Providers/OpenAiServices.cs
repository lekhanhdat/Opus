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
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using AvePoint.RAI.Core.Models;
using SKChatCompletion = Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService;

namespace AvePoint.RAI.Core.Services.Providers
{
    /// <summary>
    /// OpenAI chat completion service implementation using Semantic Kernel
    /// </summary>
    public class OpenAiChatCompletionService : BaseChatCompletionService
    {
        private readonly Kernel _kernel;
        private readonly SKChatCompletion _chatService;

        public OpenAiChatCompletionService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null) : base(provider, logger, httpClient)
        {
            if (provider.AiType != AiProviderType.OpenAI)
            {
                throw new ArgumentException($"Provider type must be OpenAI, but was {provider.AiType}");
            }

            var builder = Kernel.CreateBuilder();
            
            // For regular OpenAI
            builder.AddOpenAIChatCompletion(
                modelId: _apiService.ModelId,
                apiKey: _provider.APIKey,
                httpClient: _httpClient,
                serviceId: null);

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<SKChatCompletion>();
            
            _logger.LogInfo("OpenAI chat completion service initialized for provider {0} with model {1}", _provider.Code, _apiService.ModelId);
        }

        protected override async Task<ChatCompletionResponse> GetChatCompletionInternalAsync(
            IEnumerable<ChatMessage> messages, ChatCompletionSettings settings, string modelId)
        {
            try
            {
                _logger.LogDebug("Starting OpenAI chat completion request with model {0}", modelId);
                
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

                // Configure execution settings for OpenAI using provided settings
                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    ModelId = modelId,
                    MaxTokens = settings.MaxTokens,
                    Temperature = (float)settings.Temperature,
                    TopP = (float)settings.TopP,
                    FrequencyPenalty = (float)settings.FrequencyPenalty,
                    PresencePenalty = (float)settings.PresencePenalty,
                    StopSequences = settings.StopSequences?.ToList(),
                    ExtensionData = new Dictionary<string, object>
                    {
                        { "n", settings.ResultsPerPrompt ?? 1 },
                        { "results_per_prompt", settings.ResultsPerPrompt ?? 1 }
                    }
                };

                // Get chat completion from Semantic Kernel
                var result = await _chatService.GetChatMessageContentAsync(
                    chatHistory, 
                    executionSettings);

                // Extract usage information if available
                var tokensUsed = 0;
                var finishReason = "stop";

                if (result.Metadata != null)
                {
                    // Try to get usage information from metadata
                    if (result.Metadata.TryGetValue("Usage", out var usageObj))
                    {
                        // Handle usage information based on available metadata
                        if (usageObj != null)
                        {
                            // Try to extract token count from different possible formats
                            var usageString = usageObj.ToString();
                            if (int.TryParse(usageString, out var parsedTokens))
                            {
                                tokensUsed = parsedTokens;
                            }
                        }
                    }

                    // Try to get finish reason from metadata
                    if (result.Metadata.TryGetValue("FinishReason", out var finishReasonObj))
                    {
                        finishReason = finishReasonObj?.ToString() ?? "stop";
                    }
                }

                _logger.LogDebug("OpenAI chat completion request completed. Tokens used: {0}", tokensUsed);

                return new ChatCompletionResponse(
                    Content: result.Content ?? string.Empty,
                    ModelId: modelId,
                    TokensUsed: tokensUsed,
                    FinishReason: finishReason);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get chat completion from OpenAI: {0}", ex, ex.Message);
                throw new InvalidOperationException($"Failed to get chat completion from OpenAI: {ex.Message}", ex);
            }
        }

        protected override void DisposeInternal()
        {
            // Kernel doesn't implement IDisposable in this version
            // Resources will be cleaned up by GC
        }
    }

    /// <summary>
    /// OpenAI text embedding service implementation using Semantic Kernel
    /// </summary>
    public class OpenAiTextEmbeddingService : BaseTextEmbeddingService
    {
        private readonly Kernel _kernel;
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public OpenAiTextEmbeddingService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null) : base(provider, logger, httpClient)
        {
            if (provider.AiType != AiProviderType.OpenAI)
            {
                throw new ArgumentException($"Provider type must be OpenAI, but was {provider.AiType}");
            }

            var builder = Kernel.CreateBuilder();
            
            // For regular OpenAI
            builder.AddOpenAITextEmbeddingGeneration(
                modelId: _apiService.ModelId,
                apiKey: _provider.APIKey,
                httpClient: _httpClient,
                serviceId: null);

            _kernel = builder.Build();
            _embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            
            _logger.LogInfo("OpenAI text embedding service initialized for provider {0} with model {1}", _provider.Code, _apiService.ModelId);
        }

        protected override async Task<EmbeddingResponse> GetEmbeddingsInternalAsync(
            IEnumerable<string> texts, string modelId)
        {
            try
            {
                var textList = texts.ToList();
                _logger.LogDebug("Starting OpenAI embeddings request for {0} texts with model {1}", textList.Count, modelId);
                
                var embeddings = new List<float[]>();
                var totalTokens = 0;

                // OpenAI supports batch processing
                var results = await _embeddingService.GenerateEmbeddingsAsync(textList);
                embeddings.AddRange(results.Select(r => r.ToArray()));
                
                // Estimate token usage for OpenAI
                totalTokens = textList.Sum(text => EstimateTokenCount(text));

                _logger.LogDebug("OpenAI embeddings request completed. Estimated tokens used: {0}", totalTokens);

                return new EmbeddingResponse(
                    Embeddings: embeddings,
                    ModelId: modelId,
                    TokensUsed: totalTokens);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get embeddings from OpenAI: {0}", ex, ex.Message);
                throw new InvalidOperationException($"Failed to get embeddings from OpenAI: {ex.Message}", ex);
            }
        }

        protected override void DisposeInternal()
        {
            // Kernel doesn't implement IDisposable in this version
            // Resources will be cleaned up by GC
        }
    }
}
