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
using AvePoint.RA.Common.AI;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RAI.Core;
using AvePoint.RAI.Core.Auth;
using AvePoint.RAI.Core.Services;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace AvePoint.RA.Common.AI.VertexAI
{
    /// <summary>
    /// Unified VertexAI Client
    /// Provides centralized access to both VertexAI chat completion and text embedding services
    /// </summary>
    public sealed class VertexAIClient : IDisposable
    {
        private static readonly Lazy<VertexAIClient> _instance = new(() => new VertexAIClient());
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(VertexAIClient));
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        
        private IChatCompletionService? _chatService;
        private ITextEmbeddingService? _embeddingService;
        private HttpClient? _httpClient;
        private volatile bool _isInitialized = false;
        private volatile bool _disposed = false;

        /// <summary>
        /// Gets the singleton instance of the VertexAI client
        /// </summary>
        public static VertexAIClient Instance => _instance.Value;

        /// <summary>
        /// Client type name for logging
        /// </summary>
        private string ClientTypeName => "VertexAI Client";

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private VertexAIClient()
        {
        }

        /// <summary>
        /// Initialize the client with VertexAI configuration
        /// </summary>
        /// <returns>Task representing the initialization operation</returns>
        public async Task InitializeAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(ClientTypeName);

            await AiClientUtils.ExecuteInitializationAsync(
                () => _isInitialized,
                _semaphore,
                async () =>
                {
                    await InitializeServiceAsync();
                    _isInitialized = true;
                },
                ClientTypeName);
        }

        /// <summary>
        /// Check if the client is initialized and ready to use
        /// </summary>
        /// <returns>True if initialized</returns>
        public bool IsInitialized => _isInitialized && !_disposed;

        /// <summary>
        /// Get the chat completion service instance
        /// </summary>
        /// <returns>Chat completion service</returns>
        /// <exception cref="InvalidOperationException">Thrown when client is not initialized or chat service is not available</exception>
        public async Task<IChatCompletionService> GetChatServiceAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(ClientTypeName);

            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            if (_chatService == null)
            {
                throw new InvalidOperationException("Chat service is not available. Ensure VERTEX_AI_CHAT_MODEL_NAME is configured and the client is properly initialized.");
            }

            return _chatService;
        }

        /// <summary>
        /// Get the text embedding service instance
        /// </summary>
        /// <returns>Text embedding service</returns>
        /// <exception cref="InvalidOperationException">Thrown when client is not initialized or embedding service is not available</exception>
        public async Task<ITextEmbeddingService> GetEmbeddingServiceAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(ClientTypeName);

            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            if (_embeddingService == null)
            {
                throw new InvalidOperationException("Embedding service is not available. Ensure VERTEX_AI_TEXT_MODEL_NAME is configured and the client is properly initialized.");
            }

            return _embeddingService;
        }

        /// <summary>
        /// Initialize or refresh both chat and embedding services based on available configuration
        /// </summary>
        private Task InitializeServiceAsync()
        {
            // Get basic configuration
            var (projectId, serviceAccount, chatModelName, privateKey, location) = GetVertexAIConfig();
            
            // Get embedding model name
            var embeddingModelName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_TEXT_MODEL_NAME];

            // Create development HTTP client if needed
            if (_httpClient == null)
            {
                _httpClient = CreateDevelopmentHttpClient();
            }

            var loggerAdapter = new RALoggerAdapter(_logger);

            // Initialize Chat Service only if chat model is configured
            if (!string.IsNullOrWhiteSpace(chatModelName))
            {
                InitializeChatService(chatModelName, loggerAdapter);
                _logger.Info("VertexAI Chat service initialized successfully - Chat model: {0}", chatModelName);
            }
            else
            {
                _logger.Info("VertexAI Chat service not initialized - VERTEX_AI_CHAT_MODEL_NAME configuration not found");
            }

            // Initialize Embedding Service only if embedding model is configured
            if (!string.IsNullOrWhiteSpace(embeddingModelName))
            {
                InitializeEmbeddingService(embeddingModelName, loggerAdapter);
                _logger.Info("VertexAI Embedding service initialized successfully - Embedding model: {0}", embeddingModelName);
            }
            else
            {
                _logger.Info("VertexAI Embedding service not initialized - VERTEX_AI_TEXT_MODEL_NAME configuration not found");
            }

            _logger.Info("VertexAI client initialization completed - Project: {0}, Location: {1}", projectId, location);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get common VertexAI configuration
        /// </summary>
        /// <returns>Configuration tuple</returns>
        private (string projectId, string serviceAccount, string modelName, string privateKey, string location) GetVertexAIConfig()
        {
            var projectId = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_PROJECT_ID];
            var serviceAccount = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_SERVICE_ACCOUNT];
            var modelName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_CHAT_MODEL_NAME];
            var privateKey = RMGlobalConfiguration.EncryptConfig.GetVertexAIPrivateKey();
            var location = AiClientUtils.GetConfigurationWithDefault(RMAppSettingKey.VERTEX_AI_LOCATION, "us-central1");

            // Validate required configuration using utility method
            AiClientUtils.ValidateRequiredConfiguration(projectId, nameof(RMAppSettingKey.VERTEX_AI_PROJECT_ID), ClientTypeName);
            // Note: modelName validation is now handled in InitializeServiceAsync based on availability

            return (projectId, serviceAccount, modelName, privateKey, location);
        }

        /// <summary>
        /// Create common VertexAI provider using RAI.Core
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="providerCode">Provider code</param>
        /// <param name="serviceName">Service name</param>
        /// <returns>Configured AI provider</returns>
        private AiProvider CreateVertexAIProvider(string providerName, string providerCode, string serviceName)
        {
            var (projectId, serviceAccount, modelName, privateKey, location) = GetVertexAIConfig();

            return AiClientUtils.CreateAiProvider(providerName, providerCode, AiProviderType.VertexAI, aiProvider =>
            {
                aiProvider.ServiceAccountEmail = serviceAccount;
                aiProvider.PrivateKey = privateKey;
                aiProvider.ProjectId = projectId;
                aiProvider.Location = location;

                // Add service configuration
                aiProvider.ApiServices.Add(new ApiService(
                    Name: serviceName,
                    ModelId: modelName
                ));
            });
        }

        /// <summary>
        /// Create development HTTP client with proxy configuration for debugging
        /// </summary>
        /// <returns>HTTP client configured for development proxy</returns>
        private HttpClient CreateDevelopmentHttpClient()
        {
            return AiClientUtils.CreateDevelopmentHttpClient(ClientTypeName, 5);
        }

        /// <summary>
        /// Initialize chat completion service
        /// </summary>
        private void InitializeChatService(string chatModelName, RALoggerAdapter loggerAdapter)
        {
            // Dispose old service if exists
            _chatService?.Dispose();

            // Create VertexAI provider for chat using base class method
            var chatProvider = CreateVertexAIProvider("VertexAI-Chat", "vertexai-chat", "chat");
            
            // Override the model for chat service
            if (chatProvider.ApiServices.Count > 0)
            {
                chatProvider.ApiServices[0] = new ApiService(
                    Name: "chat",
                    ModelId: chatModelName
                );
            }

            // Create chat completion service
            _chatService = AiServiceFactory.CreateChatCompletionService(chatProvider, loggerAdapter, _httpClient);
        }

        /// <summary>
        /// Initialize text embedding service
        /// </summary>
        private void InitializeEmbeddingService(string embeddingModelName, RALoggerAdapter loggerAdapter)
        {
            // Dispose old service if exists
            _embeddingService?.Dispose();

            // Create VertexAI provider for embedding using base class method
            var embeddingProvider = CreateVertexAIProvider("VertexAI-Embedding", "vertexai-embedding", "embeddings");
            
            // Override the model for embedding service
            if (embeddingProvider.ApiServices.Count > 0)
            {
                embeddingProvider.ApiServices[0] = new ApiService(
                    Name: "embeddings",
                    ModelId: embeddingModelName
                );
            }

            // Create text embedding service
            _embeddingService = AiServiceFactory.CreateTextEmbeddingService(embeddingProvider, loggerAdapter, _httpClient);
        }

        /// <summary>
        /// Dispose service-specific resources
        /// </summary>
        private void DisposeService()
        {
            AiClientUtils.SafeDispose(_chatService, "Chat Service", ClientTypeName);
            AiClientUtils.SafeDispose(_embeddingService, "Embedding Service", ClientTypeName);
        }

        /// <summary>
        /// Dispose the client and release resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                DisposeService();
                AiClientUtils.SafeDispose(_httpClient, "HTTP Client", ClientTypeName);
                _disposed = true;
                _logger.Info("{0} disposed", ClientTypeName);
            }
            catch (Exception ex)
            {
                _logger.Error("Error disposing {0}: {1}", ClientTypeName, ex, ex.Message);
            }
        }
    }
}
