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
using System.Threading.Tasks;
using AvePoint.RAI.Core.Models;

namespace AvePoint.RAI.Core.Services
{
    /// <summary>
    /// Interface for chat completion service
    /// </summary>
    public interface IChatCompletionService : IDisposable
    {
        /// <summary>
        /// Get chat completion response
        /// </summary>
        /// <param name="messages">Chat messages</param>
        /// <param name="modelId">Model ID (optional, uses default if not specified)</param>
        /// <returns>Chat completion response</returns>
        Task<ChatCompletionResponse> GetChatCompletionAsync(IEnumerable<ChatMessage> messages, string? modelId = null);

        /// <summary>
        /// Get chat completion response with custom execution settings
        /// </summary>
        /// <param name="messages">Chat messages</param>
        /// <param name="settings">Configuration settings for the chat completion request</param>
        /// <param name="modelId">Model ID (optional, uses default if not specified)</param>
        /// <returns>Chat completion response</returns>
        Task<ChatCompletionResponse> GetChatCompletionAsync(IEnumerable<ChatMessage> messages, ChatCompletionSettings settings, string? modelId = null);

        /// <summary>
        /// Get available models for chat completion
        /// </summary>
        /// <returns>List of available model IDs</returns>
        IEnumerable<string> GetAvailableModels();

        /// <summary>
        /// Get Ai provider information
        /// </summary>
        /// <returns>Ai provider</returns>
        AiProvider GetProvider();
    }

    /// <summary>
    /// Interface for text embedding service
    /// </summary>
    public interface ITextEmbeddingService : IDisposable
    {
        /// <summary>
        /// Get text embeddings
        /// </summary>
        /// <param name="texts">Input texts</param>
        /// <param name="modelId">Model ID (optional, uses default if not specified)</param>
        /// <returns>Text embeddings</returns>
        Task<EmbeddingResponse> GetEmbeddingsAsync(IEnumerable<string> texts, string? modelId = null);

        /// <summary>
        /// Get available models for text embedding
        /// </summary>
        /// <returns>List of available model IDs</returns>
        IEnumerable<string> GetAvailableModels();

        /// <summary>
        /// Get Ai provider information
        /// </summary>
        /// <returns>Ai provider</returns>
        AiProvider GetProvider();
    }
}
