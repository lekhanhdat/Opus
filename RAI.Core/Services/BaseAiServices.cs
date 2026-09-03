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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AvePoint.RAI.Core.Models;

namespace AvePoint.RAI.Core.Services
{
    /// <summary>
    /// Abstract base class for chat completion service implementations
    /// </summary>
    public abstract class BaseChatCompletionService : IChatCompletionService
    {
        protected readonly AiProvider _provider;
        protected readonly ApiService _apiService;
        protected readonly IAiLogger _logger;
        protected readonly HttpClient? _httpClient;
        protected bool _disposed = false;

        protected BaseChatCompletionService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _apiService = provider.GetChatCompletionApiService() 
                ?? throw new InvalidOperationException($"No chat completion service found for provider {provider.Name}");
            _logger = logger ?? new ConsoleAiLogger();
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get chat completion response
        /// </summary>
        /// <param name="messages">Chat messages</param>
        /// <param name="modelId">Model ID (optional, uses default if not specified)</param>
        /// <returns>Chat completion response</returns>
        public async Task<ChatCompletionResponse> GetChatCompletionAsync(
            IEnumerable<ChatMessage> messages, string? modelId = null)
        {
            return await GetChatCompletionAsync(messages, ChatCompletionSettings.Default, modelId);
        }

        /// <summary>
        /// Get chat completion response with custom execution settings
        /// </summary>
        /// <param name="messages">Chat messages</param>
        /// <param name="settings">Configuration settings for the chat completion request</param>
        /// <param name="modelId">Model ID (optional, uses default if not specified)</param>
        /// <returns>Chat completion response</returns>
        public async Task<ChatCompletionResponse> GetChatCompletionAsync(
            IEnumerable<ChatMessage> messages, ChatCompletionSettings settings, string? modelId = null)
        {
            if (messages == null || !messages.Any())
            {
                throw new ArgumentException("Messages cannot be null or empty", nameof(messages));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var targetModelId = modelId ?? _apiService.ModelId;
            
            // Validate model ID if multiple models are available
            if (_apiService.ModelIds != null && _apiService.ModelIds.Length > 0)
            {
                if (!_apiService.ModelIds.Contains(targetModelId))
                {
                    throw new ArgumentException($"Model '{targetModelId}' is not available for this service");
                }
            }

            _logger.LogInfo("Starting chat completion request for provider {0} with model {1}", _provider.Code, targetModelId);
            
            try
            {
                var response = await GetChatCompletionInternalAsync(messages, settings, targetModelId);
                _logger.LogInfo("Chat completion completed successfully. Tokens used: {0}", response.TokensUsed);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get chat completion response: {0}", ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Provider-specific implementation
        /// </summary>
        /// <param name="messages">Chat messages</param>
        /// <param name="modelId">Model ID</param>
        /// <returns>Chat completion response</returns>
        protected async Task<ChatCompletionResponse> GetChatCompletionInternalAsync(
            IEnumerable<ChatMessage> messages, string modelId)
        {
            return await GetChatCompletionInternalAsync(messages, ChatCompletionSettings.Default, modelId);
        }

        /// <summary>
        /// Provider-specific implementation with custom settings
        /// </summary>
        /// <param name="messages">Chat messages</param>
        /// <param name="settings">Configuration settings for the chat completion request</param>
        /// <param name="modelId">Model ID</param>
        /// <returns>Chat completion response</returns>
        protected abstract Task<ChatCompletionResponse> GetChatCompletionInternalAsync(
            IEnumerable<ChatMessage> messages, ChatCompletionSettings settings, string modelId);

        /// <summary>
        /// Get available models for chat completion
        /// </summary>
        /// <returns>List of available model IDs</returns>
        public IEnumerable<string> GetAvailableModels()
        {
            if (_apiService.ModelIds != null && _apiService.ModelIds.Length > 0)
            {
                return _apiService.ModelIds;
            }
            
            return new[] { _apiService.ModelId };
        }

        /// <summary>
        /// Get AI provider information
        /// </summary>
        /// <returns>AI provider</returns>
        public AiProvider GetProvider()
        {
            return _provider;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                DisposeInternal();
                _disposed = true;
            }
        }

        /// <summary>
        /// Provider-specific cleanup logic
        /// </summary>
        protected abstract void DisposeInternal();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Abstract base class for text embedding service implementations
    /// </summary>
    public abstract class BaseTextEmbeddingService : ITextEmbeddingService
    {
        protected readonly AiProvider _provider;
        protected readonly ApiService _apiService;
        protected readonly IAiLogger _logger;
        protected readonly HttpClient? _httpClient;
        protected bool _disposed = false;

        protected BaseTextEmbeddingService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _apiService = provider.GetEmbeddingApiService() 
                ?? throw new InvalidOperationException($"No embedding service found for provider {provider.Name}");
            _logger = logger ?? new ConsoleAiLogger();
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get text embeddings
        /// </summary>
        /// <param name="texts">Input texts</param>
        /// <param name="modelId">Model ID (optional, uses default if not specified)</param>
        /// <returns>Text embeddings</returns>
        public async Task<EmbeddingResponse> GetEmbeddingsAsync(
            IEnumerable<string> texts, string? modelId = null)
        {
            if (texts == null || !texts.Any())
            {
                throw new ArgumentException("Texts cannot be null or empty", nameof(texts));
            }

            var targetModelId = modelId ?? _apiService.ModelId;
            
            // Validate model ID if multiple models are available
            if (_apiService.ModelIds != null && _apiService.ModelIds.Length > 0)
            {
                if (!_apiService.ModelIds.Contains(targetModelId))
                {
                    throw new ArgumentException($"Model '{targetModelId}' is not available for this service");
                }
            }

            _logger.LogInfo("Starting embedding request for provider {0} with model {1}", _provider.Code, targetModelId);
            
            try
            {
                var response = await GetEmbeddingsInternalAsync(texts, targetModelId);
                _logger.LogInfo("Embedding completed successfully. Tokens used: {0}", response.TokensUsed);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get embeddings response: {0}", ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Provider-specific implementation
        /// </summary>
        /// <param name="texts">Input texts</param>
        /// <param name="modelId">Model ID</param>
        /// <returns>Text embeddings</returns>
        protected abstract Task<EmbeddingResponse> GetEmbeddingsInternalAsync(
            IEnumerable<string> texts, string modelId);

        /// <summary>
        /// Get available models for text embedding
        /// </summary>
        /// <returns>List of available model IDs</returns>
        public IEnumerable<string> GetAvailableModels()
        {
            if (_apiService.ModelIds != null && _apiService.ModelIds.Length > 0)
            {
                return _apiService.ModelIds;
            }
            
            return new[] { _apiService.ModelId };
        }

        /// <summary>
        /// Get AI provider information
        /// </summary>
        /// <returns>AI provider</returns>
        public AiProvider GetProvider()
        {
            return _provider;
        }

        /// <summary>
        /// Estimate token count for a given text (rough approximation)
        /// </summary>
        /// <param name="text">Input text</param>
        /// <returns>Estimated token count</returns>
        protected static int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Rough estimation: 1 token ≈ 4 characters for English text
            return Math.Max(1, text.Length / 4);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                DisposeInternal();
                _disposed = true;
            }
        }

        /// <summary>
        /// Provider-specific cleanup logic
        /// </summary>
        protected abstract void DisposeInternal();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
