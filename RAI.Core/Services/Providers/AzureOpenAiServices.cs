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
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using AvePoint.RAI.Core.Models;
using SKChatCompletion = Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService;
using AvePoint.RAI.Core.Utils;

namespace AvePoint.RAI.Core.Services.Providers
{
    /// <summary>
    /// Azure OpenAI chat completion service implementation using Semantic Kernel
    /// </summary>
    public class AzureOpenAiChatCompletionService : BaseChatCompletionService
    {
        private const string MaxCompletionTokensCompatibleDeploymentName = "gpt-5.1";
        private readonly Kernel _kernel;
        private readonly SKChatCompletion _chatService;

        public AzureOpenAiChatCompletionService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null) : base(provider, logger, httpClient)
        {
            if (provider.AiType != AiProviderType.AzureOpenAI)
            {
                throw new ArgumentException($"Provider type must be AzureOpenAI, but was {provider.AiType}");
            }
            if (string.IsNullOrEmpty(_apiService.DeploymentName))
            {
                throw new ArgumentException($"DeploymentName must be declare");
            }

            var builder = Kernel.CreateBuilder();
            var httpClientInstance = _httpClient ?? new HttpClient();
            httpClientInstance.Timeout = TimeSpan.FromMinutes(5);

#if DEBUG
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: _apiService.DeploymentName,
                endpoint: _provider.APIEndpoint,
                apiKey: _provider.APIKey,
                modelId: _apiService.ModelId,
                httpClient: httpClientInstance);
#else
            // For Azure OpenAI, create properly configured credentials for cloud environment
            var credential = AzureCloudEnvironmentHelper.CreateAzureOpenAICredential(_provider.APIEndpoint, _logger);
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: _apiService.DeploymentName,
                endpoint: _provider.APIEndpoint,
                credentials: credential,
                modelId: _apiService.ModelId,
                httpClient: httpClientInstance);
#endif

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<SKChatCompletion>();

            _logger.LogInfo("Azure OpenAI chat completion service initialized for provider {0} with deployment {1}", _provider.Code, _apiService.ModelId);
        }

        protected override async Task<ChatCompletionResponse> GetChatCompletionInternalAsync(
            IEnumerable<ChatMessage> messages, ChatCompletionSettings settings, string modelId)
        {
            try
            {
                _logger.LogDebug("Starting Azure OpenAI chat completion request with deployment {0}", modelId);

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

                var useMaxCompletionTokens = string.Equals(
                    _apiService.DeploymentName,
                    MaxCompletionTokensCompatibleDeploymentName,
                    StringComparison.OrdinalIgnoreCase);

                var extensionData = new Dictionary<string, object>
                {
                    { "n", settings.ResultsPerPrompt ?? 1 },
                    { "results_per_prompt", settings.ResultsPerPrompt ?? 1 }
                };

                // Configure execution settings for Azure OpenAI using provided settings
                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    ModelId = modelId,
                    Temperature = (float)settings.Temperature,
                    TopP = (float)settings.TopP,
                    FrequencyPenalty = (float)settings.FrequencyPenalty,
                    PresencePenalty = (float)settings.PresencePenalty,
                    StopSequences = settings.StopSequences?.ToList(),
                    ExtensionData = extensionData
                };

                if (useMaxCompletionTokens)
                {
                    extensionData["max_completion_tokens"] = settings.MaxTokens;
                }
                else
                {
                    executionSettings.MaxTokens = settings.MaxTokens;
                }

                // Overall operation timeout (independent of network read timeout). Adjustable constant for now.
                // Measure request duration
                var stopwatch = Stopwatch.StartNew();
                var result = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings);
                stopwatch.Stop();
                _logger.LogInfo("Azure OpenAI chat completion request duration: {0} ms", stopwatch.ElapsedMilliseconds);

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

                _logger.LogDebug("Azure OpenAI chat completion request completed. Tokens used: {0}", tokensUsed);

                return new ChatCompletionResponse(
                    Content: result.Content ?? string.Empty,
                    ModelId: modelId,
                    TokensUsed: tokensUsed,
                    FinishReason: finishReason);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get chat completion from Azure OpenAI: {0}", ex, ex.Message);
                throw new InvalidOperationException($"Failed to get chat completion from Azure OpenAI: {ex.Message}", ex);
            }
        }

        protected override void DisposeInternal()
        {
            // Kernel doesn't implement IDisposable in this version
            // Resources will be cleaned up by GC
        }
    }

    /// <summary>
    /// Azure OpenAI text embedding service implementation using Semantic Kernel
    /// </summary>
    public class AzureOpenAiTextEmbeddingService : BaseTextEmbeddingService
    {
        private readonly Kernel _kernel;
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public AzureOpenAiTextEmbeddingService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null) : base(provider, logger, httpClient)
        {
            if (provider.AiType != AiProviderType.AzureOpenAI)
            {
                throw new ArgumentException($"Provider type must be AzureOpenAI, but was {provider.AiType}");
            }
            if (string.IsNullOrEmpty(_apiService.DeploymentName))
            {
                throw new ArgumentException($"DeploymentName must be declare");
            }

            var builder = Kernel.CreateBuilder();
            var httpClientInstance = _httpClient ?? new HttpClient();
            httpClientInstance.Timeout = TimeSpan.FromMinutes(5);

#if DEBUG
            builder.AddAzureOpenAITextEmbeddingGeneration(
                deploymentName: _apiService.DeploymentName,
                endpoint: _provider.APIEndpoint,
                apiKey: _provider.APIKey,
                modelId: _apiService.ModelId,
                serviceId: null,
                httpClient: httpClientInstance);
#else
            // For Azure OpenAI, create properly configured credentials for cloud environment
            var credential = AzureCloudEnvironmentHelper.CreateAzureOpenAICredential(_provider.APIEndpoint, _logger);
            
            builder.AddAzureOpenAITextEmbeddingGeneration(
                deploymentName: _apiService.DeploymentName,
                endpoint: _provider.APIEndpoint,
                modelId: _apiService.ModelId,
                serviceId: null,
                credential: credential,
                httpClient: httpClientInstance);
#endif


            _kernel = builder.Build();
            _embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            _logger.LogInfo("Azure OpenAI text embedding service initialized for provider {0} with deployment {1}", _provider.Code, _apiService.ModelId);
        }

        protected override async Task<EmbeddingResponse> GetEmbeddingsInternalAsync(
            IEnumerable<string> texts, string modelId)
        {
            try
            {
                var textList = texts.ToList();
                _logger.LogDebug("Starting Azure OpenAI embeddings request for {0} texts with deployment {1}", textList.Count, modelId);

                var embeddings = new List<float[]>();
                var totalTokens = 0;

                // Azure OpenAI supports batch processing
                var results = await _embeddingService.GenerateEmbeddingsAsync(textList);
                embeddings.AddRange(results.Select(r => r.ToArray()));

                // Estimate token usage for Azure OpenAI
                totalTokens = textList.Sum(text => EstimateTokenCount(text));

                _logger.LogDebug("Azure OpenAI embeddings request completed. Estimated tokens used: {0}", totalTokens);

                return new EmbeddingResponse(
                    Embeddings: embeddings,
                    ModelId: modelId,
                    TokensUsed: totalTokens);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get embeddings from Azure OpenAI: {0}", ex, ex.Message);
                throw new InvalidOperationException($"Failed to get embeddings from Azure OpenAI: {ex.Message}", ex);
            }
        }

        protected override void DisposeInternal()
        {
            // Kernel doesn't implement IDisposable in this version
            // Resources will be cleaned up by GC
        }
    }
}
