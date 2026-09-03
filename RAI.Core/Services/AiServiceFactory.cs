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
using AvePoint.RAI.Core.Services.Providers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace AvePoint.RAI.Core.Services
{
    /// <summary>
    /// Factory class for creating AI services
    /// </summary>
    public static class AiServiceFactory
    {
        /// <summary>
        /// Create chat completion service
        /// </summary>
        /// <param name="provider">AI provider configuration</param>
        /// <param name="logger">Optional logger instance</param>
        /// <param name="httpClient">Optional HTTP client instance</param>
        /// <returns>Chat completion service instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when provider is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when chat completion service is not configured</exception>
        /// <exception cref="NotSupportedException">Thrown when provider type is not supported</exception>
        public static IChatCompletionService CreateChatCompletionService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider), "AI provider cannot be null");
            }

            var chatService = provider.GetChatCompletionApiService();
            if (chatService == null)
            {
                throw new InvalidOperationException($"No chat completion service configured for provider '{provider.Name}'");
            }

            try
            {
                return provider.AiType switch
                {
                    AiProviderType.OpenAI => new OpenAiChatCompletionService(provider, logger, httpClient),
                    AiProviderType.AzureOpenAI => new AzureOpenAiChatCompletionService(provider, logger, httpClient),
                    AiProviderType.Google => new GoogleAiChatCompletionService(provider, logger, httpClient),
                    AiProviderType.VertexAI => new VertexAiChatCompletionService(provider, logger, httpClient),
                    _ => throw new NotSupportedException($"Provider type {provider.AiType} is not supported")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create chat completion service for provider '{provider.Name}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create text embedding service
        /// </summary>
        /// <param name="provider">AI provider configuration</param>
        /// <param name="logger">Optional logger instance</param>
        /// <param name="httpClient">Optional HTTP client instance</param>
        /// <returns>Text embedding service instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when provider is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when embedding service is not configured</exception>
        /// <exception cref="NotSupportedException">Thrown when provider type is not supported</exception>
        public static ITextEmbeddingService CreateTextEmbeddingService(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider), "AI provider cannot be null");
            }

            var embeddingService = provider.GetEmbeddingApiService();
            if (embeddingService == null)
            {
                throw new InvalidOperationException($"No embedding service configured for provider '{provider.Name}'");
            }

            try
            {
                return provider.AiType switch
                {
                    AiProviderType.OpenAI => new OpenAiTextEmbeddingService(provider, logger, httpClient),
                    AiProviderType.AzureOpenAI => new AzureOpenAiTextEmbeddingService(provider, logger, httpClient),
                    AiProviderType.Google => new GoogleAiTextEmbeddingService(provider, logger, httpClient),
                    AiProviderType.VertexAI => new VertexAiTextEmbeddingService(provider, logger, httpClient),
                    _ => throw new NotSupportedException($"Provider type {provider.AiType} is not supported")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create text embedding service for provider '{provider.Name}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate provider configuration for chat completion
        /// </summary>
        /// <param name="provider">AI provider to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateChatCompletionProvider(AiProvider provider)
        {
            if (provider == null)
                return false;

            if (string.IsNullOrWhiteSpace(provider.APIKey))
                return false;

            if (provider.AiType != AiProviderType.OpenAI && 
                provider.AiType != AiProviderType.AzureOpenAI && 
                provider.AiType != AiProviderType.Google && 
                provider.AiType != AiProviderType.VertexAI)
                return false;

            var chatService = provider.GetChatCompletionApiService();
            if (chatService == null || string.IsNullOrWhiteSpace(chatService.ModelId))
                return false;

            return true;
        }

        /// <summary>
        /// Validate provider configuration for text embedding
        /// </summary>
        /// <param name="provider">AI provider to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateTextEmbeddingProvider(AiProvider provider)
        {
            if (provider == null)
                return false;

            if (string.IsNullOrWhiteSpace(provider.APIKey))
                return false;

            if (provider.AiType != AiProviderType.OpenAI && 
                provider.AiType != AiProviderType.AzureOpenAI && 
                provider.AiType != AiProviderType.Google && 
                provider.AiType != AiProviderType.VertexAI)
                return false;

            var embeddingService = provider.GetEmbeddingApiService();
            if (embeddingService == null || string.IsNullOrWhiteSpace(embeddingService.ModelId))
                return false;

            return true;
        }
    }

    /// <summary>
    /// Service provider for managing AI service instances
    /// </summary>
    public class AiServiceProvider : IDisposable
    {
        private readonly AiProvider _provider;
        private readonly IAiLogger? _logger;
        private readonly HttpClient? _httpClient;
        private IChatCompletionService? _chatService;
        private ITextEmbeddingService? _embeddingService;
        private bool _disposed = false;

        public AiServiceProvider(AiProvider provider, IAiLogger? logger = null, HttpClient? httpClient = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get or create chat completion service
        /// </summary>
        /// <returns>Chat completion service instance</returns>
        public IChatCompletionService GetChatCompletionService()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AiServiceProvider));

            if (_chatService == null)
            {
                _chatService = AiServiceFactory.CreateChatCompletionService(_provider, _logger, _httpClient);
            }

            return _chatService;
        }

        /// <summary>
        /// Get or create text embedding service
        /// </summary>
        /// <returns>Text embedding service instance</returns>
        public ITextEmbeddingService GetTextEmbeddingService()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AiServiceProvider));

            if (_embeddingService == null)
            {
                _embeddingService = AiServiceFactory.CreateTextEmbeddingService(_provider, _logger, _httpClient);
            }

            return _embeddingService;
        }

        /// <summary>
        /// Get AI provider information
        /// </summary>
        /// <returns>AI provider</returns>
        public AiProvider GetProvider()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AiServiceProvider));

            return _provider;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _chatService?.Dispose();
                _embeddingService?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
