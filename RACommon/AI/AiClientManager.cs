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
using AvePoint.RA.CommonUtil;
using AvePoint.RAI.Core.Services;
using AvePoint.RAI.Core.Models;
using AvePoint.RAI.Core;
using AvePoint.RA.Common.AI.VertexAI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using AvePoint.RA.Common.AI.AiClient;

namespace AvePoint.RA.Common.AI
{
    /// <summary>
    /// Centralized AI Client Manager
    /// Provides unified access to all AI service clients with singleton pattern
    /// </summary>
    public static class AiClientManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(AiClientManager));

        /// <summary>
        /// Get VertexAI Chat Service
        /// </summary>
        /// <returns>VertexAI chat completion service</returns>
        public static async Task<IChatCompletionService> GetVertexAIChatServiceAsync()
        {
            try
            {
                _logger.Info("Requesting VertexAI chat service");
                return await VertexAIClient.Instance.GetChatServiceAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get VertexAI chat service: {0}", ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get VertexAI Embedding Service
        /// </summary>
        /// <returns>VertexAI text embedding service</returns>
        public static async Task<ITextEmbeddingService> GetVertexAIEmbeddingServiceAsync()
        {
            try
            {
                _logger.Info("Requesting VertexAI embedding service");
                return await VertexAIClient.Instance.GetEmbeddingServiceAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get VertexAI embedding service: {0}", ex, ex.Message);
                throw;
            }
        }

        public static async Task<IChatCompletionService> GetAzureOpenAIChatServiceAsync()
        {
            try
            {
                _logger.Info("Requesting Azure OpenAI chat service");
                return await AiClient.AzureOpenAIClient.Instance.GetChatServiceAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get Azure OpenAI chat service: {0}", ex, ex.Message);
                throw;
            }
        }

        public static async Task<ITextEmbeddingService> GetAzureOpenAIEmbeddingServiceAsync()
        {
            try
            {
                _logger.Info("Requesting Azure OpenAI embeddings service");
                return await AiClient.AzureOpenAIClient.Instance.GetEmbeddingServiceAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get Azure OpenAI embeddings service: {0}", ex, ex.Message);
                throw;
            }
        }
    }
}
