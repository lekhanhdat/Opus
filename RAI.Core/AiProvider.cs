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

namespace AvePoint.RAI.Core
{
    /// <summary>
    /// API service record definition
    /// </summary>
    public record ApiService(string Name, string ModelId, string? DeploymentName = null, string[]? ModelIds = null);

    /// <summary>
    /// Ai Provider abstraction class
    /// </summary>
    public class AiProvider
    {
        /// <summary>
        /// Provider name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Provider code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// API key for authentication
        /// </summary>
        public string APIKey { get; set; } = string.Empty;

        /// <summary>
        /// Service account email for Google Cloud authentication
        /// </summary>
        public string? ServiceAccountEmail { get; set; }

        /// <summary>
        /// Private key for Google Cloud service account authentication
        /// </summary>
        public string? PrivateKey { get; set; }

        /// <summary>
        /// API endpoint URL
        /// </summary>
        public string APIEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Project ID (required for Google Cloud services like Vertex AI)
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Location/Region (used by Google Cloud services like Vertex AI)
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Ai provider type
        /// </summary>
        public AiProviderType AiType { get; set; }

        /// <summary>
        /// Available API services
        /// </summary>
        public List<ApiService> ApiServices { get; set; } = new List<ApiService>();

        /// <summary>
        /// Get embedding API service
        /// </summary>
        /// <returns>Embedding API service or null if not found</returns>
        public ApiService? GetEmbeddingApiService() => GetApiService("embeddings");
        

        /// <summary>
        /// Get chat completion API service
        /// </summary>
        /// <returns>Chat completion API service or null if not found</returns>
        public ApiService? GetChatCompletionApiService() => GetApiService("chat");
    
        private ApiService? GetApiService(string apiServiceName) => ApiServices.FirstOrDefault(x => x.Name == apiServiceName);
    }
}
