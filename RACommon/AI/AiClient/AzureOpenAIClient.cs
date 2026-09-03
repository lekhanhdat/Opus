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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Common.AI.VertexAI;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RAI.Core;
using AvePoint.RAI.Core.Services;
using Google.Cloud.AIPlatform.V1;

namespace AvePoint.RA.Common.AI.AiClient
{
    public sealed class AzureOpenAIClient : IDisposable
    {
        private static readonly Lazy<AzureOpenAIClient> _instance = new(() => new AzureOpenAIClient());
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(AzureOpenAIClient));
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private IChatCompletionService? _chatService;
        private ITextEmbeddingService? _embeddingService;
        private HttpClient? _httpClient;
        private volatile bool _isInitialized = false;
        private volatile bool _disposed = false;

        /// <summary>
        /// Client type name for logging
        /// </summary>
        private string ClientTypeName => "AzureOpenAI Client";

        /// <summary>
        /// Gets the singleton instance of the AzureOpenAI client
        /// </summary>
        public static AzureOpenAIClient Instance => _instance.Value;

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
                throw new InvalidOperationException("Chat service is not available. Ensure AZURE_OPEN_AI_CHAT_DEPLOYMENT_NAME is configured and the client is properly initialized.");
            }

            return _chatService;
        }

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
                throw new InvalidOperationException("Embedding service is not available. Ensure AZURE_OPEN_AI_TEXT_DEPLOYMENT_NAME is configured and the client is properly initialized.");
            }

            return _embeddingService;
        }

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

        private Task InitializeServiceAsync()
        {
            var chatDeploymentName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AZURE_OPEN_AI_CHAT_DEPLOYMENT_NAME];

            var embeddingDeploymentName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AZURE_OPEN_AI_TEXT_DEPLOYMENT_NAME];

            // Create development HTTP client if needed
            if (_httpClient == null)
            {
                _httpClient = CreateDevelopmentHttpClient();
            }
            var loggerAdapter = new RALoggerAdapter(_logger);


            if (!string.IsNullOrEmpty(chatDeploymentName))
            {
                InitializeChatService(chatDeploymentName, loggerAdapter);
                _logger.Info("Azure OpenAI Chat service initialized successfully - Chat model: {0}", chatDeploymentName);

            }
            else
            {
                _logger.Info("Azure OpenAI Chat service not initialized - AZURE_OPEN_AI_CHAT_DEPLOYMENT_NAME configuration not found");
            }
            if (!string.IsNullOrEmpty(embeddingDeploymentName))
            {
                InitializeEmbeddingService(embeddingDeploymentName, loggerAdapter);
                _logger.Info("Azure Open AI Embedding service initialized successfully - EmbeddingModelName model: {0}", embeddingDeploymentName);
            }
            else
            {
                _logger.Info("Azure Open Embedding service not initialized - AZURE_OPEN_AI_TEXT_DEPLOYMENT_NAME configuration not found");
            }


            _logger.Info("Azure OpenAI client initialization completed");

            return Task.CompletedTask;
        }

        private void InitializeEmbeddingService(string embeddingDeploymentName, RALoggerAdapter loggerAdapter)
        {
            _embeddingService?.Dispose();

            var embeddingProvider = CreateAzureOpenAIProvider("AzureOpenAI-Embedding", "AzureOpenAI-embedding", "embeddings");

            // Override the model for embedding service
            if (embeddingProvider.ApiServices.Count > 0)
            {
                embeddingProvider.ApiServices[0] = new ApiService(
                    Name: "embeddings",
                    ModelId: embeddingDeploymentName,
                    DeploymentName: embeddingDeploymentName
                );
            }

            // Create text embedding service
            _embeddingService = AiServiceFactory.CreateTextEmbeddingService(embeddingProvider, loggerAdapter, _httpClient);

        }

        private void InitializeChatService(string chatDeploymentName, RALoggerAdapter loggerAdapter)
        {
            // Dispose old service if exists
            _chatService?.Dispose();

            // Create AzureOpenAI provider for chat using base class method
            var chatProvider = CreateAzureOpenAIProvider("AzureOpenAI-Chat", "AzureOpenAI-chat", "chat");

            // Override the model for chat service
            if (chatProvider.ApiServices.Count > 0)
            {
                chatProvider.ApiServices[0] = new ApiService(
                    Name: "chat",
                    ModelId: chatDeploymentName,
                    DeploymentName: chatDeploymentName
                );
            }

            // Create chat completion service
            _chatService = AiServiceFactory.CreateChatCompletionService(chatProvider, loggerAdapter, _httpClient);
        }

        private AiProvider CreateAzureOpenAIProvider(string providerName, string providerCode, string serviceName)
        {
            var (modelName, apiKey, endpoint) = GetAzureOpenAIConfig();

            return AiClientUtils.CreateAiProvider(providerName, providerCode, AiProviderType.AzureOpenAI, aiProvider =>
            {
                aiProvider.APIKey = apiKey;
                aiProvider.APIEndpoint = endpoint;

                // Add service configuration
                aiProvider.ApiServices.Add(new ApiService(
                    Name: serviceName,
                    ModelId: modelName
                ));
            });
        }

        private HttpClient CreateDevelopmentHttpClient()
        {
            return AiClientUtils.CreateDevelopmentHttpClient(ClientTypeName, 5);
        }

        private (string modelName, string apiKey, string endpoint) GetAzureOpenAIConfig()
        {
            var modelName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AZURE_OPEN_AI_CHAT_DEPLOYMENT_NAME];
            var apiKey = RMGlobalConfiguration.EncryptConfig.GetAzureOpenAIAPIKey();
            var endpoint = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AZURE_OPENAI_ENDPOINT];
            return (modelName, apiKey, endpoint);
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
